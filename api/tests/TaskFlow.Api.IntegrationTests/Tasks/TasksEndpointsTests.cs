using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Api.IntegrationTests.Tasks;

public class TasksEndpointsTests : IClassFixture<TaskFlowApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public TasksEndpointsTests(TaskFlowApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesTaskAndReturnsLocationHeader()
    {
        var request = new CreateTaskRequest("Draft brief", "for Q3 launch", TaskItemPriority.High, null, "Alex");

        var response = await _client.PostAsJsonAsync("/api/tasks", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var dto = await response.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        dto.Should().NotBeNull();
        dto!.Title.Should().Be("Draft brief");
        dto.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task Post_ReturnsValidationProblemWhenTitleMissing()
    {
        var invalid = new CreateTaskRequest("", null, TaskItemPriority.Low, null, null);

        var response = await _client.PostAsJsonAsync("/api/tasks", invalid, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Get_ReturnsCreatedTasksOrderedNewestFirst()
    {
        var first = await Create("First task");
        var second = await Create("Second task");

        var listResponse = await _client.GetAsync("/api/tasks");
        listResponse.EnsureSuccessStatusCode();
        var tasks = await listResponse.Content.ReadFromJsonAsync<List<TaskDto>>(JsonOptions);

        tasks.Should().NotBeNull();
        tasks!.Select(t => t.Id).Should().Contain(new[] { first.Id, second.Id });
    }

    [Fact]
    public async Task Patch_ChangesStatus()
    {
        var task = await Create("Plan workshop");

        var response = await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status",
            new ChangeStatusRequest(TaskItemStatus.Done), JsonOptions);

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        dto!.Status.Should().Be(TaskItemStatus.Done);
    }

    [Fact]
    public async Task Get_ReturnsNotFoundForMissingTask()
    {
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        var task = await Create("Delete me");

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{task.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var followup = await _client.GetAsync($"/api/tasks/{task.Id}");
        followup.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_ReportsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    private async Task<TaskDto> Create(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest(title, null, TaskItemPriority.Medium, null, null), JsonOptions);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        return dto!;
    }
}
