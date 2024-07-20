namespace MessageAPI.Models.Implementations
{
    public class PostPhoto
    {
        public string ImageUri { get; set; } = "";
        public string[] UserTags { get; set; } = [];

        public string Caption { get; set; } = "";
    }
}
