using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.ItemMaterialRequirements;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
public class ItemMaterialRequirementsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public ItemMaterialRequirementsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/v1/item-masters/{itemMasterId:guid}/material-requirements")]
    [ProducesResponseType(typeof(IEnumerable<ItemMaterialRequirementResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ItemMaterialRequirementResponseDto>>> GetAll(Guid itemMasterId)
    {
        var itemMasterExists = await _context.ItemMasters.AnyAsync(x => x.Id == itemMasterId);

        if (!itemMasterExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item master '{itemMasterId}' was not found."));
        }

        var requirements = await _context.ItemMaterialRequirements
            .Where(x => x.ItemMasterId == itemMasterId)
            .OrderBy(x => x.MaterialCode)
            .ThenBy(x => x.Id)
            .Select(x => MapRequirement(x))
            .ToListAsync();

        return Ok(requirements);
    }

    [HttpGet("api/v1/item-material-requirements/{id:guid}")]
    [ProducesResponseType(typeof(ItemMaterialRequirementResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemMaterialRequirementResponseDto>> GetById(Guid id)
    {
        var requirement = await _context.ItemMaterialRequirements
            .Where(x => x.Id == id)
            .Select(x => MapRequirement(x))
            .FirstOrDefaultAsync();

        if (requirement is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item material requirement '{id}' was not found."));
        }

        return Ok(requirement);
    }

    [HttpPost("api/v1/item-masters/{itemMasterId:guid}/material-requirements")]
    [ProducesResponseType(typeof(ItemMaterialRequirementResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemMaterialRequirementResponseDto>> Create(
        Guid itemMasterId,
        [FromBody] CreateItemMaterialRequirementRequestDto request)
    {
        var itemMasterExists = await _context.ItemMasters.AnyAsync(x => x.Id == itemMasterId);

        if (!itemMasterExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Item master '{itemMasterId}' was not found."));
        }

        var requirement = new ItemMaterialRequirement
        {
            Id = Guid.NewGuid(),
            ItemMasterId = itemMasterId,
            MaterialCode = request.MaterialCode.Trim(),
            RequiredQuantity = request.RequiredQuantity,
            Uom = request.Uom.Trim(),
            ThicknessMm = request.ThicknessMm,
            LengthMm = request.LengthMm,
            WidthMm = request.WidthMm,
            WeightKg = request.WeightKg,
            Notes = NormalizeOptional(request.Notes)
        };

        _context.ItemMaterialRequirements.Add(requirement);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = requirement.Id },
            MapRequirement(requirement));
    }

    private static ItemMaterialRequirementResponseDto MapRequirement(ItemMaterialRequirement requirement)
    {
        return new ItemMaterialRequirementResponseDto(
            requirement.Id,
            requirement.ItemMasterId,
            requirement.MaterialCode,
            requirement.RequiredQuantity,
            requirement.Uom,
            requirement.ThicknessMm,
            requirement.LengthMm,
            requirement.WidthMm,
            requirement.WeightKg,
            requirement.Notes
        );
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
