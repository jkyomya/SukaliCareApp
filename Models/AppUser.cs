
namespace SukaliCareApp.Models
{
    public class AppUser : BaseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; }
        public string FullName { get; private set; } = string.Empty;
        public string ProfilePhoto { get; set;} = string.Empty;
        public string Email { get; set; }
        public string Phone { get; set; } = string.Empty;  
        public DateTime DoB { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
