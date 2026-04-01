using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.ProjectBomInstances;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/projects/{projectId:guid}/generate-from-bom")]
public class ProjectBomInstancesController : ControllerBase
{
    private readonly OstaDbContext _context;

    public ProjectBomInstancesController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectBomInstanceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectBomInstanceResponseDto>> Create(
        Guid projectId,
        [FromBody] GenerateProjectStructureFromBomRequestDto request)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Project '{projectId}' was not found."));
        }

        var bomHeader = await _context.BomHeaders
            .Include(x => x.ParentItemMaster)
            .Include(x => x.Items)
                .ThenInclude(x => x.ComponentItemMaster)
            .FirstOrDefaultAsync(x => x.Id == request.BomHeaderId);

        if (bomHeader is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"BOM header '{request.BomHeaderId}' was not found."));
        }

        var finishedGood = await _context.FinishedGoods
            .FirstOrDefaultAsync(x =>
                x.ProjectId == projectId &&
                x.Code == bomHeader.ParentItemMaster.Code);

        var finishedGoodAction = "Reused";

        if (finishedGood is null)
        {
            finishedGood = new FinishedGood
            {
                Id = Guid.NewGuid(),
                Code = bomHeader.ParentItemMaster.Code,
                Name = bomHeader.ParentItemMaster.Name,
                ProjectId = projectId,
                SourceItemMasterId = bomHeader.ParentItemMasterId,
                SourceBomHeaderId = bomHeader.Id
            };

            _context.FinishedGoods.Add(finishedGood);
            finishedGoodAction = "Created";
        }
        else
        {
            finishedGood.Name = bomHeader.ParentItemMaster.Name;
            finishedGood.SourceItemMasterId = bomHeader.ParentItemMasterId;
            finishedGood.SourceBomHeaderId = bomHeader.Id;
        }

        var existingAssemblies = await _context.Assemblies
            .Where(x => x.FinishedGoodId == finishedGood.Id)
            .ToDictionaryAsync(x => x.Code);

        var assemblyResponses = new List<ProjectBomInstanceAssemblyResponseDto>();

        foreach (var bomItem in bomHeader.Items
                     .OrderBy(x => x.SortOrder)
                     .ThenBy(x => x.ItemNumber))
        {
            var action = "Reused";

            if (!existingAssemblies.TryGetValue(bomItem.ComponentItemMaster.Code, out var assembly))
            {
                assembly = new Assembly
                {
                    Id = Guid.NewGuid(),
                    Code = bomItem.ComponentItemMaster.Code,
                    Name = bomItem.ComponentItemMaster.Name,
                    FinishedGoodId = finishedGood.Id,
                    SourceBomItemId = bomItem.Id,
                    SourceComponentItemMasterId = bomItem.ComponentItemMasterId
                };

                _context.Assemblies.Add(assembly);
                existingAssemblies.Add(assembly.Code, assembly);
                action = "Created";
            }
            else
            {
                assembly.Name = bomItem.ComponentItemMaster.Name;
                assembly.SourceBomItemId = bomItem.Id;
                assembly.SourceComponentItemMasterId = bomItem.ComponentItemMasterId;
            }

            assemblyResponses.Add(new ProjectBomInstanceAssemblyResponseDto(
                assembly.Id,
                assembly.Code,
                assembly.Name,
                action,
                bomItem.Id,
                bomItem.ComponentItemMasterId
            ));
        }

        await _context.SaveChangesAsync();

        return Ok(new ProjectBomInstanceResponseDto(
            projectId,
            bomHeader.Id,
            bomHeader.ParentItemMasterId,
            finishedGood.Id,
            finishedGood.Code,
            finishedGood.Name,
            finishedGoodAction,
            assemblyResponses
        ));
    }
}
