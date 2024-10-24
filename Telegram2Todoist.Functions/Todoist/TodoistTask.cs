using System.Text.Json.Serialization;

namespace Telegram2Todoist.Functions.Todoist;

public class TodoistTask
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("content")]
    public string Content { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("project_id")]
    public string ProjectId { get; init; }
}