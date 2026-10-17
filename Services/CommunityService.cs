using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SukaliCareApp.Services;

/// <summary>
/// Support groups and group chat, stored in Cloud Firestore and accessed through its REST API.
/// Access rules are enforced on the server by firestore.rules (in the repository root).
///
/// Data layout:
///   groups/{groupId}                 group details (created by moderators)
///   groups/{groupId}/members/{uid}   membership
///   groups/{groupId}/posts/{postId}  messages
///   reports/{reportId}               posts reported by members
///   moderators/{uid}                 who can hide posts (added in the console)
/// </summary>
public class CommunityService(FirebaseService firebaseService)
{
    private readonly FirebaseService firebase = firebaseService;
    private static readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public const int MaxPostLength = 1000;

    public string CurrentUserId => firebase.CurrentUserId;

    // ---------- Groups ----------

    public async Task<IReadOnlyList<SupportGroup>> GetGroupsAsync()
    {
        JsonNode result = await SendAsync(HttpMethod.Get, "groups?pageSize=100");
        var groups = new List<SupportGroup>();
        if (result?["documents"] is JsonArray docs)
        {
            foreach (JsonNode doc in docs)
            {
                if (doc is null) continue;
                JsonNode f = doc["fields"];
                groups.Add(new SupportGroup
                {
                    Id = LastSegment(doc["name"]?.GetValue<string>()),
                    Name = Str(f, "name"),
                    Description = Str(f, "description"),
                    Kind = Str(f, "kind"),
                    Facility = Str(f, "facility"),
                    District = Str(f, "district"),
                    MeetingDay = Str(f, "meetingDay"),
                    MeetingTime = Str(f, "meetingTime"),
                    ContactName = Str(f, "contactName"),
                    ContactPhone = Str(f, "contactPhone"),
                });
            }
        }
        return groups.OrderBy(g => g.Name).ToList();
    }

    // ---------- Membership ----------

    public async Task<bool> IsMemberAsync(string groupId)
    {
        string uid = RequireUser();
        return await ExistsAsync($"groups/{groupId}/members/{uid}");
    }

    public async Task JoinAsync(string groupId, string displayName)
    {
        string uid = RequireUser();
        var body = new JsonObject
        {
            ["fields"] = new JsonObject
            {
                ["displayName"] = StringValue(displayName),
                ["joinedAt"] = TimestampValue(DateTime.UtcNow)
            }
        };
        try
        {
            await SendAsync(HttpMethod.Post, $"groups/{groupId}/members?documentId={Uri.EscapeDataString(uid)}", body);
        }
        catch (CommunityException ex) when (ex.Status == HttpStatusCode.Conflict)
        {
            // Already a member.
        }
    }

    public async Task LeaveAsync(string groupId)
    {
        string uid = RequireUser();
        await SendAsync(HttpMethod.Delete, $"groups/{groupId}/members/{uid}");
    }

    public async Task<bool> IsModeratorAsync()
    {
        string uid = firebase.CurrentUserId;
        if (string.IsNullOrEmpty(uid)) return false;
        try
        {
            return await ExistsAsync($"moderators/{uid}");
        }
        catch (CommunityException)
        {
            return false;
        }
    }

    // ---------- Posts ----------

    public async Task<IReadOnlyList<CommunityPost>> GetPostsAsync(string groupId, bool includeHidden = false)
    {
        var query = new JsonObject
        {
            ["structuredQuery"] = new JsonObject
            {
                ["from"] = new JsonArray(new JsonObject { ["collectionId"] = "posts" }),
                ["orderBy"] = new JsonArray(new JsonObject
                {
                    ["field"] = new JsonObject { ["fieldPath"] = "createdAt" },
                    ["direction"] = "DESCENDING"
                }),
                ["limit"] = 100
            }
        };

        JsonNode result = await SendAsync(HttpMethod.Post, $"groups/{groupId}:runQuery", query);
        string me = firebase.CurrentUserId;
        var posts = new List<CommunityPost>();
        if (result is JsonArray rows)
        {
            foreach (JsonNode row in rows)
            {
                JsonNode doc = row?["document"];
                if (doc is null) continue;
                JsonNode f = doc["fields"];
                var post = new CommunityPost
                {
                    Id = LastSegment(doc["name"]?.GetValue<string>()),
                    GroupId = groupId,
                    AuthorUid = Str(f, "authorUid"),
                    AuthorName = Str(f, "authorName"),
                    Text = Str(f, "text"),
                    CreatedAt = Time(f, "createdAt"),
                    Hidden = Bool(f, "hidden"),
                };
                post.IsMine = post.AuthorUid == me;
                if (!post.Hidden || includeHidden)
                {
                    posts.Add(post);
                }
            }
        }
        // Oldest first, like a chat.
        return posts.OrderBy(p => p.CreatedAt).ToList();
    }

    public async Task AddPostAsync(string groupId, string authorName, string text)
    {
        string uid = RequireUser();
        text = (text ?? string.Empty).Trim();
        if (text.Length == 0) return;
        if (text.Length > MaxPostLength) text = text[..MaxPostLength];

        var body = new JsonObject
        {
            ["fields"] = new JsonObject
            {
                ["authorUid"] = StringValue(uid),
                ["authorName"] = StringValue(authorName),
                ["text"] = StringValue(text),
                ["createdAt"] = TimestampValue(DateTime.UtcNow),
                ["hidden"] = new JsonObject { ["booleanValue"] = false }
            }
        };
        await SendAsync(HttpMethod.Post, $"groups/{groupId}/posts", body);
    }

    public async Task DeleteOwnPostAsync(CommunityPost post)
    {
        await SendAsync(HttpMethod.Delete, $"groups/{post.GroupId}/posts/{post.Id}");
    }

    /// <summary>Moderators only: hide or show a post.</summary>
    public async Task SetHiddenAsync(CommunityPost post, bool hidden)
    {
        var body = new JsonObject
        {
            ["fields"] = new JsonObject { ["hidden"] = new JsonObject { ["booleanValue"] = hidden } }
        };
        await SendAsync(HttpMethod.Patch,
            $"groups/{post.GroupId}/posts/{post.Id}?updateMask.fieldPaths=hidden", body);
    }

    public async Task ReportPostAsync(CommunityPost post, string reason)
    {
        string uid = RequireUser();
        var body = new JsonObject
        {
            ["fields"] = new JsonObject
            {
                ["groupId"] = StringValue(post.GroupId),
                ["postId"] = StringValue(post.Id),
                ["postText"] = StringValue(post.Text),
                ["reporterUid"] = StringValue(uid),
                ["reason"] = StringValue(reason ?? string.Empty),
                ["createdAt"] = TimestampValue(DateTime.UtcNow)
            }
        };
        await SendAsync(HttpMethod.Post, "reports", body);
    }

    // ---------- Helpers ----------

    private string RequireUser()
    {
        string uid = firebase.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            throw new CommunityException("Please sign in to use the community.", HttpStatusCode.Unauthorized);
        }
        return uid;
    }

    private async Task<bool> ExistsAsync(string path)
    {
        try
        {
            await SendAsync(HttpMethod.Get, path);
            return true;
        }
        catch (CommunityException ex) when (ex.Status == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private async Task<JsonNode> SendAsync(HttpMethod method, string path, JsonNode body = null)
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new CommunityException("No internet connection. Please try again when you are online.", HttpStatusCode.ServiceUnavailable);
        }

        string token = await firebase.GetIdTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            throw new CommunityException("Please sign in to use the community.", HttpStatusCode.Unauthorized);
        }

        using var request = new HttpRequestMessage(method, $"{FirebaseConfig.FirestoreBaseUrl}/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage response = await http.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Firestore {method} {path} failed: {(int)response.StatusCode} {json}");
            string message = response.StatusCode switch
            {
                HttpStatusCode.Forbidden => "You do not have permission to do that. You may need to join the group first.",
                HttpStatusCode.NotFound => "Not found.",
                _ => "The community service is not available right now. Please try again later."
            };
            throw new CommunityException(message, response.StatusCode);
        }

        return string.IsNullOrWhiteSpace(json) ? null : JsonNode.Parse(json);
    }

    private static JsonObject StringValue(string value) => new() { ["stringValue"] = value ?? string.Empty };

    private static JsonObject TimestampValue(DateTime utc) =>
        new() { ["timestampValue"] = utc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture) };

    private static string Str(JsonNode fields, string name) =>
        fields?[name]?["stringValue"]?.GetValue<string>() ?? string.Empty;

    private static bool Bool(JsonNode fields, string name) =>
        fields?[name]?["booleanValue"]?.GetValue<bool>() ?? false;

    private static DateTime Time(JsonNode fields, string name)
    {
        string raw = fields?[name]?["timestampValue"]?.GetValue<string>();
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime t)
            ? DateTime.SpecifyKind(t, DateTimeKind.Utc)
            : DateTime.MinValue;
    }

    private static string LastSegment(string name) =>
        string.IsNullOrEmpty(name) ? string.Empty : name[(name.LastIndexOf('/') + 1)..];
}

public class CommunityException(string message, HttpStatusCode status) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}
