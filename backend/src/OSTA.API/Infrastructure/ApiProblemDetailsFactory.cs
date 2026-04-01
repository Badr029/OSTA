using Microsoft.AspNetCore.Mvc;

namespace OSTA.API.Infrastructure;

internal static class ApiProblemDetailsFactory
{
    public static ProblemDetails BadRequest(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid request",
        Detail = detail
    };

    public static ProblemDetails NotFound(string detail) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Resource not found",
        Detail = detail
    };

    public static ProblemDetails Conflict(string detail) => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Duplicate resource",
        Detail = detail
    };
}
