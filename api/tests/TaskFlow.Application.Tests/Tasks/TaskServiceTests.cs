using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskFlow.Application.Abstractions;
using TaskFlow.Application.Tasks;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Application.Tasks.Validators;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tests.Tasks;

public class TaskServiceTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly TimeProvider _clock = new FakeTimeProvider(new DateTimeOffset(2026, 5, 14, 9, 0, 0, TimeSpan.Zero));

    private TaskService CreateSut() => new(
        _repository,
        new CreateTaskRequestValidator(),
        new UpdateTaskRequestValidator(),
        new ChangeStatusRequestValidator(),
        _clock,
        NullLogger<TaskService>.Instance);

    [Fact]
    public async Task CreateAsync_PersistsTaskAndReturnsDto()
    {
        var sut = CreateSut();
        var request = new CreateTaskRequest("Plan", null, TaskItemPriority.Medium, null, "Alex");

        var dto = await sut.CreateAsync(request, CancellationToken.None);

        dto.Title.Should().Be("Plan");
        dto.Status.Should().Be(TaskItemStatus.Todo);
        await _repository.Received(1).AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PropagatesValidationFailure()
    {
        var sut = CreateSut();
        var invalid = new CreateTaskRequest(Title: "", null, TaskItemPriority.Medium, null, null);

        var act = () => sut.CreateAsync(invalid, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_ThrowsTaskNotFound_WhenMissing()
    {
        var sut = CreateSut();
        _repository.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TaskItem?>(null));

        var act = () => sut.GetAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<TaskNotFoundException>();
    }

    [Fact]
    public async Task ChangeStatusAsync_UpdatesAndSaves()
    {
        var sut = CreateSut();
        var task = TaskItem.Create("Plan", null, TaskItemPriority.Medium, null, null, _clock);
        _repository.FindAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TaskItem?>(task));

        var dto = await sut.ChangeStatusAsync(task.Id, new ChangeStatusRequest(TaskItemStatus.Done), CancellationToken.None);

        dto.Status.Should().Be(TaskItemStatus.Done);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_RemovesAndSaves()
    {
        var sut = CreateSut();
        var task = TaskItem.Create("Plan", null, TaskItemPriority.Low, null, null, _clock);
        _repository.FindAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TaskItem?>(task));

        await sut.DeleteAsync(task.Id, CancellationToken.None);

        _repository.Received(1).Remove(task);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenTaskNotFound()
    {
        var sut = CreateSut();
        _repository.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TaskItem?>(null));

        var act = () => sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<TaskNotFoundException>();
    }
}
