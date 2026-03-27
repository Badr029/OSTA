using Microsoft.EntityFrameworkCore;
using OSTA.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<OstaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapGet("/swagger", () => Results.Redirect("/openapi/v1.json"))
        .ExcludeFromDescription();
    app.MapGet("/swagger/index.html", () => Results.Redirect("/openapi/v1.json"))
        .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "OSTA.API",
    status = "running",
    docs = "/openapi/v1.json",
    health = "/health"
}))
    .ExcludeFromDescription();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .ExcludeFromDescription();

app.Run();
