using Microsoft.EntityFrameworkCore;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Development;

public class QccSupervisorDemoSeeder
{
    private const string ProjectCode = "QCC-25-30744";
    private const string FinishedGoodCode = "QCC-25-30744";
    private const string RouteRevision = "A";
    private const string DemoRequirementNote = "QCC supervisor demo seed";

    private static readonly DemoAssemblyScenario[] Scenarios =
    [
        new("BE1001", DemoWorkOrderState.InProgressFitup, true, "STL-S355", 12m, 2400m, 1200m, 226.08m),
        new("BE1002", DemoWorkOrderState.PlannedBlocked, false, null, null, null, null, null),
        new("CL1001", DemoWorkOrderState.ReleasedCutReady, true, "STL-S275", 8m, 1800m, 1000m, 118.40m),
        new("HR1001", DemoWorkOrderState.Completed, true, "STL-S355", 10m, 1600m, 900m, 142.30m),
        new("VB1001", DemoWorkOrderState.InProgressWeld, true, "AL-5052", 6m, 1400m, 800m, 58.20m)
    ];

    private readonly OstaDbContext _context;

    public QccSupervisorDemoSeeder(OstaDbContext context)
    {
        _context = context;
    }

    public async Task<QccSupervisorDemoSeedResponseDto> SeedAsync(CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == ProjectCode, cancellationToken);

        if (project is null)
        {
            throw new InvalidOperationException(
                $"Project '{ProjectCode}' was not found. Import the QCC file first before seeding the supervisor demo.");
        }

        var finishedGood = await _context.FinishedGoods
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id && x.Code == FinishedGoodCode, cancellationToken)
            ?? await _context.FinishedGoods
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == project.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No finished good was found under project '{ProjectCode}'. Import the QCC file first before seeding the supervisor demo.");

        var selectedAssemblyCodes = Scenarios.Select(x => x.AssemblyCode).ToList();

        var assemblies = await _context.Assemblies
            .Where(x => x.FinishedGoodId == finishedGood.Id && selectedAssemblyCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        var missingAssemblies = selectedAssemblyCodes.Except(assemblies.Select(x => x.Code)).ToList();
        if (missingAssemblies.Count > 0)
        {
            throw new InvalidOperationException(
                $"The following QCC execution assemblies were not found under finished good '{finishedGood.Code}': {string.Join(", ", missingAssemblies)}.");
        }

        var assemblyByCode = assemblies.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        var itemMasters = await _context.ItemMasters
            .Where(x => selectedAssemblyCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        var missingItemMasters = selectedAssemblyCodes.Except(itemMasters.Select(x => x.Code)).ToList();
        if (missingItemMasters.Count > 0)
        {
            throw new InvalidOperationException(
                $"The following QCC item masters were not found: {string.Join(", ", missingItemMasters)}.");
        }

        var itemMasterByCode = itemMasters.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var workCenters = await EnsureWorkCentersAsync(cancellationToken);

        foreach (var scenario in Scenarios)
        {
            var itemMaster = itemMasterByCode[scenario.AssemblyCode];
            await EnsureMaterialRequirementsAsync(itemMaster, scenario, cancellationToken);
            await EnsureDemoRoutingAsync(itemMaster, workCenters, cancellationToken);
        }

        await DeleteExistingDemoWorkOrdersAsync(assemblies.Select(x => x.Id).ToList(), cancellationToken);

        var seededItems = new List<SeededWorkOrderSummaryDto>();
        var seededAtUtc = DateTime.UtcNow;

        foreach (var scenario in Scenarios)
        {
            var assembly = assemblyByCode[scenario.AssemblyCode];
            var itemMaster = itemMasterByCode[scenario.AssemblyCode];
            var route = await GetDemoRoutingAsync(itemMaster.Id, assembly.Code, cancellationToken);

            var workOrder = await CreateDemoWorkOrderAsync(
                project.Id,
                finishedGood.Id,
                assembly.Id,
                assembly.Code,
                route,
                seededAtUtc,
                cancellationToken);

            ApplyScenarioState(workOrder, scenario, seededAtUtc);
            await _context.SaveChangesAsync(cancellationToken);

            var currentOperation = workOrder.Operations
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.OperationNumber)
                .FirstOrDefault(x => x.Status == WorkOrderOperationStatus.InProgress)
                ?? workOrder.Operations
                    .OrderBy(x => x.Sequence)
                    .ThenBy(x => x.OperationNumber)
                    .FirstOrDefault(x => x.Status == WorkOrderOperationStatus.Ready);

            var isMaterialReady = await _context.ItemMaterialRequirements
                .AnyAsync(x => x.ItemMasterId == itemMaster.Id, cancellationToken);

            var isReleaseReady = workOrder.Status == WorkOrderStatus.Planned &&
                                 workOrder.Operations.Count > 0 &&
                                 isMaterialReady;

            seededItems.Add(new SeededWorkOrderSummaryDto(
                assembly.Code,
                workOrder.Id,
                workOrder.WorkOrderNumber,
                workOrder.Status.ToString(),
                currentOperation?.OperationCode,
                isMaterialReady,
                isReleaseReady));
        }

        return new QccSupervisorDemoSeedResponseDto(
            project.Code,
            finishedGood.Code,
            seededAtUtc,
            seededItems.Count,
            seededItems);
    }

    private async Task<Dictionary<string, WorkCenter>> EnsureWorkCentersAsync(CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            new WorkCenterDefinition("LASER", "Laser Cutting", "Fabrication", 120m),
            new WorkCenterDefinition("FITUP", "Fit-Up Station", "Fabrication", 95m),
            new WorkCenterDefinition("WELD", "Welding", "Fabrication", 110m),
            new WorkCenterDefinition("QC_FINAL", "Final QC", "Quality", 80m)
        };

        var existing = await _context.WorkCenters
            .Where(x => definitions.Select(d => d.Code).Contains(x.Code))
            .ToListAsync(cancellationToken);

        foreach (var definition in definitions)
        {
            var workCenter = existing.FirstOrDefault(x => x.Code == definition.Code);
            if (workCenter is null)
            {
                workCenter = new WorkCenter
                {
                    Id = Guid.NewGuid(),
                    Code = definition.Code,
                    Name = definition.Name,
                    Department = definition.Department,
                    HourlyRate = definition.HourlyRate,
                    IsActive = true
                };

                _context.WorkCenters.Add(workCenter);
                existing.Add(workCenter);
            }
            else
            {
                workCenter.Name = definition.Name;
                workCenter.Department = definition.Department;
                workCenter.HourlyRate = definition.HourlyRate;
                workCenter.IsActive = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existing.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
    }

    private async Task EnsureMaterialRequirementsAsync(
        ItemMaster itemMaster,
        DemoAssemblyScenario scenario,
        CancellationToken cancellationToken)
    {
        var existing = await _context.ItemMaterialRequirements
            .Where(x => x.ItemMasterId == itemMaster.Id)
            .ToListAsync(cancellationToken);

        if (!scenario.HasMaterialRequirement)
        {
            if (existing.Count > 0)
            {
                _context.ItemMaterialRequirements.RemoveRange(existing);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var requirement = existing.FirstOrDefault();
        if (requirement is null)
        {
            requirement = new ItemMaterialRequirement
            {
                Id = Guid.NewGuid(),
                ItemMasterId = itemMaster.Id,
                MaterialCode = scenario.MaterialCode!,
                RequiredQuantity = 1m,
                Uom = "PL"
            };

            _context.ItemMaterialRequirements.Add(requirement);
        }

        requirement.MaterialCode = scenario.MaterialCode!;
        requirement.RequiredQuantity = 1m;
        requirement.Uom = "PL";
        requirement.ThicknessMm = scenario.ThicknessMm;
        requirement.LengthMm = scenario.LengthMm;
        requirement.WidthMm = scenario.WidthMm;
        requirement.WeightKg = scenario.WeightKg;
        requirement.Notes = DemoRequirementNote;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDemoRoutingAsync(
        ItemMaster itemMaster,
        IReadOnlyDictionary<string, WorkCenter> workCenters,
        CancellationToken cancellationToken)
    {
        var templateCode = GetDemoRoutingCode(itemMaster.Code);

        var routingTemplate = await _context.RoutingTemplates
            .Include(x => x.Operations)
            .FirstOrDefaultAsync(
                x => x.ItemMasterId == itemMaster.Id && x.Code == templateCode,
                cancellationToken);

        if (routingTemplate is null)
        {
            routingTemplate = new RoutingTemplate
            {
                Id = Guid.NewGuid(),
                ItemMasterId = itemMaster.Id,
                Code = templateCode,
                Name = $"{itemMaster.Code} Demo Route",
                Revision = RouteRevision,
                Status = RoutingTemplateStatus.Active,
                IsActive = true
            };

            _context.RoutingTemplates.Add(routingTemplate);
            await _context.SaveChangesAsync(cancellationToken);
            await _context.Entry(routingTemplate).Collection(x => x.Operations).LoadAsync(cancellationToken);
        }
        else
        {
            routingTemplate.Name = $"{itemMaster.Code} Demo Route";
            routingTemplate.Revision = RouteRevision;
            routingTemplate.Status = RoutingTemplateStatus.Active;
            routingTemplate.IsActive = true;

            if (routingTemplate.Operations.Count > 0)
            {
                _context.RoutingOperations.RemoveRange(routingTemplate.Operations);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var operations = new[]
        {
            new RoutingOperationDefinition("0010", "CUT", "Cutting", "LASER", 10m, 20m, 10, false),
            new RoutingOperationDefinition("0020", "FITUP", "Fit-Up", "FITUP", 8m, 18m, 20, false),
            new RoutingOperationDefinition("0030", "WELD", "Welding", "WELD", 12m, 24m, 30, false),
            new RoutingOperationDefinition("0040", "FINAL_QC", "Final QC", "QC_FINAL", 5m, 10m, 40, true)
        };

        foreach (var definition in operations)
        {
            _context.RoutingOperations.Add(new RoutingOperation
            {
                Id = Guid.NewGuid(),
                RoutingTemplateId = routingTemplate.Id,
                OperationNumber = definition.OperationNumber,
                OperationCode = definition.OperationCode,
                OperationName = definition.OperationName,
                WorkCenterId = workCenters[definition.WorkCenterCode].Id,
                SetupTimeMinutes = definition.SetupTimeMinutes,
                RunTimeMinutes = definition.RunTimeMinutes,
                Sequence = definition.Sequence,
                IsQcGate = definition.IsQcGate
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<RoutingTemplate> GetDemoRoutingAsync(Guid itemMasterId, string assemblyCode, CancellationToken cancellationToken)
    {
        var templateCode = GetDemoRoutingCode(assemblyCode);

        var route = await _context.RoutingTemplates
            .Include(x => x.Operations.OrderBy(op => op.Sequence).ThenBy(op => op.OperationNumber))
            .FirstOrDefaultAsync(x => x.ItemMasterId == itemMasterId && x.Code == templateCode, cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException($"No demo routing was found for item master '{itemMasterId}'.");
        }

        return route;
    }

    private async Task DeleteExistingDemoWorkOrdersAsync(IReadOnlyCollection<Guid> assemblyIds, CancellationToken cancellationToken)
    {
        var workOrders = await _context.WorkOrders
            .Include(x => x.Operations)
            .Where(x => assemblyIds.Contains(x.AssemblyId))
            .ToListAsync(cancellationToken);

        if (workOrders.Count == 0)
        {
            return;
        }

        var operations = workOrders.SelectMany(x => x.Operations).ToList();
        if (operations.Count > 0)
        {
            _context.WorkOrderOperations.RemoveRange(operations);
        }

        _context.WorkOrders.RemoveRange(workOrders);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<WorkOrder> CreateDemoWorkOrderAsync(
        Guid projectId,
        Guid finishedGoodId,
        Guid assemblyId,
        string assemblyCode,
        RoutingTemplate route,
        DateTime seededAtUtc,
        CancellationToken cancellationToken)
    {
        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FinishedGoodId = finishedGoodId,
            AssemblyId = assemblyId,
            WorkOrderNumber = $"WO-QCC-{assemblyCode}",
            Status = WorkOrderStatus.Planned,
            PlannedQuantity = 1m,
            CompletedQuantity = 0m,
            ReleasedAtUtc = null,
            ClosedAtUtc = null,
            Operations = route.Operations
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.OperationNumber)
                .Select((operation, index) => new WorkOrderOperation
                {
                    Id = Guid.NewGuid(),
                    RoutingOperationId = operation.Id,
                    OperationNumber = operation.OperationNumber,
                    OperationCode = operation.OperationCode,
                    OperationName = operation.OperationName,
                    WorkCenterId = operation.WorkCenterId,
                    Status = index == 0 ? WorkOrderOperationStatus.Ready : WorkOrderOperationStatus.Blocked,
                    PlannedQuantity = 1m,
                    CompletedQuantity = 0m,
                    Sequence = operation.Sequence,
                    IsQcGate = operation.IsQcGate
                })
                .ToList()
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.WorkOrders
            .Include(x => x.Operations.OrderBy(op => op.Sequence).ThenBy(op => op.OperationNumber))
            .FirstAsync(x => x.Id == workOrder.Id, cancellationToken);
    }

    private static void ApplyScenarioState(WorkOrder workOrder, DemoAssemblyScenario scenario, DateTime seededAtUtc)
    {
        workOrder.Status = WorkOrderStatus.Planned;
        workOrder.CompletedQuantity = 0m;
        workOrder.ReleasedAtUtc = null;
        workOrder.ClosedAtUtc = null;

        foreach (var operation in workOrder.Operations.OrderBy(x => x.Sequence).ThenBy(x => x.OperationNumber))
        {
            operation.Status = operation.Sequence == workOrder.Operations.Min(y => y.Sequence)
                ? WorkOrderOperationStatus.Ready
                : WorkOrderOperationStatus.Blocked;
            operation.StartedAtUtc = null;
            operation.CompletedAtUtc = null;
            operation.CompletedQuantity = 0m;
        }

        switch (scenario.State)
        {
            case DemoWorkOrderState.PlannedBlocked:
                return;

            case DemoWorkOrderState.ReleasedCutReady:
                Release(workOrder, seededAtUtc.AddHours(-6));
                return;

            case DemoWorkOrderState.InProgressFitup:
                Release(workOrder, seededAtUtc.AddHours(-5));
                var cut = GetOperation(workOrder, "0010");
                Start(workOrder, cut, seededAtUtc.AddHours(-4.5));
                Complete(workOrder, cut, seededAtUtc.AddHours(-4));
                var fitup = GetOperation(workOrder, "0020");
                Start(workOrder, fitup, seededAtUtc.AddHours(-3.5));
                return;

            case DemoWorkOrderState.InProgressWeld:
                Release(workOrder, seededAtUtc.AddHours(-4));
                var firstCut = GetOperation(workOrder, "0010");
                Start(workOrder, firstCut, seededAtUtc.AddHours(-3.75));
                Complete(workOrder, firstCut, seededAtUtc.AddHours(-3.5));
                var firstFitup = GetOperation(workOrder, "0020");
                Start(workOrder, firstFitup, seededAtUtc.AddHours(-3.25));
                Complete(workOrder, firstFitup, seededAtUtc.AddHours(-3));
                var weld = GetOperation(workOrder, "0030");
                Start(workOrder, weld, seededAtUtc.AddHours(-2.5));
                return;

            case DemoWorkOrderState.Completed:
                Release(workOrder, seededAtUtc.AddHours(-8));
                foreach (var operation in workOrder.Operations.OrderBy(x => x.Sequence).ThenBy(x => x.OperationNumber))
                {
                    Start(workOrder, operation, seededAtUtc.AddHours(-7 + (operation.Sequence / 20d)));
                    Complete(workOrder, operation, seededAtUtc.AddHours(-6.5 + (operation.Sequence / 20d)));
                }
                return;
        }
    }

    private static WorkOrderOperation GetOperation(WorkOrder workOrder, string operationNumber)
    {
        return workOrder.Operations.First(x => x.OperationNumber == operationNumber);
    }

    private static void Release(WorkOrder workOrder, DateTime releasedAtUtc)
    {
        workOrder.Status = WorkOrderStatus.Released;
        workOrder.ReleasedAtUtc = releasedAtUtc;
        workOrder.ClosedAtUtc = null;
    }

    private static void Start(WorkOrder workOrder, WorkOrderOperation operation, DateTime startedAtUtc)
    {
        if (operation.Status != WorkOrderOperationStatus.Ready)
        {
            throw new InvalidOperationException(
                $"Operation '{operation.OperationNumber}' on work order '{workOrder.WorkOrderNumber}' is not ready to start.");
        }

        if (workOrder.Status != WorkOrderStatus.Released && workOrder.Status != WorkOrderStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Work order '{workOrder.WorkOrderNumber}' must be Released or InProgress before starting operations.");
        }

        operation.Status = WorkOrderOperationStatus.InProgress;
        operation.StartedAtUtc = startedAtUtc;

        if (workOrder.Status == WorkOrderStatus.Released)
        {
            workOrder.Status = WorkOrderStatus.InProgress;
        }
    }

    private static void Complete(WorkOrder workOrder, WorkOrderOperation operation, DateTime completedAtUtc)
    {
        if (operation.Status != WorkOrderOperationStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Operation '{operation.OperationNumber}' on work order '{workOrder.WorkOrderNumber}' is not in progress.");
        }

        operation.Status = WorkOrderOperationStatus.Completed;
        operation.CompletedAtUtc = completedAtUtc;
        operation.CompletedQuantity = operation.PlannedQuantity;

        var nextOperation = workOrder.Operations
            .Where(x => x.Sequence > operation.Sequence)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.OperationNumber)
            .FirstOrDefault();

        if (nextOperation is not null && nextOperation.Status == WorkOrderOperationStatus.Blocked)
        {
            nextOperation.Status = WorkOrderOperationStatus.Ready;
        }

        if (workOrder.Operations.All(x => x.Status == WorkOrderOperationStatus.Completed))
        {
            workOrder.Status = WorkOrderStatus.Completed;
            workOrder.CompletedQuantity = workOrder.PlannedQuantity;
            workOrder.ClosedAtUtc = completedAtUtc;
        }
    }

    private static string GetDemoRoutingCode(string assemblyCode) => $"QCC-DEMO-{assemblyCode}";

    private sealed record WorkCenterDefinition(string Code, string Name, string Department, decimal HourlyRate);

    private sealed record RoutingOperationDefinition(
        string OperationNumber,
        string OperationCode,
        string OperationName,
        string WorkCenterCode,
        decimal SetupTimeMinutes,
        decimal RunTimeMinutes,
        int Sequence,
        bool IsQcGate);

    private sealed record DemoAssemblyScenario(
        string AssemblyCode,
        DemoWorkOrderState State,
        bool HasMaterialRequirement,
        string? MaterialCode,
        decimal? ThicknessMm,
        decimal? LengthMm,
        decimal? WidthMm,
        decimal? WeightKg);

    private enum DemoWorkOrderState
    {
        PlannedBlocked = 1,
        ReleasedCutReady = 2,
        InProgressFitup = 3,
        InProgressWeld = 4,
        Completed = 5
    }
}

public sealed record QccSupervisorDemoSeedResponseDto(
    string ProjectCode,
    string FinishedGoodCode,
    DateTime SeededAtUtc,
    int WorkOrderCount,
    IReadOnlyList<SeededWorkOrderSummaryDto> Items);

public sealed record SeededWorkOrderSummaryDto(
    string AssemblyCode,
    Guid WorkOrderId,
    string WorkOrderNumber,
    string Status,
    string? CurrentOperationCode,
    bool IsMaterialReady,
    bool IsReleaseReady);
