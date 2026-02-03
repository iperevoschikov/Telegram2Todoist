using System.Text.Json.Serialization;

namespace Telegram2Todoist.Functions.Todoist;

public class TodoistProject(string id)
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = id;

    [JsonPropertyName("is_inbox_project")]
    public bool IsInboxProject { get; set; }
}

