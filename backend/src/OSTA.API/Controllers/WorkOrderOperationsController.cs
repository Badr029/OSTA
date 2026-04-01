using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.WorkOrderOperations;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/work-order-operations")]
public class WorkOrderOperationsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public WorkOrderOperationsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkOrderOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderOperationResponseDto>> GetById(Guid id)
    {
        var operation = await GetOperationResponseAsync(id);

        if (operation is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order operation '{id}' was not found."));
        }

        return Ok(operation);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(WorkOrderOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderOperationResponseDto>> Start(Guid id)
    {
        var operation = await _context.WorkOrderOperations
            .Include(x => x.WorkOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (operation is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order operation '{id}' was not found."));
        }

        if (operation.Status != WorkOrderOperationStatus.Ready)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"Work order operation '{id}' cannot be started because its status is '{operation.Status}' instead of 'Ready'."));
        }

        operation.Status = WorkOrderOperationStatus.InProgress;
        operation.StartedAtUtc = DateTime.UtcNow;

        if (operation.WorkOrder.Status is WorkOrderStatus.Planned or WorkOrderStatus.Released)
        {
            operation.WorkOrder.Status = WorkOrderStatus.InProgress;
        }

        await _context.SaveChangesAsync();

        var response = await GetOperationResponseAsync(id);
        return Ok(response!);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(WorkOrderOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderOperationResponseDto>> Complete(Guid id)
    {
        var operation = await _context.WorkOrderOperations
            .Include(x => x.WorkOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (operation is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order operation '{id}' was not found."));
        }

        if (operation.Status != WorkOrderOperationStatus.InProgress)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"Work order operation '{id}' cannot be completed because its status is '{operation.Status}' instead of 'InProgress'."));
        }

        operation.Status = WorkOrderOperationStatus.Completed;
        operation.CompletedAtUtc = DateTime.UtcNow;
        operation.CompletedQuantity = operation.PlannedQuantity;

        var nextOperation = await _context.WorkOrderOperations
            .Where(x => x.WorkOrderId == operation.WorkOrderId && x.Sequence > operation.Sequence)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.OperationNumber)
            .FirstOrDefaultAsync();

        if (nextOperation is not null && nextOperation.Status == WorkOrderOperationStatus.Blocked)
        {
            nextOperation.Status = WorkOrderOperationStatus.Ready;
        }

        var incompleteOperationsExist = await _context.WorkOrderOperations
            .Where(x => x.WorkOrderId == operation.WorkOrderId && x.Id != operation.Id)
            .AnyAsync(x => x.Status != WorkOrderOperationStatus.Completed);

        if (!incompleteOperationsExist)
        {
            operation.WorkOrder.Status = WorkOrderStatus.Completed;
            operation.WorkOrder.CompletedQuantity = operation.WorkOrder.PlannedQuantity;
            operation.WorkOrder.ClosedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var response = await GetOperationResponseAsync(id);
        return Ok(response!);
    }

    private Task<WorkOrderOperationResponseDto?> GetOperationResponseAsync(Guid id)
    {
        return _context.WorkOrderOperations
            .Where(x => x.Id == id)
            .Select(x => new WorkOrderOperationResponseDto(
                x.Id,
                x.WorkOrderId,
                x.RoutingOperationId,
                x.OperationNumber,
                x.OperationCode,
                x.OperationName,
                x.WorkCenterId,
                x.WorkCenter.Code,
                x.Status.ToString(),
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.StartedAtUtc,
                x.CompletedAtUtc,
                x.Sequence,
                x.IsQcGate))
            .FirstOrDefaultAsync();
    }
}
