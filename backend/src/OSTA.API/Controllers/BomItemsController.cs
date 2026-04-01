using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.BomItems;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/bom-headers/{bomHeaderId:guid}/items")]
public class BomItemsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public BomItemsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BomItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<BomItemResponseDto>>> GetAll(Guid bomHeaderId)
    {
        var bomHeaderExists = await _context.BomHeaders.AnyAsync(x => x.Id == bomHeaderId);

        if (!bomHeaderExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM header '{bomHeaderId}' was not found."));
        }

        var bomItems = await _context.BomItems
            .Where(x => x.BomHeaderId == bomHeaderId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ItemNumber)
            .Select(x => new BomItemResponseDto(
                x.Id,
                x.BomHeaderId,
                x.ItemNumber,
                x.ComponentItemMasterId,
                x.ComponentItemMaster.Code,
                x.ComponentItemMaster.Name,
                x.Quantity,
                x.Uom,
                x.ItemCategory.ToString(),
                x.ProcurementType.ToString(),
                x.ScrapPercent,
                x.ProcessRouteCode,
                x.PositionText,
                x.LineNotes,
                x.IsPhantom,
                x.IsBulk,
                x.CutOnly,
                x.SortOrder))
            .ToListAsync();

        return Ok(bomItems);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BomItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BomItemResponseDto>> GetById(Guid bomHeaderId, Guid id)
    {
        var bomItem = await _context.BomItems
            .Where(x => x.BomHeaderId == bomHeaderId && x.Id == id)
            .Select(x => new BomItemResponseDto(
                x.Id,
                x.BomHeaderId,
                x.ItemNumber,
                x.ComponentItemMasterId,
                x.ComponentItemMaster.Code,
                x.ComponentItemMaster.Name,
                x.Quantity,
                x.Uom,
                x.ItemCategory.ToString(),
                x.ProcurementType.ToString(),
                x.ScrapPercent,
                x.ProcessRouteCode,
                x.PositionText,
                x.LineNotes,
                x.IsPhantom,
                x.IsBulk,
                x.CutOnly,
                x.SortOrder))
            .FirstOrDefaultAsync();

        if (bomItem is null)
        {
            var bomHeaderExists = await _context.BomHeaders.AnyAsync(x => x.Id == bomHeaderId);

            if (!bomHeaderExists)
            {
                return NotFound(ApiProblemDetailsFactory.NotFound($"BOM header '{bomHeaderId}' was not found."));
            }

            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM item '{id}' was not found under BOM header '{bomHeaderId}'."));
        }

        return Ok(bomItem);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BomItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BomItemResponseDto>> Create(Guid bomHeaderId, [FromBody] CreateBomItemRequestDto request)
    {
        var itemNumber = request.ItemNumber.Trim();
        var uom = request.Uom.Trim();

        var bomHeaderExists = await _context.BomHeaders.AnyAsync(x => x.Id == bomHeaderId);

        if (!bomHeaderExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM header '{bomHeaderId}' was not found."));
        }

        var componentItemExists = await _context.ItemMasters.AnyAsync(x => x.Id == request.ComponentItemMasterId);

        if (!componentItemExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item master '{request.ComponentItemMasterId}' was not found."));
        }

        var duplicateItemNumber = await _context.BomItems.AnyAsync(x =>
            x.BomHeaderId == bomHeaderId &&
            x.ItemNumber == itemNumber);

        if (duplicateItemNumber)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict(
                $"A BOM item with item number '{itemNumber}' already exists under BOM header '{bomHeaderId}'."));
        }

        var bomItem = new BomItem
        {
            Id = Guid.NewGuid(),
            BomHeaderId = bomHeaderId,
            ItemNumber = itemNumber,
            ComponentItemMasterId = request.ComponentItemMasterId,
            Quantity = request.Quantity,
            Uom = uom,
            ItemCategory = request.ItemCategory,
            ProcurementType = request.ProcurementType,
            ScrapPercent = request.ScrapPercent,
            ProcessRouteCode = NormalizeOptional(request.ProcessRouteCode),
            PositionText = NormalizeOptional(request.PositionText),
            LineNotes = NormalizeOptional(request.LineNotes),
            IsPhantom = request.IsPhantom,
            IsBulk = request.IsBulk,
            CutOnly = request.CutOnly,
            SortOrder = request.SortOrder
        };

        _context.BomItems.Add(bomItem);
        await _context.SaveChangesAsync();

        var response = await _context.BomItems
            .Where(x => x.Id == bomItem.Id)
            .Select(x => new BomItemResponseDto(
                x.Id,
                x.BomHeaderId,
                x.ItemNumber,
                x.ComponentItemMasterId,
                x.ComponentItemMaster.Code,
                x.ComponentItemMaster.Name,
                x.Quantity,
                x.Uom,
                x.ItemCategory.ToString(),
                x.ProcurementType.ToString(),
                x.ScrapPercent,
                x.ProcessRouteCode,
                x.PositionText,
                x.LineNotes,
                x.IsPhantom,
                x.IsBulk,
                x.CutOnly,
                x.SortOrder))
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { bomHeaderId, id = bomItem.Id }, response);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
