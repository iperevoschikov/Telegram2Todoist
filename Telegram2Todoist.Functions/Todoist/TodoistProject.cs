using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions.Todoist;

[PublicAPI]
public class TodoistProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("is_inbox_project")]
    public bool IsInboxProject { get; set; }
}