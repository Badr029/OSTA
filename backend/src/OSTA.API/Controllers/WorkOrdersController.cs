using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.WorkOrderOperations;
using OSTA.API.Contracts.WorkOrders;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly OstaDbContext _context;

    public WorkOrdersController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkOrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkOrderResponseDto>>> GetAll()
    {
        var workOrders = await _context.WorkOrders
            .OrderBy(x => x.WorkOrderNumber)
            .Select(x => new WorkOrderResponseDto(
                x.Id,
                x.WorkOrderNumber,
                x.ProjectId,
                x.Project.Code,
                x.FinishedGoodId,
                x.FinishedGood.Code,
                x.AssemblyId,
                x.Assembly.Code,
                x.Status.ToString(),
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.ReleasedAtUtc,
                x.ClosedAtUtc,
                x.Operations.Count))
            .ToListAsync();

        return Ok(workOrders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponseDto>> GetById(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Where(x => x.Id == id)
            .Select(x => new WorkOrderResponseDto(
                x.Id,
                x.WorkOrderNumber,
                x.ProjectId,
                x.Project.Code,
                x.FinishedGoodId,
                x.FinishedGood.Code,
                x.AssemblyId,
                x.Assembly.Code,
                x.Status.ToString(),
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.ReleasedAtUtc,
                x.ClosedAtUtc,
                x.Operations.Count))
            .FirstOrDefaultAsync();

        if (workOrder is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order '{id}' was not found."));
        }

        return Ok(workOrder);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(WorkOrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkOrderResponseDto>> Generate([FromBody] GenerateWorkOrderRequestDto request)
    {
        var assemblyContext = await _context.Assemblies
            .Where(x => x.Id == request.AssemblyId)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.FinishedGoodId,
                FinishedGoodCode = x.FinishedGood.Code,
                x.FinishedGood.ProjectId,
                ProjectCode = x.FinishedGood.Project.Code
            })
            .FirstOrDefaultAsync();

        if (assemblyContext is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Assembly '{request.AssemblyId}' was not found."));
        }

        if (assemblyContext.ProjectId != request.ProjectId)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"Assembly '{request.AssemblyId}' does not belong to project '{request.ProjectId}'."));
        }

        if (assemblyContext.FinishedGoodId != request.FinishedGoodId)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"Assembly '{request.AssemblyId}' does not belong to finished good '{request.FinishedGoodId}'."));
        }

        var existingWorkOrder = await _context.WorkOrders.AnyAsync(x => x.AssemblyId == request.AssemblyId);

        if (existingWorkOrder)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict(
                $"A work order already exists for assembly '{request.AssemblyId}'."));
        }

        var itemMaster = await _context.ItemMasters
            .Where(x => x.Code == assemblyContext.Code)
            .Select(x => new { x.Id, x.Code })
            .FirstOrDefaultAsync();

        if (itemMaster is null)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"No item master was found matching assembly code '{assemblyContext.Code}'."));
        }

        var routingTemplate = await _context.RoutingTemplates
            .Where(x => x.ItemMasterId == itemMaster.Id && x.IsActive && x.Status == RoutingTemplateStatus.Active)
            .OrderByDescending(x => x.Revision)
            .Select(x => new
            {
                x.Id,
                Operations = x.Operations
                    .OrderBy(op => op.Sequence)
                    .ThenBy(op => op.OperationNumber)
                    .Select(op => new
                    {
                        op.Id,
                        op.OperationNumber,
                        op.OperationCode,
                        op.OperationName,
                        op.WorkCenterId,
                        op.Sequence,
                        op.IsQcGate
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (routingTemplate is null)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"No active routing template was found for item master code '{itemMaster.Code}'."));
        }

        if (routingTemplate.Operations.Count == 0)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"Routing template '{routingTemplate.Id}' has no operations to generate."));
        }

        var plannedQuantity = request.PlannedQuantity ?? 1m;
        var workOrderNumber = await GenerateNextWorkOrderNumberAsync();

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            FinishedGoodId = request.FinishedGoodId,
            AssemblyId = request.AssemblyId,
            WorkOrderNumber = workOrderNumber,
            Status = WorkOrderStatus.Planned,
            PlannedQuantity = plannedQuantity,
            CompletedQuantity = 0m
        };

        var operations = routingTemplate.Operations
            .Select((operation, index) => new WorkOrderOperation
            {
                Id = Guid.NewGuid(),
                WorkOrderId = workOrder.Id,
                RoutingOperationId = operation.Id,
                OperationNumber = operation.OperationNumber,
                OperationCode = operation.OperationCode,
                OperationName = operation.OperationName,
                WorkCenterId = operation.WorkCenterId,
                Status = index == 0 ? WorkOrderOperationStatus.Ready : WorkOrderOperationStatus.Blocked,
                PlannedQuantity = plannedQuantity,
                CompletedQuantity = 0m,
                Sequence = operation.Sequence,
                IsQcGate = operation.IsQcGate
            })
            .ToList();

        _context.WorkOrders.Add(workOrder);
        _context.WorkOrderOperations.AddRange(operations);
        await _context.SaveChangesAsync();

        var createdWorkOrder = await _context.WorkOrders
            .Where(x => x.Id == workOrder.Id)
            .Select(x => new WorkOrderResponseDto(
                x.Id,
                x.WorkOrderNumber,
                x.ProjectId,
                x.Project.Code,
                x.FinishedGoodId,
                x.FinishedGood.Code,
                x.AssemblyId,
                x.Assembly.Code,
                x.Status.ToString(),
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.ReleasedAtUtc,
                x.ClosedAtUtc,
                x.Operations.Count))
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = workOrder.Id }, createdWorkOrder);
    }

    [HttpGet("{workOrderId:guid}/operations")]
    [ProducesResponseType(typeof(IEnumerable<WorkOrderOperationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<WorkOrderOperationResponseDto>>> GetOperations(Guid workOrderId)
    {
        var workOrderExists = await _context.WorkOrders.AnyAsync(x => x.Id == workOrderId);

        if (!workOrderExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order '{workOrderId}' was not found."));
        }

        var operations = await _context.WorkOrderOperations
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.OperationNumber)
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
            .ToListAsync();

        return Ok(operations);
    }

    private async Task<string> GenerateNextWorkOrderNumberAsync()
    {
        var nextSequence = await _context.WorkOrders.CountAsync() + 1;
        var candidate = $"WO-{nextSequence:D6}";

        while (await _context.WorkOrders.AnyAsync(x => x.WorkOrderNumber == candidate))
        {
            nextSequence++;
            candidate = $"WO-{nextSequence:D6}";
        }

        return candidate;
    }
}
