using FluentAssertions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram2Todoist.Functions;

namespace Telegram2Todoist.Tests;

public class TelegramMessageEntitiesFormatterTests
{
    [Test]
    public void Test()
    {
        var message = new Message
        {
            Text = "Тест выделения жирным, курсивом, зачеркнутым и подчеркнутым, а также ссылка",
            Entities =
            [
                new MessageEntity
                {
                    Offset = 15,
                    Length = 6,
                    Type = MessageEntityType.Bold,
                },
                new MessageEntity
                {
                    Offset = 23,
                    Length = 8,
                    Type = MessageEntityType.Italic,
                },
                new MessageEntity
                {
                    Offset = 33,
                    Length = 10,
                    Type = MessageEntityType.Strikethrough,
                },
                new MessageEntity
                {
                    Offset = 46,
                    Length = 11,
                    Type = MessageEntityType.Underline,
                },
                new MessageEntity
                {
                    Offset = 67,
                    Length = 6,
                    Type = MessageEntityType.TextLink,
                    Url = "https://google.com/"
                },
            ]
        };

        var actual = TelegramMessageEntitiesFormatter.ToMarkdown(message);
        actual
            .Should()
            .Be("Тест выделения **жирным**, *курсивом*, ~зачеркнутым~ и _подчеркнутым_," +
                " а также [ссылка](https://google.com/)");
    }
}