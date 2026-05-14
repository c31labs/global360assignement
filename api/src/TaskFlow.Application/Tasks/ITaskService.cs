using TaskFlow.Application.Tasks.Dtos;

namespace TaskFlow.Application.Tasks;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> ListAsync(CancellationToken cancellationToken);

    Task<TaskDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);

    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken);

    Task<TaskDto> ChangeStatusAsync(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
