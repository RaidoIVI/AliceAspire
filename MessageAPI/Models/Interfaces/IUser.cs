using InstagramApiSharp.Classes;

namespace MessageAPI.Models.Interfaces
{
    public interface IUser : IModel
    {
        public string InstaLogin { get; set; }
        public string InstaPasswordHash { get; set; }
        public StateData? State { get; set; }

        public string MailLogin { get; set; }
        public string MailPasswordHash { get; set; }
    }
}
