using Google.Cloud.Firestore;

namespace Telegram2Todoist.Functions.Storage;

public class UsersStorage(FirestoreDb firestoreDb)
{
    private const string TodoistApiTokenFieldName = "todoist_access_token";

    public async Task<string?> GetAccessTokenFor(long userId)
    {
        var user = await GetDocumentReference(userId).GetSnapshotAsync();

        return !user.Exists || !user.ContainsField(TodoistApiTokenFieldName)
            ? null
            : user.GetValue<string>(TodoistApiTokenFieldName);
    }

    public async Task SetAccessTokenFor(long userId, string authToken)
    {
        await GetDocumentReference(userId)
            .SetAsync(new Dictionary<string, object> { [TodoistApiTokenFieldName] = authToken });
    }

    private DocumentReference GetDocumentReference(long userId) =>
        firestoreDb.Collection("users").Document(userId.ToString());
}

