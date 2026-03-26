using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.FinishedGoods;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/finishedgoods")]
[Route("api/v1/projects/{projectId:guid}/finishedgoods")]
public class FinishedGoodsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public FinishedGoodsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinishedGoodResponseDto>>> GetAll(Guid projectId)
    {
        var projectExists = await _context.Projects.AnyAsync(x => x.Id == projectId);
        if (!projectExists)
        {
            return NotFound();
        }

        var finishedGoods = await _context.FinishedGoods
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Code)
            .Select(x => new FinishedGoodResponseDto(x.Id, x.Code, x.Name, x.ProjectId))
            .ToListAsync();

        return Ok(finishedGoods);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FinishedGoodResponseDto>> GetById(Guid projectId, Guid id)
    {
        var finishedGood = await _context.FinishedGoods
            .Where(x => x.ProjectId == projectId && x.Id == id)
            .Select(x => new FinishedGoodResponseDto(x.Id, x.Code, x.Name, x.ProjectId))
            .FirstOrDefaultAsync();

        if (finishedGood is null)
        {
            return NotFound();
        }

        return Ok(finishedGood);
    }

    [HttpPost]
    public async Task<ActionResult<FinishedGoodResponseDto>> Create(Guid projectId, CreateFinishedGoodRequestDto request)
    {
        var projectExists = await _context.Projects.AnyAsync(x => x.Id == projectId);
        if (!projectExists)
        {
            return NotFound();
        }

        var duplicateCode = await _context.FinishedGoods
            .AnyAsync(x => x.ProjectId == projectId && x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict($"A finished good with code '{request.Code}' already exists for this project.");
        }

        var finishedGood = new FinishedGood
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            ProjectId = projectId
        };

        _context.FinishedGoods.Add(finishedGood);
        await _context.SaveChangesAsync();

        var response = new FinishedGoodResponseDto(
            finishedGood.Id,
            finishedGood.Code,
            finishedGood.Name,
            finishedGood.ProjectId
        );

        return CreatedAtAction(nameof(GetById), new { projectId, id = finishedGood.Id }, response);
    }
}
