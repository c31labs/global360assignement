using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Abstractions;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;
    private readonly IValidator<ChangeStatusRequest> _statusValidator;
    private readonly TimeProvider _clock;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository repository,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        IValidator<ChangeStatusRequest> statusValidator,
        TimeProvider clock,
        ILogger<TaskService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
        _clock = clock;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskDto>> ListAsync(CancellationToken cancellationToken)
    {
        var tasks = await _repository.ListAsync(cancellationToken).ConfigureAwait(false);
        return tasks.Select(t => t.ToDto()).ToList();
    }

    public async Task<TaskDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.FindAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new TaskNotFoundException(id);

        return task.ToDto();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.Assignee,
            _clock);

        await _repository.AddAsync(task, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created task {TaskId}", task.Id);
        return task.ToDto();
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var task = await _repository.FindAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new TaskNotFoundException(id);

        task.UpdateDetails(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.Assignee,
            _clock);

        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Updated task {TaskId}", task.Id);
        return task.ToDto();
    }

    public async Task<TaskDto> ChangeStatusAsync(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        await _statusValidator.ValidateAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var task = await _repository.FindAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new TaskNotFoundException(id);

        task.ChangeStatus(request.Status, _clock);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Task {TaskId} status changed to {Status}", task.Id, task.Status);
        return task.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.FindAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new TaskNotFoundException(id);

        _repository.Remove(task);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted task {TaskId}", task.Id);
    }
}
