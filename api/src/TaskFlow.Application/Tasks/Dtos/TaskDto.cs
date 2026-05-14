using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Dtos;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskItemPriority Priority,
    DateTimeOffset? DueDate,
    string? Assignee,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
