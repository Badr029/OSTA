using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.WorkCenters;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/work-centers")]
public class WorkCentersController : ControllerBase
{
    private readonly OstaDbContext _context;

    public WorkCentersController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkCenterResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkCenterResponseDto>>> GetAll()
    {
        var workCenters = await _context.WorkCenters
            .OrderBy(x => x.Code)
            .Select(x => MapWorkCenter(x))
            .ToListAsync();

        return Ok(workCenters);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkCenterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkCenterResponseDto>> GetById(Guid id)
    {
        var workCenter = await _context.WorkCenters
            .Where(x => x.Id == id)
            .Select(x => MapWorkCenter(x))
            .FirstOrDefaultAsync();

        if (workCenter is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work center '{id}' was not found."));
        }

        return Ok(workCenter);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkCenterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkCenterResponseDto>> Create([FromBody] CreateWorkCenterRequestDto request)
    {
        var duplicateCode = await _context.WorkCenters.AnyAsync(x => x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"A work center with code '{request.Code}' already exists."));
        }

        var workCenter = new WorkCenter
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Department = request.Department.Trim(),
            HourlyRate = request.HourlyRate,
            IsActive = request.IsActive
        };

        _context.WorkCenters.Add(workCenter);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = workCenter.Id }, MapWorkCenter(workCenter));
    }

    private static WorkCenterResponseDto MapWorkCenter(WorkCenter workCenter)
    {
        return new WorkCenterResponseDto(
            workCenter.Id,
            workCenter.Code,
            workCenter.Name,
            workCenter.Department,
            workCenter.HourlyRate,
            workCenter.IsActive
        );
    }
}
