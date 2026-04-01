using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.FinishedGoods;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/projects/{projectId:guid}/finishedgoods")]
public class FinishedGoodsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public FinishedGoodsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FinishedGoodResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<FinishedGoodResponseDto>>> GetAll(Guid projectId)
    {
        var projectExists = await _context.Projects.AnyAsync(x => x.Id == projectId);
        if (!projectExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Project '{projectId}' was not found."));
        }

        var finishedGoods = await _context.FinishedGoods
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Code)
            .Select(x => new FinishedGoodResponseDto(
                x.Id,
                x.Code,
                x.Name,
                x.ProjectId,
                x.SourceItemMasterId,
                x.SourceBomHeaderId))
            .ToListAsync();

        return Ok(finishedGoods);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FinishedGoodResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FinishedGoodResponseDto>> GetById(Guid projectId, Guid id)
    {
        var finishedGood = await _context.FinishedGoods
            .Where(x => x.ProjectId == projectId && x.Id == id)
            .Select(x => new FinishedGoodResponseDto(
                x.Id,
                x.Code,
                x.Name,
                x.ProjectId,
                x.SourceItemMasterId,
                x.SourceBomHeaderId))
            .FirstOrDefaultAsync();

        if (finishedGood is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Finished good '{id}' was not found under project '{projectId}'."));
        }

        return Ok(finishedGood);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FinishedGoodResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FinishedGoodResponseDto>> Create(Guid projectId, [FromBody] CreateFinishedGoodRequestDto request)
    {
        var projectExists = await _context.Projects.AnyAsync(x => x.Id == projectId);
        if (!projectExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Project '{projectId}' was not found."));
        }

        var duplicateCode = await _context.FinishedGoods
            .AnyAsync(x => x.ProjectId == projectId && x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"A finished good with code '{request.Code}' already exists for project '{projectId}'."));
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
            finishedGood.ProjectId,
            finishedGood.SourceItemMasterId,
            finishedGood.SourceBomHeaderId
        );

        return CreatedAtAction(nameof(GetById), new { projectId, id = finishedGood.Id }, response);
    }
}
