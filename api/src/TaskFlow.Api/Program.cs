using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskFlow.Api;
using TaskFlow.Api.Endpoints;
using TaskFlow.Api.Middleware;
using TaskFlow.Application;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

const string CorsPolicy = "TaskFlowWeb";
string[] defaultAllowedOrigins = { "http://localhost:4200" };

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, logger) => logger
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TaskFlow.Api"));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? defaultAllowedOrigins;
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "TaskFlow API", Version = "v1" });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskFlowDbContext>("database");

builder.Services.AddProblemDetails();

var app = builder.Build();

// Apply migrations / create schema at startup for the prototype. In production this would be a
// separate, deliberate step (CI job or sidecar) rather than running on every pod boot.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
    await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthChecks.WriteJsonResponse,
});

app.MapTasksEndpoints();

await app.RunAsync().ConfigureAwait(false);

public partial class Program;
