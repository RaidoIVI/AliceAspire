namespace MessageAPI.Models.Implementations
{
    public class PostPhoto
    {
        public string ImageUri { get; set; } = "";
        public IEnumerable<string> UserTags { get; set; } = [];

        public string Caption { get; set; } = "";
    }
}
