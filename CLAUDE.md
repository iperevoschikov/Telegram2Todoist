# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A Telegram bot (`@todoist_forward_bot`) that turns forwarded/written Telegram messages into Todoist tasks. It runs as two Yandex Cloud Functions (not ASP.NET/Azure Functions) built on the `yandexcloudfunctions.net.sdk` package, deployed via a shared reusable GitHub Actions workflow (`iperevoschikov/YandexCloudFunctions.Net.Sdk`).

## Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test class/method
dotnet test --filter "FullyQualifiedName~TelegramMessageEntitiesFormatterTests"
dotnet test --filter "FullyQualifiedName~YcDeserializationTests.SomeMethodName"
```

Tests use NUnit + FluentAssertions. There is no local run/emulator setup — the functions are only meaningfully exercised via deployed Yandex Cloud Functions or unit tests.

Deployment is triggered by pushing a `v*.*.*` tag (see `.github/workflows/ci.yml`); it deploys both function entrypoints in a matrix and requires secrets (`YC_OAUTH_TOKEN`, AWS-compatible bucket creds, `FUNC_ENV_VARIABLES`). Do not attempt to run this workflow yourself.

## Architecture

Two independent Yandex Cloud Function entrypoints, each a `WebhookFunctionHandler` subclass from the YC SDK, sharing one DI container built in `ContainerConfiguration.ConfigureServices`:

- **`WebHookAsyncFunctionHandler`** — the Telegram webhook. Deserializes the incoming `Update`, looks up the sender's Todoist access token in `UsersStorage`. If missing, generates an auth state (`AuthStateStorage`) and replies with a Todoist OAuth link. Otherwise builds a task title (forwarded contact name → forwarded sender name → sender name → fallback) and a markdown description (via `TelegramMessageEntitiesFormatter`, plus any extracted inline-keyboard URLs), then creates the task through `TodoistApiClientFactory`.
- **`OAuthAsyncFunctionHandler`** — the Todoist OAuth redirect target. Validates `code`/`state` query params, resolves the Telegram user id from `AuthStateStorage`, exchanges the code for an access token via `TodoistAuthClient`, persists it in `UsersStorage`, notifies the user on Telegram, and redirects to the bot's t.me link.

Persistence is Firestore (`Telegram2Todoist.Functions/Storage/*`), using two collections: `users` (Telegram user id → Todoist access token) and `auth_states` (random state id → Telegram user id, used as CSRF-protection/correlation for the OAuth flow, expires after 3 days).

`Telegram2Todoist.Functions/Todoist/*` is a small hand-rolled Todoist API client: `TodoistAuthClient` for the OAuth code/token exchange, `TodoistApiClientFactory`/`TodoistApiClient` for creating tasks (per-request client carrying the caller's bearer token — not a singleton, since each user has their own token).

Configuration is read directly from environment variables in `ContainerConfiguration` (`TG_ACCESS_TOKEN`, `TODOIST_AUTH_CLIENT_ID`, `TODOIST_AUTH_CLIENT_SECRET`, `GOOGLE_CLOUD_JSON_CREDENTIALS` — base64-encoded service account JSON used to build the Firestore credential). Missing values throw at startup.

`TelegramMessageEntitiesFormatter.ToMarkdown` converts Telegram's `MessageEntity` list (bold/italic/strikethrough/underline/code/spoiler/text-link) into markdown by inserting wrapper strings at entity offsets, processed in descending offset order so earlier insertions don't shift later offsets.
