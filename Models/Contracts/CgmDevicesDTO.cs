namespace SukaliCareApp.Models;

public class CgmDevicesDTO
{
    public Guid Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Uuid { get; set; }
    public string SerialNumber { get; set; }
    public string MacAddress { get; set; }
    public DateTime LastConnected { get; set; }
    public Guid ClientId { get; set; }
    public ClientDTO Owner { get; set; }
}
