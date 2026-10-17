namespace SukaliCareApp.Models;

/// <summary>
/// Settings for the Firebase project used for sign-in and the community database.
/// To switch project, copy these three values from the Firebase console:
/// Project settings > General > Your apps > Web app (SDK setup and configuration).
/// </summary>
public static class FirebaseConfig
{
    // TODO: replace with the values from your own Firebase project.
    public const string ApiKey = "AIzaSyDGCPg-xuD0siGy_iSbTRJ28bgdYiEqPl8";
    public const string AuthDomain = "sukalicare-8e53d.firebaseapp.com";
    public const string ProjectId = "sukalicare-8e53d";

    public static string FirestoreBaseUrl =>
        $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
}
