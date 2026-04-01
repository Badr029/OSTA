using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.RoutingTemplates;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/routing-templates")]
public class RoutingTemplatesController : ControllerBase
{
    private readonly OstaDbContext _context;

    public RoutingTemplatesController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoutingTemplateResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoutingTemplateResponseDto>>> GetAll()
    {
        var routingTemplates = await _context.RoutingTemplates
            .OrderBy(x => x.ItemMasterId)
            .ThenBy(x => x.Code)
            .ThenBy(x => x.Revision)
            .Select(x => MapRoutingTemplate(x))
            .ToListAsync();

        return Ok(routingTemplates);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoutingTemplateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutingTemplateResponseDto>> GetById(Guid id)
    {
        var routingTemplate = await _context.RoutingTemplates
            .Where(x => x.Id == id)
            .Select(x => MapRoutingTemplate(x))
            .FirstOrDefaultAsync();

        if (routingTemplate is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Routing template '{id}' was not found."));
        }

        return Ok(routingTemplate);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoutingTemplateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoutingTemplateResponseDto>> Create([FromBody] CreateRoutingTemplateRequestDto request)
    {
        var itemMasterExists = await _context.ItemMasters.AnyAsync(x => x.Id == request.ItemMasterId);

        if (!itemMasterExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item master '{request.ItemMasterId}' was not found."));
        }

        var duplicateTemplate = await _context.RoutingTemplates.AnyAsync(x =>
            x.ItemMasterId == request.ItemMasterId &&
            x.Code == request.Code &&
            x.Revision == request.Revision);

        if (duplicateTemplate)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict(
                $"A routing template with code '{request.Code}' and revision '{request.Revision}' already exists for item master '{request.ItemMasterId}'."));
        }

        var routingTemplate = new RoutingTemplate
        {
            Id = Guid.NewGuid(),
            ItemMasterId = request.ItemMasterId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Revision = request.Revision.Trim(),
            Status = request.Status,
            IsActive = request.IsActive
        };

        _context.RoutingTemplates.Add(routingTemplate);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = routingTemplate.Id }, MapRoutingTemplate(routingTemplate));
    }

    private static RoutingTemplateResponseDto MapRoutingTemplate(RoutingTemplate routingTemplate)
    {
        return new RoutingTemplateResponseDto(
            routingTemplate.Id,
            routingTemplate.ItemMasterId,
            routingTemplate.Code,
            routingTemplate.Name,
            routingTemplate.Revision,
            routingTemplate.Status.ToString(),
            routingTemplate.IsActive
        );
    }
}
