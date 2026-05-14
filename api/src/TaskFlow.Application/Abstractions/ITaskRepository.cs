using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Abstractions;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken);

    Task<TaskItem?> FindAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    void Remove(TaskItem task);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
