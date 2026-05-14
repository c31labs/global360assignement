using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks;
using TaskFlow.Application.Tasks.Dtos;

namespace TaskFlow.Api.Endpoints;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .WithOpenApi();

        group.MapGet("/", ListAsync)
            .WithName("ListTasks")
            .Produces<IReadOnlyList<TaskDto>>();

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetTask")
            .Produces<TaskDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateTask")
            .Produces<TaskDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateTask")
            .Produces<TaskDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .WithName("ChangeTaskStatus")
            .Produces<TaskDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteTask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(ITaskService service, CancellationToken cancellationToken)
    {
        var tasks = await service.ListAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(tasks);
    }

    private static async Task<IResult> GetAsync(Guid id, ITaskService service, CancellationToken cancellationToken)
    {
        var task = await service.GetAsync(id, cancellationToken).ConfigureAwait(false);
        return Results.Ok(task);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateTaskRequest request,
        ITaskService service,
        LinkGenerator links,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken).ConfigureAwait(false);
        var location = links.GetUriByName(http, "GetTask", new { id = created.Id });
        return Results.Created(location, created);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        ITaskService service,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(updated);
    }

    private static async Task<IResult> ChangeStatusAsync(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        ITaskService service,
        CancellationToken cancellationToken)
    {
        var updated = await service.ChangeStatusAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(updated);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ITaskService service, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
