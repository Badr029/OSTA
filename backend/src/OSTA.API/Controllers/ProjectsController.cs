using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.Projects;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public ProjectsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAll()
    {
        var projects = await _context.Projects
            .OrderBy(x => x.Code)
            .Select(x => new ProjectResponseDto(x.Id, x.Code, x.Name))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponseDto>> GetById(Guid id)
    {
        var project = await _context.Projects
            .Where(x => x.Id == id)
            .Select(x => new ProjectResponseDto(x.Id, x.Code, x.Name))
            .FirstOrDefaultAsync();

        if (project is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Project '{id}' was not found."));
        }

        return Ok(project);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectRequestDto request)
    {
        var duplicateCode = await _context.Projects.AnyAsync(x => x.Code == request.Code);

        if (duplicateCode)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict($"A project with code '{request.Code}' already exists."));
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var response = new ProjectResponseDto(project.Id, project.Code, project.Name);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response);
    }
}
