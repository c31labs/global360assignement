using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Tasks;

/// <summary>
/// A task is the only aggregate in this prototype. The domain encapsulates its own
/// invariants so the application layer cannot construct an invalid task.
/// </summary>
public sealed class TaskItem
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxAssigneeLength = 120;

    private TaskItem()
    {
        Title = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    public TaskItemPriority Priority { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public string? Assignee { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static TaskItem Create(
        string title,
        string? description,
        TaskItemPriority priority,
        DateTimeOffset? dueDate,
        string? assignee,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        var normalisedTitle = NormaliseTitle(title);
        var normalisedDescription = NormaliseDescription(description);
        var normalisedAssignee = NormaliseAssignee(assignee);
        EnsurePriorityDefined(priority);
        EnsureDueDateInFutureOrNull(dueDate, clock);

        var now = clock.GetUtcNow();
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = normalisedTitle,
            Description = normalisedDescription,
            Priority = priority,
            DueDate = dueDate,
            Assignee = normalisedAssignee,
            Status = TaskItemStatus.Todo,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void UpdateDetails(
        string title,
        string? description,
        TaskItemPriority priority,
        DateTimeOffset? dueDate,
        string? assignee,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        Title = NormaliseTitle(title);
        Description = NormaliseDescription(description);
        Assignee = NormaliseAssignee(assignee);
        EnsurePriorityDefined(priority);
        EnsureDueDateInFutureOrNull(dueDate, clock);
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = clock.GetUtcNow();
    }

    public void ChangeStatus(TaskItemStatus newStatus, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        if (!Enum.IsDefined(newStatus))
        {
            throw new DomainValidationException("Status is not a recognised value.");
        }

        if (Status == newStatus)
        {
            return;
        }

        Status = newStatus;
        UpdatedAt = clock.GetUtcNow();
    }

    private static string NormaliseTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Title is required.");
        }

        var trimmed = title.Trim();
        if (trimmed.Length > MaxTitleLength)
        {
            throw new DomainValidationException($"Title must be {MaxTitleLength} characters or fewer.");
        }

        return trimmed;
    }

    private static string? NormaliseDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > MaxDescriptionLength)
        {
            throw new DomainValidationException($"Description must be {MaxDescriptionLength} characters or fewer.");
        }

        return trimmed;
    }

    private static string? NormaliseAssignee(string? assignee)
    {
        if (assignee is null)
        {
            return null;
        }

        var trimmed = assignee.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > MaxAssigneeLength)
        {
            throw new DomainValidationException($"Assignee must be {MaxAssigneeLength} characters or fewer.");
        }

        return trimmed;
    }

    private static void EnsurePriorityDefined(TaskItemPriority priority)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new DomainValidationException("Priority is not a recognised value.");
        }
    }

    private static void EnsureDueDateInFutureOrNull(DateTimeOffset? dueDate, TimeProvider clock)
    {
        if (dueDate is null)
        {
            return;
        }

        var nowDate = clock.GetUtcNow().Date;
        if (dueDate.Value.UtcDateTime.Date < nowDate)
        {
            throw new DomainValidationException("Due date cannot be in the past.");
        }
    }
}
