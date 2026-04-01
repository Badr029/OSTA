using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.BomImportTemplates;
using OSTA.API.Infrastructure;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/bom-import-templates")]
public class BomImportTemplatesController : ControllerBase
{
    private readonly OstaDbContext _context;

    public BomImportTemplatesController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BomImportTemplateSummaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BomImportTemplateSummaryResponseDto>>> GetAll()
    {
        var templates = await _context.BomImportTemplates
            .OrderBy(x => x.Code)
            .Select(x => new BomImportTemplateSummaryResponseDto(
                x.Id,
                x.Code,
                x.Name,
                x.FormatType.ToString(),
                x.StructureType.ToString(),
                x.HeaderRowIndex,
                x.DataStartRowIndex,
                x.IsActive))
            .ToListAsync();

        return Ok(templates);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BomImportTemplateDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BomImportTemplateDetailResponseDto>> GetById(Guid id)
    {
        var template = await _context.BomImportTemplates
            .Where(x => x.Id == id)
            .Select(x => new BomImportTemplateDetailResponseDto(
                x.Id,
                x.Code,
                x.Name,
                x.FormatType.ToString(),
                x.StructureType.ToString(),
                x.HeaderRowIndex,
                x.DataStartRowIndex,
                x.IsActive,
                x.Notes,
                x.FieldMappings
                    .OrderBy(mapping => mapping.SortOrder)
                    .ThenBy(mapping => mapping.TargetField)
                    .Select(mapping => new BomImportTemplateFieldMappingResponseDto(
                        mapping.Id,
                        mapping.TargetField.ToString(),
                        mapping.SourceColumnName,
                        mapping.DefaultValue,
                        mapping.IsRequired,
                        mapping.SortOrder))
                    .ToList()))
            .FirstOrDefaultAsync();

        if (template is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM import template '{id}' was not found."));
        }

        return Ok(template);
    }
}
