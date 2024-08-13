using InstagramApiSharp.API;
using MessageAPI.Models.Interfaces;

namespace MessageAPI.Models.Implementations
{
    public class Session : Interfaces.ISession
    {
        public Guid Id { get; set; }
        public IUser User { get; set; }


        [NonSerialized]
        private readonly IInstaApi _instaApi;
        public IInstaApi InstaApi { get => _instaApi; }

        public Session(IUser user, IInstaApi instaApi)
        {
            Id = Guid.NewGuid();
            User = user;
            _instaApi = instaApi;
        }
    }
}
