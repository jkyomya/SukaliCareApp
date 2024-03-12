namespace SukaliCareApp.Models;

public class ReadingEntity : BaseModel
{
    public DateTime ReadingDate { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public float Value { get; set; }
    public string Unit { get; set; }
}
