namespace SukaliCareApp.Models;

public class ClientDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; }
    public string ProfilePhoto { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DoB { get; set; }
    public DateTime RegisteredOn { get; set; }
    public DateTime LastSeenOn { get; set; }
}
