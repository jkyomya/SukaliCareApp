namespace SukaliCareApp.Models;

public class CgmReading : BaseModel
{
	public GlucoseValue GlucoseValue { get; set; }
	public DateTime ReadingDate { get;set; }
	public string UserId { get; set; }
	public string UserName { get; set; }
   
}
