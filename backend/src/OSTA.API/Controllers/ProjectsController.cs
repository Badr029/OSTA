using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.Projects;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/projects")]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public ProjectsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAll()
    {
        var projects = await _context.Projects
            .OrderBy(x => x.Code)
            .Select(x => new ProjectResponseDto(x.Id, x.Code, x.Name))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponseDto>> GetById(Guid id)
    {
        var project = await _context.Projects
            .Where(x => x.Id == id)
            .Select(x => new ProjectResponseDto(x.Id, x.Code, x.Name))
            .FirstOrDefaultAsync();

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectRequestDto request)
    {
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
