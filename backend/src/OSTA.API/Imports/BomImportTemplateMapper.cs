using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.BomImports;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Imports;

public sealed class BomImportTemplateMapper
{
    private readonly OstaDbContext _context;

    public BomImportTemplateMapper(OstaDbContext context)
    {
        _context = context;
    }

    public async Task<CreateBomImportBatchRequestDto> MapAsync(CreateBomImportBatchFromTemplateRequestDto request)
    {
        var template = await LoadTemplateDefinitionAsync(request.TemplateCode.Trim());

        if (template.FormatType is not BomImportTemplateFormatType.Csv and not BomImportTemplateFormatType.Excel)
        {
            throw new BomImportTemplateMappingException(
                $"Template '{template.TemplateCode}' is configured with unsupported format '{template.FormatType}'.");
        }

        var rows = ParseCsvRows(
            request.CsvContent,
            template.Delimiter,
            template.HeaderRowIndex,
            template.DataStartRowIndex);

        if (rows.Count == 0)
        {
            throw new BomImportTemplateMappingException("The provided CSV content did not contain any data rows.");
        }

        var effectiveDefaults = new Dictionary<BomImportTemplateTargetField, string>(template.DefaultValues);
        if (request.DefaultValues is not null)
        {
            foreach (var pair in request.DefaultValues)
            {
                if (!Enum.TryParse<BomImportTemplateTargetField>(pair.Key, true, out var targetField))
                {
                    throw new BomImportTemplateMappingException(
                        $"Default value target field '{pair.Key}' is not supported.");
                }

                effectiveDefaults[targetField] = pair.Value;
            }
        }

        if (string.Equals(template.TemplateCode, "CONVEYOR_BOM_LEVELLED_V1", StringComparison.OrdinalIgnoreCase))
        {
            return MapConveyorLevelledRows(request, rows, effectiveDefaults);
        }

        var lines = new List<CreateBomImportLineRequestDto>();

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var values = ResolveValues(template, row, effectiveDefaults);

            var rowNumber = ResolveRowNumber(values, index);
            var quantity = ResolveQuantity(values, rowNumber);

            lines.Add(new CreateBomImportLineRequestDto
            {
                RowNumber = rowNumber,
                ProjectCode = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.ProjectCode),
                ProjectName = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.ProjectName),
                FinishedGoodCode = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.FinishedGoodCode, BomImportTemplateTargetField.ParentItemCode),
                FinishedGoodName = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.FinishedGoodName, BomImportTemplateTargetField.ParentItemName),
                AssemblyCode = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.AssemblyCode, BomImportTemplateTargetField.ComponentItemCode),
                AssemblyName = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.AssemblyName, BomImportTemplateTargetField.ComponentItemName),
                PartNumber = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.PartNumber),
                Revision = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.Revision),
                Description = GetRequiredString(values, rowNumber, BomImportTemplateTargetField.Description),
                Quantity = quantity,
                MaterialCode = GetOptionalString(values, BomImportTemplateTargetField.MaterialCode),
                ThicknessMm = GetOptionalDecimal(values, rowNumber, BomImportTemplateTargetField.ThicknessMm),
                WeightKg = GetOptionalDecimal(values, rowNumber, BomImportTemplateTargetField.WeightKg),
                DrawingNumber = GetOptionalString(values, BomImportTemplateTargetField.DrawingNumber),
                FinishCode = GetOptionalString(values, BomImportTemplateTargetField.FinishCode),
                Specification = GetOptionalString(values, BomImportTemplateTargetField.Specification),
                Notes = GetOptionalString(values, BomImportTemplateTargetField.Notes),
                ProcessRouteCode = GetOptionalString(values, BomImportTemplateTargetField.ProcessRouteCode, BomImportTemplateTargetField.ProcessRoute),
                ScrapPercent = GetOptionalDecimal(values, rowNumber, BomImportTemplateTargetField.ScrapPercent),
                CutOnly = GetOptionalBoolean(values, rowNumber, BomImportTemplateTargetField.CutOnly)
            });
        }

        return new CreateBomImportBatchRequestDto
        {
            SourceFileName = request.SourceFileName.Trim(),
            ImportMode = request.ImportMode,
            Lines = lines
        };
    }

    private static CreateBomImportBatchRequestDto MapConveyorLevelledRows(
        CreateBomImportBatchFromTemplateRequestDto request,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        IReadOnlyDictionary<BomImportTemplateTargetField, string> defaultValues)
    {
        var levelZeroIndex = -1;
        string? finishedGoodCode = null;
        string? finishedGoodName = null;

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var level = GetOptionalInteger(row, "Level");
            if (level != 0)
            {
                continue;
            }

            finishedGoodCode = GetRequiredCsvValue(row, index + 1, "ComponentCode");
            finishedGoodName = GetRequiredCsvValue(row, index + 1, "Description");
            levelZeroIndex = index;
            break;
        }

        if (levelZeroIndex < 0 || string.IsNullOrWhiteSpace(finishedGoodCode) || string.IsNullOrWhiteSpace(finishedGoodName))
        {
            throw new BomImportTemplateMappingException(
                "CONVEYOR_BOM_LEVELLED_V1 requires one Level = 0 row with ComponentCode and Description to define the finished good.");
        }

        var projectCode = GetRequiredDefaultValue(defaultValues, BomImportTemplateTargetField.ProjectCode);
        var projectName = GetRequiredDefaultValue(defaultValues, BomImportTemplateTargetField.ProjectName);
        var defaultRevision = GetOptionalDefaultValue(defaultValues, BomImportTemplateTargetField.Revision) ?? "A";

        var lines = new List<CreateBomImportLineRequestDto>();

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var level = GetOptionalInteger(row, "Level");
            if (level != 1)
            {
                continue;
            }

            var rowNumber = index + 1;
            var componentCode = GetRequiredCsvValue(row, rowNumber, "ComponentCode");
            var description = GetRequiredCsvValue(row, rowNumber, "Description");
            var quantity = GetRequiredCsvDecimal(row, rowNumber, "Qty", "Quantity", "QTY");

            lines.Add(new CreateBomImportLineRequestDto
            {
                RowNumber = rowNumber,
                ProjectCode = projectCode,
                ProjectName = projectName,
                FinishedGoodCode = finishedGoodCode,
                FinishedGoodName = finishedGoodName,
                AssemblyCode = componentCode,
                AssemblyName = description,
                PartNumber = componentCode,
                Revision = GetOptionalCsvValue(row, "Revision") ?? defaultRevision,
                Description = description,
                Quantity = quantity,
                MaterialCode = GetOptionalCsvValue(row, "Material"),
                ThicknessMm = GetOptionalCsvDecimal(row, rowNumber, "ThicknessMM", "ThicknessMm", "THICKNESS_MM"),
                WeightKg = GetOptionalCsvDecimal(row, rowNumber, "WeightKG", "WeightKg", "WEIGHT_KG"),
                Notes = GetOptionalCsvValue(row, "Notes")
            });
        }

        if (lines.Count == 0)
        {
            throw new BomImportTemplateMappingException(
                "CONVEYOR_BOM_LEVELLED_V1 requires at least one Level = 1 row to generate import lines.");
        }

        return new CreateBomImportBatchRequestDto
        {
            SourceFileName = request.SourceFileName.Trim(),
            ImportMode = request.ImportMode,
            Lines = lines
        };
    }

    private async Task<BomImportTemplateDefinition> LoadTemplateDefinitionAsync(string templateCode)
    {
        var dbTemplate = await _context.BomImportTemplates
            .Include(x => x.FieldMappings)
            .Where(x => x.Code == templateCode && x.IsActive)
            .FirstOrDefaultAsync();

        if (dbTemplate is null)
        {
            return BomImportTemplateCatalog.GetRequired(templateCode);
        }

        return new BomImportTemplateDefinition(
            dbTemplate.Code,
            dbTemplate.FormatType,
            dbTemplate.StructureType,
            dbTemplate.HeaderRowIndex,
            dbTemplate.DataStartRowIndex,
            ',',
            dbTemplate.FieldMappings
                .OrderBy(x => x.SortOrder)
                .Select(x => new BomImportTemplateColumnMapping(
                    x.TargetField,
                    string.IsNullOrWhiteSpace(x.SourceColumnName) ? [] : [x.SourceColumnName],
                    x.IsRequired))
                .ToList(),
            dbTemplate.FieldMappings
                .Where(x => !string.IsNullOrWhiteSpace(x.DefaultValue))
                .ToDictionary(x => x.TargetField, x => x.DefaultValue!, EqualityComparer<BomImportTemplateTargetField>.Default)
        );
    }

    private static Dictionary<BomImportTemplateTargetField, string> ResolveValues(
        BomImportTemplateDefinition template,
        IReadOnlyDictionary<string, string> row,
        IReadOnlyDictionary<BomImportTemplateTargetField, string> defaultValues)
    {
        var values = new Dictionary<BomImportTemplateTargetField, string>();

        foreach (var mapping in template.ColumnMappings)
        {
            var matchedValue = mapping.SourceColumns
                .Select(sourceColumn => TryGetValueIgnoreCase(row, sourceColumn))
                .Select(NormalizeRawValue)
                .FirstOrDefault(value => value is not null);

            if (matchedValue is not null)
            {
                values[mapping.TargetField] = matchedValue;
                continue;
            }

            if (defaultValues.TryGetValue(mapping.TargetField, out var defaultValue))
            {
                var normalizedDefaultValue = NormalizeRawValue(defaultValue);
                if (normalizedDefaultValue is not null)
                {
                    values[mapping.TargetField] = normalizedDefaultValue;
                    continue;
                }
            }

            if (mapping.IsRequired)
            {
                throw new BomImportTemplateMappingException(
                    $"Required template field '{mapping.TargetField}' is missing from the source data and has no default value.");
            }
        }

        return values;
    }

    private static int ResolveRowNumber(IReadOnlyDictionary<BomImportTemplateTargetField, string> values, int index)
    {
        return index + 1;
    }

    private static decimal ResolveQuantity(IReadOnlyDictionary<BomImportTemplateTargetField, string> values, int rowNumber)
    {
        var quantity = GetOptionalDecimal(values, rowNumber, BomImportTemplateTargetField.Quantity);

        if (!quantity.HasValue)
        {
            throw new BomImportTemplateMappingException($"Row {rowNumber}: required field 'Quantity' is missing.");
        }

        return quantity.Value;
    }

    private static string GetRequiredString(
        IReadOnlyDictionary<BomImportTemplateTargetField, string> values,
        int rowNumber,
        params BomImportTemplateTargetField[] targetFields)
    {
        foreach (var field in targetFields)
        {
            if (values.TryGetValue(field, out var value))
            {
                var normalized = NormalizeRawValue(value);
                if (normalized is not null)
                {
                    return normalized;
                }
            }
        }

        throw new BomImportTemplateMappingException(
            $"Row {rowNumber}: required field '{string.Join(" / ", targetFields.Select(x => x.ToString()))}' is missing.");
    }

    private static string? GetOptionalString(
        IReadOnlyDictionary<BomImportTemplateTargetField, string> values,
        params BomImportTemplateTargetField[] targetFields)
    {
        foreach (var field in targetFields)
        {
            if (values.TryGetValue(field, out var value))
            {
                var normalized = NormalizeRawValue(value);
                if (normalized is not null)
                {
                    return normalized;
                }
            }
        }

        return null;
    }

    private static decimal? GetOptionalDecimal(
        IReadOnlyDictionary<BomImportTemplateTargetField, string> values,
        int rowNumber,
        params BomImportTemplateTargetField[] targetFields)
    {
        var text = GetOptionalString(values, targetFields);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return number;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out number))
        {
            return number;
        }

        throw new BomImportTemplateMappingException(
            $"Row {rowNumber}: value '{text}' for field '{string.Join(" / ", targetFields.Select(x => x.ToString()))}' is not a valid decimal number.");
    }

    private static bool? GetOptionalBoolean(
        IReadOnlyDictionary<BomImportTemplateTargetField, string> values,
        int rowNumber,
        params BomImportTemplateTargetField[] targetFields)
    {
        var text = GetOptionalString(values, targetFields);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (bool.TryParse(text, out var boolean))
        {
            return boolean;
        }

        if (text is "1" or "Y" or "y" or "Yes" or "YES")
        {
            return true;
        }

        if (text is "0" or "N" or "n" or "No" or "NO")
        {
            return false;
        }

        throw new BomImportTemplateMappingException(
            $"Row {rowNumber}: value '{text}' for field '{string.Join(" / ", targetFields.Select(x => x.ToString()))}' is not a valid boolean.");
    }

    private static List<IReadOnlyDictionary<string, string>> ParseCsvRows(
        string csvContent,
        char delimiter,
        int headerRowIndex,
        int dataStartRowIndex)
    {
        var records = ParseCsvRecords(csvContent, delimiter);
        var headerIndex = headerRowIndex - 1;
        var dataIndex = dataStartRowIndex - 1;

        if (records.Count <= headerIndex || records.Count <= dataIndex)
        {
            return [];
        }

        var headers = records[headerIndex]
            .Select(header => header.Trim())
            .ToList();

        var rows = new List<IReadOnlyDictionary<string, string>>();

        for (var i = dataIndex; i < records.Count; i++)
        {
            var record = records[i];
            if (record.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                var header = headers[columnIndex];
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                row[header] = columnIndex < record.Count ? record[columnIndex].Trim() : string.Empty;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<List<string>> ParseCsvRecords(string csvContent, char delimiter)
    {
        var records = new List<List<string>>();
        var currentRecord = new List<string>();
        var currentField = new List<char>();
        var inQuotes = false;

        for (var i = 0; i < csvContent.Length; i++)
        {
            var ch = csvContent[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    var isEscapedQuote = i + 1 < csvContent.Length && csvContent[i + 1] == '"';
                    if (isEscapedQuote)
                    {
                        currentField.Add('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField.Add(ch);
                }

                continue;
            }

            if (ch == '"')
            {
                inQuotes = true;
                continue;
            }

            if (ch == delimiter)
            {
                currentRecord.Add(new string(currentField.ToArray()));
                currentField.Clear();
                continue;
            }

            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                currentRecord.Add(new string(currentField.ToArray()));
                currentField.Clear();
                records.Add(currentRecord);
                currentRecord = new List<string>();
                continue;
            }

            currentField.Add(ch);
        }

        currentRecord.Add(new string(currentField.ToArray()));
        records.Add(currentRecord);

        return records;
    }

    private static string? TryGetValueIgnoreCase(IReadOnlyDictionary<string, string> row, string sourceColumn)
    {
        return row.TryGetValue(sourceColumn, out var value)
            ? value
            : row.FirstOrDefault(x => string.Equals(x.Key, sourceColumn, StringComparison.OrdinalIgnoreCase)).Value;
    }

    private static string? NormalizeRawValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.ToLowerInvariant() switch
        {
            "null" => null,
            "n/a" => null,
            "na" => null,
            "-" => null,
            _ => trimmed
        };
    }

    private static string GetRequiredDefaultValue(
        IReadOnlyDictionary<BomImportTemplateTargetField, string> defaultValues,
        BomImportTemplateTargetField field)
    {
        if (defaultValues.TryGetValue(field, out var value))
        {
            var normalized = NormalizeRawValue(value);
            if (normalized is not null)
            {
                return normalized;
            }
        }

        throw new BomImportTemplateMappingException(
            $"Required template field '{field}' is missing from the source data and has no default value.");
    }

    private static string? GetOptionalDefaultValue(
        IReadOnlyDictionary<BomImportTemplateTargetField, string> defaultValues,
        BomImportTemplateTargetField field)
    {
        if (!defaultValues.TryGetValue(field, out var value))
        {
            return null;
        }

        return NormalizeRawValue(value);
    }

    private static string GetRequiredCsvValue(
        IReadOnlyDictionary<string, string> row,
        int rowNumber,
        params string[] headers)
    {
        var value = GetOptionalCsvValue(row, headers);
        if (value is not null)
        {
            return value;
        }

        throw new BomImportTemplateMappingException(
            $"Row {rowNumber}: required field '{string.Join(" / ", headers)}' is missing.");
    }

    private static string? GetOptionalCsvValue(
        IReadOnlyDictionary<string, string> row,
        params string[] headers)
    {
        foreach (var header in headers)
        {
            var value = TryGetValueIgnoreCase(row, header);
            var normalized = NormalizeRawValue(value);
            if (normalized is not null)
            {
                return normalized;
            }
        }

        return null;
    }

    private static decimal GetRequiredCsvDecimal(
        IReadOnlyDictionary<string, string> row,
        int rowNumber,
        params string[] headers)
    {
        var value = GetOptionalCsvDecimal(row, rowNumber, headers);
        if (value.HasValue)
        {
            return value.Value;
        }

        throw new BomImportTemplateMappingException(
            $"Row {rowNumber}: required field '{string.Join(" / ", headers)}' is missing.");
    }

    private static decimal? GetOptionalCsvDecimal(
        IReadOnlyDictionary<string, string> row,
        int rowNumber,
        params string[] headers)
    {
        var text = GetOptionalCsvValue(row, headers);
        if (text is null)
        {
            return null;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return number;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out number))
        {
            return number;
        }

        throw new BomImportTemplateMappingException(
            $"Row {rowNumber}: value '{text}' for field '{string.Join(" / ", headers)}' is not a valid decimal number.");
    }

    private static int? GetOptionalInteger(
        IReadOnlyDictionary<string, string> row,
        params string[] headers)
    {
        var text = GetOptionalCsvValue(row, headers);
        if (text is null)
        {
            return null;
        }

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            return value;
        }

        return null;
    }
}
