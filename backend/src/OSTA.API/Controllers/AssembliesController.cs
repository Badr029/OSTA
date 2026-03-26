using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.Assemblies;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/finishedgoods/{finishedGoodId:guid}/assemblies")]
[Route("api/v1/finishedgoods/{finishedGoodId:guid}/assemblies")]
public class AssembliesController : ControllerBase
{
    private readonly OstaDbContext _context;

    public AssembliesController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssemblyResponseDto>>> GetAll(Guid finishedGoodId)
    {
        var finishedGoodExists = await _context.FinishedGoods.AnyAsync(x => x.Id == finishedGoodId);
        if (!finishedGoodExists)
        {
            return NotFound();
        }

        var assemblies = await _context.Assemblies
            .Where(x => x.FinishedGoodId == finishedGoodId)
            .OrderBy(x => x.Code)
            .Select(x => new AssemblyResponseDto(x.Id, x.Code, x.Name, x.FinishedGoodId))
            .ToListAsync();

        return Ok(assemblies);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssemblyResponseDto>> GetById(Guid finishedGoodId, Guid id)
    {
        var assembly = await _context.Assemblies
            .Where(x => x.FinishedGoodId == finishedGoodId && x.Id == id)
            .Select(x => new AssemblyResponseDto(x.Id, x.Code, x.Name, x.FinishedGoodId))
            .FirstOrDefaultAsync();

        if (assembly is null)
        {
            return NotFound();
        }

        return Ok(assembly);
    }

    [HttpPost]
    public async Task<ActionResult<AssemblyResponseDto>> Create(Guid finishedGoodId, CreateAssemblyRequestDto request)
    {
        var finishedGoodExists = await _context.FinishedGoods.AnyAsync(x => x.Id == finishedGoodId);
        if (!finishedGoodExists)
        {
            return NotFound();
        }

        var duplicateCode = await _context.Assemblies
            .AnyAsync(x => x.FinishedGoodId == finishedGoodId && x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict($"An assembly with code '{request.Code}' already exists for this finished good.");
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
            assembly.FinishedGoodId
        );

        return CreatedAtAction(nameof(GetById), new { finishedGoodId, id = assembly.Id }, response);
    }
}
