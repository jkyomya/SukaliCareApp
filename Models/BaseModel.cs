
using System.ComponentModel.DataAnnotations;

namespace SukaliCareApp.Models
{
    public abstract class BaseModel
    {
        [Key]
        public Guid Id { get; set; } 
    }
}
