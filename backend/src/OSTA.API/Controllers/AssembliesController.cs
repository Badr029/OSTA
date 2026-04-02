using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.Assemblies;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/finishedgoods/{finishedGoodId:guid}/assemblies")]
public class AssembliesController : ControllerBase
{
    private readonly OstaDbContext _context;

    public AssembliesController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssemblyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AssemblyResponseDto>>> GetAll(Guid finishedGoodId)
    {
        var finishedGoodExists = await _context.FinishedGoods.AnyAsync(x => x.Id == finishedGoodId);
        if (!finishedGoodExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Finished good '{finishedGoodId}' was not found."));
        }

        var assemblies = await _context.Assemblies
            .Where(x => x.FinishedGoodId == finishedGoodId)
            .OrderBy(x => x.Code)
            .Select(x => new AssemblyResponseDto(
                x.Id,
                x.Code,
                x.Name,
                x.FinishedGoodId,
                x.SourceBomItemId,
                x.SourceComponentItemMasterId))
            .ToListAsync();

        return Ok(assemblies);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssemblyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssemblyResponseDto>> GetById(Guid finishedGoodId, Guid id)
    {
        var assembly = await _context.Assemblies
            .Where(x => x.FinishedGoodId == finishedGoodId && x.Id == id)
            .Select(x => new AssemblyResponseDto(
                x.Id,
                x.Code,
                x.Name,
                x.FinishedGoodId,
                x.SourceBomItemId,
                x.SourceComponentItemMasterId))
            .FirstOrDefaultAsync();

        if (assembly is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Assembly '{id}' was not found under finished good '{finishedGoodId}'."));
        }

        return Ok(assembly);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssemblyResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssemblyResponseDto>> Create(Guid finishedGoodId, [FromBody] CreateAssemblyRequestDto request)
    {
        var finishedGoodExists = await _context.FinishedGoods.AnyAsync(x => x.Id == finishedGoodId);
        if (!finishedGoodExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Finished good '{finishedGoodId}' was not found."));
        }

        var duplicateCode = await _context.Assemblies
            .AnyAsync(x => x.FinishedGoodId == finishedGoodId && x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"An assembly with code '{request.Code}' already exists for finished good '{finishedGoodId}'."));
        }

        var assembly = new Assembly
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            FinishedGoodId = finishedGoodId
        };

        _context.Assemblies.Add(assembly);
        await _context.SaveChangesAsync();

        var response = new AssemblyResponseDto(
            assembly.Id,
            assembly.Code,
            assembly.Name,
            assembly.FinishedGoodId,
            assembly.SourceBomItemId,
            assembly.SourceComponentItemMasterId
        );

        return CreatedAtAction(nameof(GetById), new { finishedGoodId, id = assembly.Id }, response);
    }

    [HttpGet("/api/v1/assemblies/{assemblyId:guid}/material-readiness")]
    [ProducesResponseType(typeof(AssemblyMaterialReadinessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssemblyMaterialReadinessResponseDto>> GetMaterialReadiness(Guid assemblyId)
    {
        var assembly = await _context.Assemblies
            .Where(x => x.Id == assemblyId)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.SourceComponentItemMasterId,
                SourceComponentItemCode = x.SourceComponentItemMaster != null
                    ? x.SourceComponentItemMaster.Code
                    : null
            })
            .FirstOrDefaultAsync();

        if (assembly is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Assembly '{assemblyId}' was not found."));
        }

        var missingReasons = new List<string>();

        if (assembly.SourceComponentItemMasterId is null)
        {
            missingReasons.Add("Assembly is not linked to a source component item master.");

            return Ok(new AssemblyMaterialReadinessResponseDto(
                assembly.Id,
                assembly.Code,
                null,
                null,
                false,
                0,
                missingReasons,
                Array.Empty<AssemblyMaterialReadinessRequirementResponseDto>()));
        }

        var requirements = await _context.ItemMaterialRequirements
            .Where(x => x.ItemMasterId == assembly.SourceComponentItemMasterId)
            .OrderBy(x => x.MaterialCode)
            .ThenBy(x => x.Id)
            .Select(x => new AssemblyMaterialReadinessRequirementResponseDto(
                x.Id,
                x.MaterialCode,
                x.RequiredQuantity,
                x.Uom,
                x.ThicknessMm,
                x.LengthMm,
                x.WidthMm,
                x.WeightKg,
                x.Notes))
            .ToListAsync();

        if (requirements.Count == 0)
        {
            missingReasons.Add("No material requirements defined for the linked item master.");
        }

        return Ok(new AssemblyMaterialReadinessResponseDto(
            assembly.Id,
            assembly.Code,
            assembly.SourceComponentItemMasterId,
            assembly.SourceComponentItemCode,
            requirements.Count > 0,
            requirements.Count,
            missingReasons,
            requirements));
    }
}
