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

    [HttpGet("summary")]
    [ProducesResponseType(typeof(IEnumerable<WorkOrderSummaryListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkOrderSummaryListItemDto>>> GetSummaryList(
        [FromQuery] string? status,
        [FromQuery] string? projectCode,
        [FromQuery] string? assemblyCode,
        [FromQuery] bool? isMaterialReady,
        [FromQuery] bool? isReleaseReady)
    {
        WorkOrderStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<WorkOrderStatus>(status, true, out var parsedStatus))
            {
                return BadRequest(ApiProblemDetailsFactory.BadRequest(
                    $"Work order status filter '{status}' is invalid."));
            }

            statusFilter = parsedStatus;
        }

        var summaries = await BuildWorkOrderSummariesAsync(
            statusFilter,
            projectCode,
            assemblyCode);

        if (isMaterialReady.HasValue)
        {
            summaries = summaries
                .Where(x => x.IsMaterialReady == isMaterialReady.Value)
                .ToList();
        }

        if (isReleaseReady.HasValue)
        {
            summaries = summaries
                .Where(x => x.IsReleaseReady == isReleaseReady.Value)
                .ToList();
        }

        var response = summaries
            .OrderBy(x => x.WorkOrderNumber)
            .Select(x => new WorkOrderSummaryListItemDto(
                x.WorkOrderId,
                x.WorkOrderNumber,
                x.Status,
                x.ProjectCode,
                x.FinishedGoodCode,
                x.AssemblyCode,
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.IsReleaseReady,
                x.IsMaterialReady,
                x.CurrentOperation?.OperationCode,
                x.CurrentOperation?.Status,
                x.NextOperation?.OperationCode,
                x.ReleasedAtUtc))
            .ToList();

        return Ok(response);
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

    [HttpGet("{id:guid}/summary")]
    [ProducesResponseType(typeof(WorkOrderSummaryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderSummaryResponseDto>> GetSummary(Guid id)
    {
        var summary = await BuildSummaryAsync(id);

        if (summary is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order '{id}' was not found."));
        }

        return Ok(summary);
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

    [HttpGet("{id:guid}/release-readiness")]
    [ProducesResponseType(typeof(WorkOrderReleaseReadinessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderReleaseReadinessResponseDto>> GetReleaseReadiness(Guid id)
    {
        var readiness = await BuildReleaseReadinessAsync(id);

        if (readiness is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order '{id}' was not found."));
        }

        return Ok(readiness);
    }

    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(typeof(WorkOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponseDto>> Release(Guid id)
    {
        var readiness = await BuildReleaseReadinessAsync(id);

        if (readiness is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work order '{id}' was not found."));
        }

        if (!readiness.IsReleaseReady)
        {
            return BadRequest(ApiProblemDetailsFactory.BadRequest(
                $"Work order '{id}' cannot be released: {string.Join(" ", readiness.BlockingReasons)}"));
        }

        var workOrder = await _context.WorkOrders.FirstAsync(x => x.Id == id);
        workOrder.Status = WorkOrderStatus.Released;
        workOrder.ReleasedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = await _context.WorkOrders
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
            .FirstAsync();

        return Ok(response);
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

    private async Task<WorkOrderSummaryResponseDto?> BuildSummaryAsync(Guid id)
    {
        var releaseReadiness = await BuildReleaseReadinessAsync(id);
        var summary = (await BuildWorkOrderSummariesAsync(workOrderId: id)).FirstOrDefault();

        if (summary is null)
        {
            return null;
        }

        return new WorkOrderSummaryResponseDto(
            summary.WorkOrderId,
            summary.WorkOrderNumber,
            summary.Status,
            summary.ProjectCode,
            summary.FinishedGoodCode,
            summary.AssemblyCode,
            summary.PlannedQuantity,
            summary.CompletedQuantity,
            releaseReadiness?.IsReleaseReady ?? false,
            releaseReadiness?.IsMaterialReady ?? false,
            summary.TotalOperations,
            summary.CompletedOperationsCount,
            summary.BlockedOperationsCount,
            summary.InProgressOperationsCount,
            summary.CurrentOperation,
            summary.NextOperation,
            summary.ReleasedAtUtc,
            summary.ClosedAtUtc);
    }

    private async Task<List<ComputedWorkOrderSummary>> BuildWorkOrderSummariesAsync(
        WorkOrderStatus? statusFilter = null,
        string? projectCode = null,
        string? assemblyCode = null,
        Guid? workOrderId = null)
    {
        var query = _context.WorkOrders.AsNoTracking().AsQueryable();

        if (workOrderId.HasValue)
        {
            query = query.Where(x => x.Id == workOrderId.Value);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(x => x.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(projectCode))
        {
            var normalizedProjectCode = projectCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.Project.Code.ToUpper() == normalizedProjectCode);
        }

        if (!string.IsNullOrWhiteSpace(assemblyCode))
        {
            var normalizedAssemblyCode = assemblyCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.Assembly.Code.ToUpper() == normalizedAssemblyCode);
        }

        var workOrders = await query
            .Select(x => new WorkOrderSummarySource(
                x.Id,
                x.WorkOrderNumber,
                x.Status,
                x.Project.Code,
                x.FinishedGood.Code,
                x.AssemblyId,
                x.Assembly.Code,
                x.Assembly.SourceComponentItemMasterId,
                x.PlannedQuantity,
                x.CompletedQuantity,
                x.ReleasedAtUtc,
                x.ClosedAtUtc))
            .ToListAsync();

        if (workOrders.Count == 0)
        {
            return [];
        }

        var workOrderIds = workOrders.Select(x => x.Id).ToList();
        var operations = await _context.WorkOrderOperations
            .AsNoTracking()
            .Where(x => workOrderIds.Contains(x.WorkOrderId))
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.OperationNumber)
            .Select(x => new WorkOrderOperationSummaryRow(
                x.WorkOrderId,
                new WorkOrderOperationSummaryDto(
                    x.Id,
                    x.OperationNumber,
                    x.OperationCode,
                    x.OperationName,
                    x.WorkCenter.Code,
                    x.Status.ToString(),
                    x.Sequence,
                    x.IsQcGate)))
            .ToListAsync();

        var operationsByWorkOrder = operations
            .GroupBy(x => x.WorkOrderId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Operation).ToList());

        var sourceItemMasterIds = workOrders
            .Where(x => x.SourceComponentItemMasterId.HasValue)
            .Select(x => x.SourceComponentItemMasterId!.Value)
            .Distinct()
            .ToList();

        var materialRequirementCounts = sourceItemMasterIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await _context.ItemMaterialRequirements
                .AsNoTracking()
                .Where(x => sourceItemMasterIds.Contains(x.ItemMasterId))
                .GroupBy(x => x.ItemMasterId)
                .Select(x => new { x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

        return workOrders
            .Select(workOrder =>
            {
                var workOrderOperations = operationsByWorkOrder.GetValueOrDefault(workOrder.Id, []);
                var currentOperation = workOrderOperations.FirstOrDefault(x => x.Status == WorkOrderOperationStatus.InProgress.ToString())
                    ?? workOrderOperations.FirstOrDefault(x => x.Status == WorkOrderOperationStatus.Ready.ToString());

                WorkOrderOperationSummaryDto? nextOperation = null;
                if (currentOperation is not null)
                {
                    nextOperation = workOrderOperations
                        .Where(x =>
                            x.Sequence > currentOperation.Sequence &&
                            (x.Status == WorkOrderOperationStatus.Ready.ToString() ||
                             x.Status == WorkOrderOperationStatus.Blocked.ToString()))
                        .OrderBy(x => x.Sequence)
                        .ThenBy(x => x.OperationNumber)
                        .FirstOrDefault();
                }

                var isMaterialReady =
                    workOrder.SourceComponentItemMasterId.HasValue &&
                    materialRequirementCounts.GetValueOrDefault(workOrder.SourceComponentItemMasterId.Value) > 0;

                var isReleaseReady = ComputeIsReleaseReady(
                    workOrder.Status,
                    workOrderOperations.Count,
                    isMaterialReady);

                return new ComputedWorkOrderSummary(
                    workOrder.Id,
                    workOrder.WorkOrderNumber,
                    workOrder.Status.ToString(),
                    workOrder.ProjectCode,
                    workOrder.FinishedGoodCode,
                    workOrder.AssemblyCode,
                    workOrder.PlannedQuantity,
                    workOrder.CompletedQuantity,
                    isReleaseReady,
                    isMaterialReady,
                    workOrderOperations.Count,
                    workOrderOperations.Count(x => x.Status == WorkOrderOperationStatus.Completed.ToString()),
                    workOrderOperations.Count(x => x.Status == WorkOrderOperationStatus.Blocked.ToString()),
                    workOrderOperations.Count(x => x.Status == WorkOrderOperationStatus.InProgress.ToString()),
                    currentOperation,
                    nextOperation,
                    workOrder.ReleasedAtUtc,
                    workOrder.ClosedAtUtc);
            })
            .ToList();
    }

    private async Task<WorkOrderReleaseReadinessResponseDto?> BuildReleaseReadinessAsync(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.WorkOrderNumber,
                WorkOrderStatus = x.Status,
                x.AssemblyId,
                AssemblyCode = x.Assembly.Code
            })
            .FirstOrDefaultAsync();

        if (workOrder is null)
        {
            return null;
        }

        var blockingReasons = new List<string>();

        var materialReadiness = await BuildAssemblyMaterialReadinessAsync(workOrder.AssemblyId);
        var assembly = materialReadiness?.Assembly;

        if (assembly is null)
        {
            blockingReasons.Add("Work order assembly was not found.");
        }

        var operationCount = await _context.WorkOrderOperations
            .Where(x => x.WorkOrderId == id)
            .CountAsync();

        var hasOperations = operationCount > 0;
        if (!hasOperations)
        {
            blockingReasons.Add("No work order operations exist.");
        }

        var isMaterialReady = false;

        if (materialReadiness is null || !materialReadiness.IsMaterialReady)
        {
            if (materialReadiness is not null)
            {
                blockingReasons.AddRange(materialReadiness.BlockingReasons);
            }
        }
        else
        {
            isMaterialReady = true;
        }

        if (workOrder.WorkOrderStatus != WorkOrderStatus.Planned)
        {
            blockingReasons.Add($"Work order status must be 'Planned' to release, but is '{workOrder.WorkOrderStatus}'.");
        }

        return new WorkOrderReleaseReadinessResponseDto(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.WorkOrderStatus.ToString(),
            workOrder.AssemblyId,
            workOrder.AssemblyCode,
            hasOperations,
            operationCount,
            isMaterialReady,
            blockingReasons.Count == 0,
            blockingReasons);
    }

    private static bool ComputeIsReleaseReady(
        WorkOrderStatus status,
        int operationCount,
        bool isMaterialReady)
    {
        return status == WorkOrderStatus.Planned &&
               operationCount > 0 &&
               isMaterialReady;
    }

    private async Task<AssemblyMaterialReadinessEvaluation?> BuildAssemblyMaterialReadinessAsync(Guid assemblyId)
    {
        var assembly = await _context.Assemblies
            .Where(x => x.Id == assemblyId)
            .Select(x => new AssemblyMaterialReadinessAssembly(
                x.Id,
                x.Code,
                x.SourceComponentItemMasterId))
            .FirstOrDefaultAsync();

        if (assembly is null)
        {
            return null;
        }

        var blockingReasons = new List<string>();

        if (assembly.SourceComponentItemMasterId is null)
        {
            blockingReasons.Add("Assembly is not linked to a source component item master.");
        }
        else
        {
            var materialRequirementCount = await _context.ItemMaterialRequirements
                .Where(x => x.ItemMasterId == assembly.SourceComponentItemMasterId)
                .CountAsync();

            if (materialRequirementCount == 0)
            {
                blockingReasons.Add("No material requirements defined for the linked item master.");
            }
        }

        return new AssemblyMaterialReadinessEvaluation(
            assembly,
            blockingReasons.Count == 0,
            blockingReasons);
    }

    private sealed record AssemblyMaterialReadinessAssembly(Guid Id, string Code, Guid? SourceComponentItemMasterId);

    private sealed record AssemblyMaterialReadinessEvaluation(
        AssemblyMaterialReadinessAssembly Assembly,
        bool IsMaterialReady,
        IReadOnlyList<string> BlockingReasons);

    private sealed record WorkOrderSummarySource(
        Guid Id,
        string WorkOrderNumber,
        WorkOrderStatus Status,
        string ProjectCode,
        string FinishedGoodCode,
        Guid AssemblyId,
        string AssemblyCode,
        Guid? SourceComponentItemMasterId,
        decimal PlannedQuantity,
        decimal CompletedQuantity,
        DateTime? ReleasedAtUtc,
        DateTime? ClosedAtUtc);

    private sealed record WorkOrderOperationSummaryRow(
        Guid WorkOrderId,
        WorkOrderOperationSummaryDto Operation);

    private sealed record ComputedWorkOrderSummary(
        Guid WorkOrderId,
        string WorkOrderNumber,
        string Status,
        string ProjectCode,
        string FinishedGoodCode,
        string AssemblyCode,
        decimal PlannedQuantity,
        decimal CompletedQuantity,
        bool IsReleaseReady,
        bool IsMaterialReady,
        int TotalOperations,
        int CompletedOperationsCount,
        int BlockedOperationsCount,
        int InProgressOperationsCount,
        WorkOrderOperationSummaryDto? CurrentOperation,
        WorkOrderOperationSummaryDto? NextOperation,
        DateTime? ReleasedAtUtc,
        DateTime? ClosedAtUtc);
}
