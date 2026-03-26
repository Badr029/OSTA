using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.Parts;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/assemblies/{assemblyId:guid}/parts")]
[Route("api/v1/assemblies/{assemblyId:guid}/parts")]
public class PartsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public PartsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartResponseDto>>> GetAll(Guid assemblyId)
    {
        var assemblyExists = await _context.Assemblies.AnyAsync(x => x.Id == assemblyId);
        if (!assemblyExists)
        {
            return NotFound();
        }

        var parts = await _context.Parts
            .Where(x => x.AssemblyId == assemblyId)
            .OrderBy(x => x.PartNumber)
            .ThenBy(x => x.Revision)
            .Select(x => new PartResponseDto(x.Id, x.PartNumber, x.Revision, x.Description, x.AssemblyId))
            .ToListAsync();

        return Ok(parts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PartResponseDto>> GetById(Guid assemblyId, Guid id)
    {
        var part = await _context.Parts
            .Where(x => x.AssemblyId == assemblyId && x.Id == id)
            .Select(x => new PartResponseDto(x.Id, x.PartNumber, x.Revision, x.Description, x.AssemblyId))
            .FirstOrDefaultAsync();

        if (part is null)
        {
            return NotFound();
        }

        return Ok(part);
    }

    [HttpPost]
    public async Task<ActionResult<PartResponseDto>> Create(Guid assemblyId, CreatePartRequestDto request)
    {
        var assemblyExists = await _context.Assemblies.AnyAsync(x => x.Id == assemblyId);
        if (!assemblyExists)
        {
            return NotFound();
        }

        var duplicatePart = await _context.Parts.AnyAsync(x =>
            x.AssemblyId == assemblyId &&
            x.PartNumber == request.PartNumber &&
            x.Revision == request.Revision
        );

        if (duplicatePart)
        {
            return Conflict(
                $"A part with number '{request.PartNumber}' and revision '{request.Revision}' already exists for this assembly."
            );
        }

        var part = new Part
        {
            Id = Guid.NewGuid(),
            PartNumber = request.PartNumber,
            Revision = request.Revision,
            Description = request.Description,
            AssemblyId = assemblyId
        };

        _context.Parts.Add(part);
        await _context.SaveChangesAsync();

        var response = new PartResponseDto(
            part.Id,
            part.PartNumber,
            part.Revision,
            part.Description,
            part.AssemblyId
        );

        return CreatedAtAction(nameof(GetById), new { assemblyId, id = part.Id }, response);
    }
}
