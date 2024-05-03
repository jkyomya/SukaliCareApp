namespace SukaliCareApp.Models;

public class GlucoseReadingDTO
{
    public Guid Id { get; set; }
    public float GlucoseLevel { get; set; }
    public string ReadingUnits { get; set; } = string.Empty;
    public DateTime ReadingDateTime { get; set; }
    public Guid ReadingId { get; set; }
    public string Note { get; set; }
    public Guid ClientId { get; set; }
}
