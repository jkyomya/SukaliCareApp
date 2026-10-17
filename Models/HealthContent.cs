namespace SukaliCareApp.Models;

/// <summary>
/// Offline health information bundled with the app (Resources/Raw/health_content.json).
/// </summary>
public class HealthContent
{
    public string Version { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = string.Empty;
    public List<HealthTopic> Topics { get; set; } = [];
    public List<string> Sources { get; set; } = [];
}

public class HealthTopic
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
    public List<HealthArticle> Articles { get; set; } = [];
}

public class HealthArticle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<HealthSection> Sections { get; set; } = [];
    public List<string> KeyPoints { get; set; } = [];
}

public class HealthSection
{
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
