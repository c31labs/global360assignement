using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Dtos;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskItemPriority Priority,
    DateTimeOffset? DueDate,
    string? Assignee);
