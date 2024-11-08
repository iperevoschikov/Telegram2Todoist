using System.Text.Json.Serialization;

namespace Telegram2Todoist.Functions.Todoist;

public class TodoistTask(string? id, string content, string? description, string projectId, string dueDate)
{
    [JsonPropertyName("id")]
    public string? Id { get; init; } = id;

    [JsonPropertyName("content")]
    public string Content { get; init; } = content;

    [JsonPropertyName("description")]
    public string? Description { get; init; } = description;

    [JsonPropertyName("project_id")]
    public string ProjectId { get; init; } = projectId;

    [JsonPropertyName("due_date")]
    public string DueDate { get; init; } = dueDate;
}