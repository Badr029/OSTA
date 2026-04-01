using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.BomHeaders;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/bom-headers")]
public class BomHeadersController : ControllerBase
{
    private readonly OstaDbContext _context;

    public BomHeadersController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BomHeaderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BomHeaderResponseDto>>> GetAll()
    {
        var bomHeaders = await _context.BomHeaders
            .OrderBy(x => x.ParentItemMasterId)
            .ThenBy(x => x.Revision)
            .ThenBy(x => x.Usage)
            .ThenBy(x => x.PlantCode)
            .Select(x => MapBomHeader(x))
            .ToListAsync();

        return Ok(bomHeaders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BomHeaderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BomHeaderResponseDto>> GetById(Guid id)
    {
        var bomHeader = await _context.BomHeaders
            .Where(x => x.Id == id)
            .Select(x => MapBomHeader(x))
            .FirstOrDefaultAsync();

        if (bomHeader is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM header '{id}' was not found."));
        }

        return Ok(bomHeader);
    }

    [HttpGet("{id:guid}/structure")]
    [ProducesResponseType(typeof(BomHeaderStructureResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BomHeaderStructureResponseDto>> GetStructure(Guid id)
    {
        var bomHeader = await _context.BomHeaders
            .Where(x => x.Id == id)
            .Select(x => new BomHeaderStructureResponseDto(
                x.Id,
                x.ParentItemMasterId,
                x.ParentItemMaster.Code,
                x.ParentItemMaster.Name,
                x.ParentItemMaster.ItemType.ToString(),
                x.Revision,
                x.BaseQuantity,
                x.Usage.ToString(),
                x.Status.ToString(),
                x.PlantCode,
                x.Items
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.ItemNumber)
                    .Select(item => new BomHeaderStructureItemResponseDto(
                        item.Id,
                        item.ItemNumber,
                        item.ComponentItemMasterId,
                        item.ComponentItemMaster.Code,
                        item.ComponentItemMaster.Name,
                        item.ComponentItemMaster.ItemType.ToString(),
                        item.Quantity,
                        item.Uom,
                        item.ItemCategory.ToString(),
                        item.ProcurementType.ToString(),
                        item.ScrapPercent,
                        item.ProcessRouteCode,
                        item.PositionText,
                        item.LineNotes,
                        item.IsPhantom,
                        item.IsBulk,
                        item.CutOnly,
                        item.SortOrder))
                    .ToList()))
            .FirstOrDefaultAsync();

        if (bomHeader is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM header '{id}' was not found."));
        }

        return Ok(bomHeader);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BomHeaderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BomHeaderResponseDto>> Create([FromBody] CreateBomHeaderRequestDto request)
    {
        var parentItemExists = await _context.ItemMasters.AnyAsync(x => x.Id == request.ParentItemMasterId);

        if (!parentItemExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item master '{request.ParentItemMasterId}' was not found."));
        }

        var duplicateHeader = await _context.BomHeaders.AnyAsync(x =>
            x.ParentItemMasterId == request.ParentItemMasterId &&
            x.Revision == request.Revision &&
            x.Usage == request.Usage &&
            x.PlantCode == request.PlantCode);

        if (duplicateHeader)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict(
                $"A BOM header already exists for item master '{request.ParentItemMasterId}' revision '{request.Revision}', usage '{request.Usage}', and plant '{request.PlantCode}'."));
        }

        var bomHeader = new BomHeader
        {
            Id = Guid.NewGuid(),
            ParentItemMasterId = request.ParentItemMasterId,
            Revision = request.Revision.Trim(),
            BaseQuantity = request.BaseQuantity,
            Usage = request.Usage,
            Status = request.Status,
            PlantCode = request.PlantCode.Trim()
        };

        _context.BomHeaders.Add(bomHeader);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = bomHeader.Id }, MapBomHeader(bomHeader));
    }

    private static BomHeaderResponseDto MapBomHeader(BomHeader bomHeader)
    {
        return new BomHeaderResponseDto(
            bomHeader.Id,
            bomHeader.ParentItemMasterId,
            bomHeader.Revision,
            bomHeader.BaseQuantity,
            bomHeader.Usage.ToString(),
            bomHeader.Status.ToString(),
            bomHeader.PlantCode
        );
    }
}
