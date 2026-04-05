using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OSTA.API.Development;
using OSTA.API.Imports;
using OSTA.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
const string FrontendDevCorsPolicy = "FrontendDevCors";

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendDevCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:4173",
                "http://127.0.0.1:4173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddScoped<ConveyorProductDefinitionSeeder>();
builder.Services.AddScoped<QccSupervisorDemoSeeder>();
builder.Services.AddScoped<BomImportProcessingService>();
builder.Services.AddScoped<BomImportTemplateMapper>();
builder.Services.AddScoped<CsvFileReader>();

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

    app.MapPost("/dev/seeds/conveyor-product-definition", async (ConveyorProductDefinitionSeeder seeder, CancellationToken cancellationToken) =>
        Results.Ok(await seeder.SeedAsync(cancellationToken)))
        .ExcludeFromDescription();

    app.MapPost("/dev/seeds/qcc-supervisor-demo", async (QccSupervisorDemoSeeder seeder, CancellationToken cancellationToken) =>
        Results.Ok(await seeder.SeedAsync(cancellationToken)))
        .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseCors(FrontendDevCorsPolicy);
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
