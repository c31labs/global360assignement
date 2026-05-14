using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Dtos;

public sealed record ChangeStatusRequest(TaskItemStatus Status);
