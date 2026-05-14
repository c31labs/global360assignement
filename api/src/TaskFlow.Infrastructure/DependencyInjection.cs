using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Abstractions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TaskFlow")
            ?? "Data Source=taskflow.db";

        services.AddDbContext<TaskFlowDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
