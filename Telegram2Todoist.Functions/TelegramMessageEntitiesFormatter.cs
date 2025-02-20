using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram2Todoist.Functions;

public static partial class TelegramMessageEntitiesFormatter
{
    public static string ToMarkdown(Message? message)
    {
        var result = new StringBuilder();
        if (message?.Text != null)
        {
            result.AppendLine(DecodeUnicodeEscapeSequences(message.Text));
            if (message.Entities != null)
            {
                var entities = message
                    .Entities
                    //Сортируем по убыванию, чтобы не сбивались офсеты при форматировании
                    .OrderByDescending(e => e.Offset);

                foreach (var entity in entities)
                {
                    Format(result, entity);
                }
            }
        }

        if (message?.Caption != null)
        {
            result.AppendLine(DecodeUnicodeEscapeSequences(message.Caption));
        }

        return result.ToString();
    }

    private static void Format(StringBuilder message, MessageEntity messageEntity)
    {
        switch (messageEntity.Type)
        {
            case MessageEntityType.Bold:
                WrapFragment(message, messageEntity, "**", "**");
                return;
            case MessageEntityType.Italic:
                WrapFragment(message, messageEntity, "*", "*");
                return;
            case MessageEntityType.Strikethrough:
                WrapFragment(message, messageEntity, "~", "~");
                return;
            case MessageEntityType.Underline:
                WrapFragment(message, messageEntity, "_", "_");
                return;
            case MessageEntityType.Code:
                WrapFragment(message, messageEntity, "```\n", "\n```");
                return;
            case MessageEntityType.Spoiler:
                WrapFragment(message, messageEntity, "||", "||");
                return;
            case MessageEntityType.TextLink:
                WrapFragment(message, messageEntity, "[", $"]({messageEntity.Url})");
                return;
            default:
                return;
        }
    }

    private static void WrapFragment(StringBuilder message,
        MessageEntity messageEntity,
        string prepend,
        string append)
    {
        message.Insert(messageEntity.Offset + messageEntity.Length, append);
        message.Insert(messageEntity.Offset, prepend);
    }

    private static string DecodeUnicodeEscapeSequences(string input)
    {
        return UnicodeEscapeSequenceRegex().Replace(input, match => char.ConvertFromUtf32(Convert.ToInt32(match.Groups[1].Value, 16)));
    }

    [GeneratedRegex(@"\\u([0-9A-Fa-f]{4})")]
    private static partial Regex UnicodeEscapeSequenceRegex();
}