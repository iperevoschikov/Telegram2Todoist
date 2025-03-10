﻿using System.Text.Json;
using FluentAssertions;
using Telegram2Todoist.Functions;

namespace Telegram2Todoist.Tests;

public class YcDeserializationTests
{
    [Test]
    public void TestRequestDeserialization()
    {
        const string request =
            """
            {
             "httpMethod": "POST",
             "headers": {
               "Accept": "*/*",
               "Content-Length": "13",
               "Content-Type": "application/x-www-form-urlencoded",
               "User-Agent": "curl/7.58.0",
               "X-Real-Remote-Address": "[88.99.0.24]:37310",
               "X-Request-Id": "cd0d12cd-c5f1-4348-9dff-c50a78f1eb79",
               "X-Trace-Id": "92c5ad34-54f7-41df-a368-d4361bf376eb"
             },
             "path": "",
             "multiValueHeaders": {
               "Accept": [ "*/*" ],
               "Content-Length": [ "13" ],
               "Content-Type": [ "application/x-www-form-urlencoded" ],
               "User-Agent": [ "curl/7.58.0" ],
               "X-Real-Remote-Address": [ "[88.99.0.24]:37310" ],
               "X-Request-Id": [ "cd0d12cd-c5f1-4348-9dff-c50a78f1eb79" ],
               "X-Trace-Id": [ "92c5ad34-54f7-41df-a368-d4361bf376eb" ]
             },
             "queryStringParameters": {
               "a": "2",
               "b": "1"
             },
             "multiValueQueryStringParameters": {
               "a": [ "1", "2" ],
               "b": [ "1" ]
             },
             "requestContext": {
               "identity": {
                 "sourceIp": "88.99.0.24",
                 "userAgent": "curl/7.58.0"
               },
               "httpMethod": "POST",
               "requestId": "cd0d12cd-c5f1-4348-9dff-c50a78f1eb79",
               "requestTime": "26/Dec/2019:14:22:07 +0000",
               "requestTimeEpoch": 1577370127
             },
             "body": "aGVsbG8sIHdvcmxkIQ==",
             "isBase64Encoded": true
            }
            """;

        var deserialize = () => JsonSerializer.Deserialize<FunctionHandlerRequest>(request);
        deserialize.Should().NotThrow();

        var result = deserialize();
        result.Should().NotBeNull();
        result!.QueryStringParameters.Should().NotBeEmpty();
    }
}