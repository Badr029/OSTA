using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.BomImports;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/bom-import-batches")]
public class BomImportBatchesController : ControllerBase
{
    private readonly OstaDbContext _context;

    public BomImportBatchesController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BomImportBatchSummaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BomImportBatchSummaryResponseDto>>> GetAll()
    {
        var batches = await _context.BOMImportBatches
            .OrderByDescending(x => x.ImportedAtUtc)
            .Select(x => new BomImportBatchSummaryResponseDto(
                x.Id,
                x.SourceFileName,
                x.ImportedAtUtc,
                x.Status.ToString(),
                x.TotalRows,
                x.SuccessfulRows,
                x.FailedRows
            ))
            .ToListAsync();

        return Ok(batches);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BomImportBatchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BomImportBatchResponseDto>> GetById(Guid id)
    {
        var batch = await _context.BOMImportBatches
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (batch is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM import batch '{id}' was not found."));
        }

        return Ok(MapBatch(batch));
    }

    [HttpPost]
    [ProducesResponseType(typeof(BomImportBatchResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BomImportBatchResponseDto>> Create(CreateBomImportBatchRequestDto request)
    {
        var batch = new BOMImportBatch
        {
            Id = Guid.NewGuid(),
            SourceFileName = request.SourceFileName.Trim(),
            ImportedAtUtc = DateTimeOffset.UtcNow,
            Status = BOMImportBatchStatus.Pending,
            TotalRows = request.Lines.Count,
            SuccessfulRows = 0,
            FailedRows = 0,
            Lines = request.Lines.Select(line => new BOMImportLine
            {
                Id = Guid.NewGuid(),
                RowNumber = line.RowNumber,
                ProjectCode = line.ProjectCode.Trim(),
                ProjectName = line.ProjectName.Trim(),
                FinishedGoodCode = line.FinishedGoodCode.Trim(),
                FinishedGoodName = line.FinishedGoodName.Trim(),
                AssemblyCode = line.AssemblyCode.Trim(),
                AssemblyName = line.AssemblyName.Trim(),
                PartNumber = line.PartNumber.Trim(),
                Revision = line.Revision.Trim(),
                Description = line.Description.Trim(),
                Quantity = line.Quantity,
                Status = BOMImportLineStatus.Pending
            }).ToList()
        };

        _context.BOMImportBatches.Add(batch);
        await _context.SaveChangesAsync();

        batch.Status = BOMImportBatchStatus.Processing;
        await _context.SaveChangesAsync();

        foreach (var line in batch.Lines.OrderBy(x => x.RowNumber))
        {
            var validationError = ValidateLine(line);
            if (validationError is not null)
            {
                line.Status = BOMImportLineStatus.Failed;
                line.ErrorMessage = validationError;
                batch.FailedRows++;
                await _context.SaveChangesAsync();
                continue;
            }

            var project = await FindOrCreateProjectAsync(line);
            var finishedGood = await FindOrCreateFinishedGoodAsync(project, line);
            var assembly = await FindOrCreateAssemblyAsync(finishedGood, line);

            var duplicatePartExists = await _context.Parts.AnyAsync(x =>
                x.AssemblyId == assembly.Id &&
                x.PartNumber == line.PartNumber &&
                x.Revision == line.Revision
            );

            if (duplicatePartExists)
            {
                line.Status = BOMImportLineStatus.Failed;
                line.ErrorMessage = $"Part '{line.PartNumber}' revision '{line.Revision}' already exists in assembly '{line.AssemblyCode}'.";
                batch.FailedRows++;
                await _context.SaveChangesAsync();
                continue;
            }

            _context.Parts.Add(new Part
            {
                Id = Guid.NewGuid(),
                PartNumber = line.PartNumber,
                Revision = line.Revision,
                Description = line.Description,
                AssemblyId = assembly.Id
            });

            line.Status = BOMImportLineStatus.Imported;
            line.ErrorMessage = null;
            batch.SuccessfulRows++;

            await _context.SaveChangesAsync();
        }

        batch.Status = batch.SuccessfulRows > 0
            ? BOMImportBatchStatus.Completed
            : BOMImportBatchStatus.Failed;

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = batch.Id }, MapBatch(batch));
    }

    private async Task<Project> FindOrCreateProjectAsync(BOMImportLine line)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Code == line.ProjectCode);

        if (project is not null)
        {
            return project;
        }

        project = new Project
        {
            Id = Guid.NewGuid(),
            Code = line.ProjectCode,
            Name = line.ProjectName
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return project;
    }

    private async Task<FinishedGood> FindOrCreateFinishedGoodAsync(Project project, BOMImportLine line)
    {
        var finishedGood = await _context.FinishedGoods
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id && x.Code == line.FinishedGoodCode);

        if (finishedGood is not null)
        {
            return finishedGood;
        }

        finishedGood = new FinishedGood
        {
            Id = Guid.NewGuid(),
            Code = line.FinishedGoodCode,
            Name = line.FinishedGoodName,
            ProjectId = project.Id
        };

        _context.FinishedGoods.Add(finishedGood);
        await _context.SaveChangesAsync();

        return finishedGood;
    }

    private async Task<Assembly> FindOrCreateAssemblyAsync(FinishedGood finishedGood, BOMImportLine line)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(x => x.FinishedGoodId == finishedGood.Id && x.Code == line.AssemblyCode);

        if (assembly is not null)
        {
            return assembly;
        }

        assembly = new Assembly
        {
            Id = Guid.NewGuid(),
            Code = line.AssemblyCode,
            Name = line.AssemblyName,
            FinishedGoodId = finishedGood.Id
        };

        _context.Assemblies.Add(assembly);
        await _context.SaveChangesAsync();

        return assembly;
    }

    private static string? ValidateLine(BOMImportLine line)
    {
        if (string.IsNullOrWhiteSpace(line.ProjectCode))
        {
            return "ProjectCode is required.";
        }

        if (string.IsNullOrWhiteSpace(line.ProjectName))
        {
            return "ProjectName is required.";
        }

        if (string.IsNullOrWhiteSpace(line.FinishedGoodCode))
        {
            return "FinishedGoodCode is required.";
        }

        if (string.IsNullOrWhiteSpace(line.FinishedGoodName))
        {
            return "FinishedGoodName is required.";
        }

        if (string.IsNullOrWhiteSpace(line.AssemblyCode))
        {
            return "AssemblyCode is required.";
        }

        if (string.IsNullOrWhiteSpace(line.AssemblyName))
        {
            return "AssemblyName is required.";
        }

        if (string.IsNullOrWhiteSpace(line.PartNumber))
        {
            return "PartNumber is required.";
        }

        if (string.IsNullOrWhiteSpace(line.Revision))
        {
            return "Revision is required.";
        }

        if (string.IsNullOrWhiteSpace(line.Description))
        {
            return "Description is required.";
        }

        if (line.Quantity <= 0)
        {
            return "Quantity must be greater than zero.";
        }

        return null;
    }

    private static BomImportBatchResponseDto MapBatch(BOMImportBatch batch)
    {
        return new BomImportBatchResponseDto(
            batch.Id,
            batch.SourceFileName,
            batch.ImportedAtUtc,
            batch.Status.ToString(),
            batch.TotalRows,
            batch.SuccessfulRows,
            batch.FailedRows,
            batch.Lines
                .OrderBy(x => x.RowNumber)
                .Select(MapLine)
                .ToList()
        );
    }

    private static BomImportLineResponseDto MapLine(BOMImportLine line)
    {
        return new BomImportLineResponseDto(
            line.Id,
            line.RowNumber,
            line.ProjectCode,
            line.ProjectName,
            line.FinishedGoodCode,
            line.FinishedGoodName,
            line.AssemblyCode,
            line.AssemblyName,
            line.PartNumber,
            line.Revision,
            line.Description,
            line.Quantity,
            line.Status.ToString(),
            line.ErrorMessage
        );
    }
}
