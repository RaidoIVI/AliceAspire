using InstagramApiSharp.Classes;
using MessageAPI.Models.Interfaces;

namespace MessageAPI.Models.Implementations
{
    public class User : IUser
    {
        public Guid Id { get; set; }

        public required string InstaLogin { get; set; }
        public required string InstaPasswordHash { get; set; }
        public StateData? State { get; set; }

        public required string MailLogin { get; set; }
        public required string MailPasswordHash { get; set; }
    }
}
