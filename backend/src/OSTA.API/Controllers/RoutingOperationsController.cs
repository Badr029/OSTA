using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.RoutingOperations;
using OSTA.API.Infrastructure;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Controllers;

[ApiController]
[Route("api/v1/routing-templates/{routingTemplateId:guid}/operations")]
public class RoutingOperationsController : ControllerBase
{
    private readonly OstaDbContext _context;

    public RoutingOperationsController(OstaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoutingOperationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RoutingOperationResponseDto>>> GetAll(Guid routingTemplateId)
    {
        var routingTemplateExists = await _context.RoutingTemplates.AnyAsync(x => x.Id == routingTemplateId);

        if (!routingTemplateExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Routing template '{routingTemplateId}' was not found."));
        }

        var operations = await _context.RoutingOperations
            .Where(x => x.RoutingTemplateId == routingTemplateId)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.OperationNumber)
            .Select(x => new RoutingOperationResponseDto(
                x.Id,
                x.RoutingTemplateId,
                x.OperationNumber,
                x.OperationCode,
                x.OperationName,
                x.WorkCenterId,
                x.WorkCenter.Code,
                x.WorkCenter.Name,
                x.SetupTimeMinutes,
                x.RunTimeMinutes,
                x.Sequence,
                x.IsQcGate))
            .ToListAsync();

        return Ok(operations);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoutingOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutingOperationResponseDto>> GetById(Guid routingTemplateId, Guid id)
    {
        var operation = await _context.RoutingOperations
            .Where(x => x.RoutingTemplateId == routingTemplateId && x.Id == id)
            .Select(x => new RoutingOperationResponseDto(
                x.Id,
                x.RoutingTemplateId,
                x.OperationNumber,
                x.OperationCode,
                x.OperationName,
                x.WorkCenterId,
                x.WorkCenter.Code,
                x.WorkCenter.Name,
                x.SetupTimeMinutes,
                x.RunTimeMinutes,
                x.Sequence,
                x.IsQcGate))
            .FirstOrDefaultAsync();

        if (operation is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound(
                $"Routing operation '{id}' was not found for routing template '{routingTemplateId}'."));
        }

        return Ok(operation);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoutingOperationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoutingOperationResponseDto>> Create(Guid routingTemplateId, [FromBody] CreateRoutingOperationRequestDto request)
    {
        var routingTemplateExists = await _context.RoutingTemplates.AnyAsync(x => x.Id == routingTemplateId);

        if (!routingTemplateExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Routing template '{routingTemplateId}' was not found."));
        }

        var workCenterExists = await _context.WorkCenters.AnyAsync(x => x.Id == request.WorkCenterId);

        if (!workCenterExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work center '{request.WorkCenterId}' was not found."));
        }

        var duplicateOperation = await _context.RoutingOperations.AnyAsync(x =>
            x.RoutingTemplateId == routingTemplateId &&
            x.OperationNumber == request.OperationNumber);

        if (duplicateOperation)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict(
                $"A routing operation with operation number '{request.OperationNumber}' already exists for routing template '{routingTemplateId}'."));
        }

        var operation = new RoutingOperation
        {
            Id = Guid.NewGuid(),
            RoutingTemplateId = routingTemplateId,
            OperationNumber = request.OperationNumber.Trim(),
            OperationCode = request.OperationCode.Trim(),
            OperationName = request.OperationName.Trim(),
            WorkCenterId = request.WorkCenterId,
            SetupTimeMinutes = request.SetupTimeMinutes,
            RunTimeMinutes = request.RunTimeMinutes,
            Sequence = request.Sequence,
            IsQcGate = request.IsQcGate
        };

        _context.RoutingOperations.Add(operation);
        await _context.SaveChangesAsync();

        var createdOperation = await _context.RoutingOperations
            .Where(x => x.Id == operation.Id)
            .Select(x => new RoutingOperationResponseDto(
                x.Id,
                x.RoutingTemplateId,
                x.OperationNumber,
                x.OperationCode,
                x.OperationName,
                x.WorkCenterId,
                x.WorkCenter.Code,
                x.WorkCenter.Name,
                x.SetupTimeMinutes,
                x.RunTimeMinutes,
                x.Sequence,
                x.IsQcGate))
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { routingTemplateId, id = operation.Id }, createdOperation);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoutingOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoutingOperationResponseDto>> Update(
        Guid routingTemplateId,
        Guid id,
        [FromBody] UpdateRoutingOperationRequestDto request)
    {
        var operation = await _context.RoutingOperations
            .FirstOrDefaultAsync(x => x.RoutingTemplateId == routingTemplateId && x.Id == id);

        if (operation is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound(
                $"Routing operation '{id}' was not found for routing template '{routingTemplateId}'."));
        }

        var workCenterExists = await _context.WorkCenters.AnyAsync(x => x.Id == request.WorkCenterId);

        if (!workCenterExists)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound($"Work center '{request.WorkCenterId}' was not found."));
        }

        var duplicateOperation = await _context.RoutingOperations.AnyAsync(x =>
            x.RoutingTemplateId == routingTemplateId &&
            x.Id != id &&
            x.OperationNumber == request.OperationNumber);

        if (duplicateOperation)
        {
            return Conflict(ApiProblemDetailsFactory.Conflict(
                $"A routing operation with operation number '{request.OperationNumber}' already exists for routing template '{routingTemplateId}'."));
        }

        operation.OperationNumber = request.OperationNumber.Trim();
        operation.OperationCode = request.OperationCode.Trim();
        operation.OperationName = request.OperationName.Trim();
        operation.WorkCenterId = request.WorkCenterId;
        operation.SetupTimeMinutes = request.SetupTimeMinutes;
        operation.RunTimeMinutes = request.RunTimeMinutes;
        operation.Sequence = request.Sequence;
        operation.IsQcGate = request.IsQcGate;

        await _context.SaveChangesAsync();

        var updatedOperation = await _context.RoutingOperations
            .Where(x => x.Id == operation.Id)
            .Select(x => new RoutingOperationResponseDto(
                x.Id,
                x.RoutingTemplateId,
                x.OperationNumber,
                x.OperationCode,
                x.OperationName,
                x.WorkCenterId,
                x.WorkCenter.Code,
                x.WorkCenter.Name,
                x.SetupTimeMinutes,
                x.RunTimeMinutes,
                x.Sequence,
                x.IsQcGate))
            .FirstAsync();

        return Ok(updatedOperation);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid routingTemplateId, Guid id)
    {
        var operation = await _context.RoutingOperations
            .FirstOrDefaultAsync(x => x.RoutingTemplateId == routingTemplateId && x.Id == id);

        if (operation is null)
        {
            return NotFound(ApiProblemDetailsFactory.NotFound(
                $"Routing operation '{id}' was not found for routing template '{routingTemplateId}'."));
        }

        _context.RoutingOperations.Remove(operation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
