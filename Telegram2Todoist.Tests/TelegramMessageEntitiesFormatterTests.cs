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
            Text =
                @"\u0422\u0435\u0441\u0442 \u0432\u044b\u0434\u0435\u043b\u0435\u043d\u0438\u044f \u0436\u0438\u0440\u043d\u044b\u043c," +
                @" \u043a\u0443\u0440\u0441\u0438\u0432\u043e\u043c, \u0437\u0430\u0447\u0435\u0440\u043a\u043d\u0443\u0442\u043e" +
                @" \u0438 \u043f\u043e\u0434\u0447\u0435\u0440\u043a\u043d\u0443\u0442\u043e, \u0430 \u0442\u0430\u043a\u0436\u0435" +
                @" \u0441\u0441\u044b\u043b\u043a\u0430",
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
            .Be("Тест выделения **жирным**, *курсивом*, ~зачеркнуто~ и _подчеркнуто_," +
                " а также [ссылка](https://google.com/)");
    }
}