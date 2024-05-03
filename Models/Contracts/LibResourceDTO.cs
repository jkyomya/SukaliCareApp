namespace SukaliCareApp.Models;

public record LibResourceDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; } = DateTime.Now;
    public string Author { get; set; } = string.Empty;
}
