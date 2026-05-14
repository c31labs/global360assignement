using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Dtos;

public static class TaskMappingExtensions
{
    public static TaskDto ToDto(this TaskItem task) =>
        new(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.Assignee,
            task.CreatedAt,
            task.UpdatedAt);
}
