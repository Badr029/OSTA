using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.ItemMasters;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/item-masters")]
public class ItemMastersController : ControllerBase
{
    private readonly OstaDbContext _context;

    public ItemMastersController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemMasterResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ItemMasterResponseDto>>> GetAll()
    {
        var itemMasters = await _context.ItemMasters
            .OrderBy(x => x.Code)
            .Select(x => MapItemMaster(x))
            .ToListAsync();

        return Ok(itemMasters);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemMasterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemMasterResponseDto>> GetById(Guid id)
    {
        var itemMaster = await _context.ItemMasters
            .Where(x => x.Id == id)
            .Select(x => MapItemMaster(x))
            .FirstOrDefaultAsync();

        if (itemMaster is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item master '{id}' was not found."));
        }

        return Ok(itemMaster);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemMasterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ItemMasterResponseDto>> Create([FromBody] CreateItemMasterRequestDto request)
    {
        var duplicateCode = await _context.ItemMasters.AnyAsync(x => x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"An item master with code '{request.Code}' already exists."));
        }

        var itemMaster = new ItemMaster
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            MaterialCode = NormalizeOptional(request.MaterialCode),
            ThicknessMm = request.ThicknessMm,
            WeightKg = request.WeightKg,
            LengthMm = request.LengthMm,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            DrawingNumber = NormalizeOptional(request.DrawingNumber),
            FinishCode = NormalizeOptional(request.FinishCode),
            Specification = NormalizeOptional(request.Specification),
            Notes = NormalizeOptional(request.Notes),
            ItemType = request.ItemType,
            ProcurementType = request.ProcurementType,
            BaseUom = request.BaseUom.Trim(),
            Revision = request.Revision.Trim(),
            IsActive = request.IsActive
        };

        _context.ItemMasters.Add(itemMaster);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = itemMaster.Id }, MapItemMaster(itemMaster));
    }

    private static ItemMasterResponseDto MapItemMaster(ItemMaster itemMaster)
    {
        return new ItemMasterResponseDto(
            itemMaster.Id,
            itemMaster.Code,
            itemMaster.Name,
            itemMaster.Description,
            itemMaster.MaterialCode,
            itemMaster.ThicknessMm,
            itemMaster.WeightKg,
            itemMaster.LengthMm,
            itemMaster.WidthMm,
            itemMaster.HeightMm,
            itemMaster.DrawingNumber,
            itemMaster.FinishCode,
            itemMaster.Specification,
            itemMaster.Notes,
            itemMaster.ItemType.ToString(),
            itemMaster.ProcurementType.ToString(),
            itemMaster.BaseUom,
            itemMaster.Revision,
            itemMaster.IsActive
        );
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
