using System.Text;
using Microsoft.AspNetCore.Http;

namespace OSTA.API.Imports;

public sealed class CsvFileReader
{
    public async Task<string> ReadAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            throw new BomImportTemplateMappingException("A CSV file is required.");
        }

        if (file.Length <= 0)
        {
            throw new BomImportTemplateMappingException("The uploaded CSV file is empty.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new BomImportTemplateMappingException("Only .csv files are supported for upload.");
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new BomImportTemplateMappingException("The uploaded CSV file did not contain any readable content.");
        }

        return content;
    }
}
