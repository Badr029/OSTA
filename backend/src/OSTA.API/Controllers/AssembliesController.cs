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
}
