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

    [HttpGet("{id:guid}/queue")]
    [ProducesResponseType(typeof(IEnumerable<WorkCenterQueueItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<WorkCenterQueueItemDto>>> GetQueue(Guid id)
    {
        var workCenterExists = await _context.WorkCenters.AnyAsync(x => x.Id == id);

        if (!workCenterExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work center '{id}' was not found."));
        }

        var queueItems = await _context.WorkOrderOperations
            .Where(x => x.WorkCenterId == id)
            .Where(x =>
                (x.WorkOrder.Status == WorkOrderStatus.Released || x.WorkOrder.Status == WorkOrderStatus.InProgress) &&
                (x.Status == WorkOrderOperationStatus.Ready || x.Status == WorkOrderOperationStatus.InProgress))
            .Select(x => new WorkCenterQueueItemDto(
                x.WorkOrderId,
                x.WorkOrder.WorkOrderNumber,
                x.WorkOrder.Status.ToString(),
                x.WorkOrder.Project.Code,
                x.WorkOrder.FinishedGood.Code,
                x.WorkOrder.Assembly.Code,
                x.Id,
                x.OperationNumber,
                x.OperationCode,
                x.OperationName,
                x.Status.ToString(),
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.WorkOrder.ReleasedAtUtc,
                x.StartedAtUtc,
                x.IsQcGate,
                x.Sequence))
            .ToListAsync();

        var orderedQueueItems = queueItems
            .OrderBy(x => x.OperationStatus == WorkOrderOperationStatus.InProgress.ToString() ? 0 : 1)
            .ThenBy(x => x.ReleasedAtUtc ?? DateTime.MaxValue)
            .ThenBy(x => x.WorkOrderNumber)
            .ThenBy(x => x.Sequence)
            .ToList();

        return Ok(orderedQueueItems);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkCenterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkCenterResponseDto>> Create([FromBody] CreateWorkCenterRequestDto request)
    {
        var normalizedCode = request.Code.Trim();
        var duplicateCode = await _context.WorkCenters.AnyAsync(x => x.Code == normalizedCode);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"A work center with code '{normalizedCode}' already exists."));
        }

        var workCenter = new WorkCenter
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Department = request.Department.Trim(),
            HourlyRate = request.HourlyRate,
            IsActive = request.IsActive
        };

        _context.WorkCenters.Add(workCenter);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = workCenter.Id }, MapWorkCenter(workCenter));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkCenterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkCenterResponseDto>> Update(Guid id, [FromBody] UpdateWorkCenterRequestDto request)
    {
        var workCenter = await _context.WorkCenters.FirstOrDefaultAsync(x => x.Id == id);

        if (workCenter is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work center '{id}' was not found."));
        }

        var normalizedCode = request.Code.Trim();
        var duplicateCode = await _context.WorkCenters.AnyAsync(x => x.Id != id && x.Code == normalizedCode);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"A work center with code '{normalizedCode}' already exists."));
        }

        workCenter.Code = normalizedCode;
        workCenter.Name = request.Name.Trim();
        workCenter.Department = request.Department.Trim();
        workCenter.HourlyRate = request.HourlyRate;
        workCenter.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(MapWorkCenter(workCenter));
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
