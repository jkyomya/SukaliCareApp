namespace SukaliCareApp.Models;

/// <summary>A diabetes support group. Created by moderators in the Firebase console.</summary>
public class SupportGroup
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>"in-person", "online" or "both".</summary>
    public string Kind { get; set; } = "both";
    public string Facility { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string MeetingDay { get; set; } = string.Empty;
    public string MeetingTime { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    public bool HasLocation => !string.IsNullOrWhiteSpace(Facility) || !string.IsNullOrWhiteSpace(District);
    public bool HasMeeting => !string.IsNullOrWhiteSpace(MeetingDay) || !string.IsNullOrWhiteSpace(MeetingTime);
    public bool HasContact => !string.IsNullOrWhiteSpace(ContactPhone);

    public string LocationText => string.Join(", ",
        new[] { Facility, District }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public string MeetingText => string.Join(", ",
        new[] { MeetingDay, MeetingTime }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public string KindText => Kind?.ToLowerInvariant() switch
    {
        "in-person" => "Meets in person",
        "online" => "Online group",
        _ => "Meets in person and online"
    };
}

/// <summary>A message posted in a support group.</summary>
public class CommunityPost
{
    public string Id { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string AuthorUid { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Hidden { get; set; }

    /// <summary>Set by the app: true when the signed-in user wrote this post.</summary>
    public bool IsMine { get; set; }

    public string TimeText
    {
        get
        {
            DateTime local = CreatedAt.ToLocalTime();
            TimeSpan age = DateTime.Now - local;
            if (age.TotalMinutes < 1) return "Just now";
            if (age.TotalHours < 1) return $"{(int)age.TotalMinutes} min ago";
            if (age.TotalDays < 1) return $"{(int)age.TotalHours} h ago";
            return local.ToString("d MMM yyyy, HH:mm");
        }
    }
}
