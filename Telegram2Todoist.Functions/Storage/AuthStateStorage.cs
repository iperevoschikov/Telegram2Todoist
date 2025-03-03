using Google.Cloud.Firestore;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions.Storage;

[UsedImplicitly]
public class AuthStateStorage(FirestoreDb firestoreDb)
{
    private const string UserIdFieldName = "userId";

    public async Task<string> CreateAuthState(long userId)
    {
        var id = Guid.NewGuid().ToString("N");
        var documentReference = GetDocumentReference(id);
        await documentReference.SetAsync(new Dictionary<string, object>
        {
            [UserIdFieldName] = userId
        });
        return id;
    }

    public async Task<long?> GetUserIdForState(string id)
    {
        var documentReference = GetDocumentReference(id);
        var document = await documentReference.GetSnapshotAsync();
        if (!document.Exists)
            return null;

        return document.GetValue<long>(UserIdFieldName);
    }

    private DocumentReference GetDocumentReference(string id) =>
        firestoreDb
            .Collection("auth_states")
            .Document(id);
}