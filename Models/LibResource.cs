
namespace SukaliCareApp.Models
{
    public class LibResource : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Thumbnail { get; set; } = string.Empty;
        public DateTime PublishedOn { get; set; } = DateTime.Now;
        public string Author {  get; set; }
    }
}
