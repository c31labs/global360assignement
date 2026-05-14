using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Infrastructure.Persistence;

internal sealed class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _db;

    public TaskRepository(TaskFlowDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<TaskItem?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken) =>
        await _db.Tasks.AddAsync(task, cancellationToken).ConfigureAwait(false);

    public void Remove(TaskItem task) => _db.Tasks.Remove(task);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
