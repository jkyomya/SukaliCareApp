
namespace SukaliCareApp.Models
{
    public class CGMDevice : BaseModel
    {
        public string Uuid { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public DateTime LastConneted { get; set; } = DateTime.Now;
        public bool IsConnected { get; set; } = false;
    }
}
