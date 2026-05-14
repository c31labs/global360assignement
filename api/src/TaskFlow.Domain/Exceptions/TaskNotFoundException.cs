namespace TaskFlow.Domain.Exceptions;

public sealed class TaskNotFoundException : Exception
{
    public TaskNotFoundException(Guid id)
        : base($"Task with id '{id}' was not found.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
