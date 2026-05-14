using FluentAssertions;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Domain.Tests.Tasks;

public class TaskItemTests
{
    private readonly TimeProvider _clock = new FakeTimeProvider(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_PopulatesAuditFieldsAndDefaults()
    {
        var task = TaskItem.Create("Plan campaign", "Outline brief", TaskItemPriority.Medium, dueDate: null, assignee: "Alex", _clock);

        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be("Plan campaign");
        task.Status.Should().Be(TaskItemStatus.Todo);
        task.CreatedAt.Should().Be(_clock.GetUtcNow());
        task.UpdatedAt.Should().Be(_clock.GetUtcNow());
    }

    [Fact]
    public void Create_TrimsWhitespaceAndCoercesEmptyOptionalsToNull()
    {
        var task = TaskItem.Create("  Plan  ", "   ", TaskItemPriority.Low, dueDate: null, assignee: "   ", _clock);

        task.Title.Should().Be("Plan");
        task.Description.Should().BeNull();
        task.Assignee.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_RejectsMissingTitle(string? title)
    {
        var act = () => TaskItem.Create(title!, null, TaskItemPriority.Low, null, null, _clock);

        act.Should().Throw<DomainValidationException>().WithMessage("*Title*");
    }

    [Fact]
    public void Create_RejectsTitleLongerThanLimit()
    {
        var oversized = new string('a', TaskItem.MaxTitleLength + 1);

        var act = () => TaskItem.Create(oversized, null, TaskItemPriority.Low, null, null, _clock);

        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Create_RejectsDueDateInPast()
    {
        var yesterday = _clock.GetUtcNow().AddDays(-1);

        var act = () => TaskItem.Create("title", null, TaskItemPriority.Low, yesterday, null, _clock);

        act.Should().Throw<DomainValidationException>().WithMessage("*past*");
    }

    [Fact]
    public void Create_RejectsUndefinedPriorityEnum()
    {
        var act = () => TaskItem.Create("title", null, (TaskItemPriority)42, null, null, _clock);

        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void ChangeStatus_BumpsUpdatedAt()
    {
        var task = TaskItem.Create("title", null, TaskItemPriority.Low, null, null, _clock);
        var later = new FakeTimeProvider(_clock.GetUtcNow().AddMinutes(5));

        task.ChangeStatus(TaskItemStatus.InProgress, later);

        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.UpdatedAt.Should().Be(later.GetUtcNow());
    }

    [Fact]
    public void ChangeStatus_NoOpWhenSameStatus_DoesNotBumpUpdatedAt()
    {
        var task = TaskItem.Create("title", null, TaskItemPriority.Low, null, null, _clock);
        var originalUpdatedAt = task.UpdatedAt;
        var later = new FakeTimeProvider(_clock.GetUtcNow().AddMinutes(5));

        task.ChangeStatus(TaskItemStatus.Todo, later);

        task.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void UpdateDetails_AppliesChangesAndBumpsUpdatedAt()
    {
        var task = TaskItem.Create("title", "desc", TaskItemPriority.Low, null, null, _clock);
        var later = new FakeTimeProvider(_clock.GetUtcNow().AddHours(1));

        task.UpdateDetails("new title", "new desc", TaskItemPriority.High, later.GetUtcNow().AddDays(2), "Bo", later);

        task.Title.Should().Be("new title");
        task.Description.Should().Be("new desc");
        task.Priority.Should().Be(TaskItemPriority.High);
        task.Assignee.Should().Be("Bo");
        task.UpdatedAt.Should().Be(later.GetUtcNow());
    }
}
