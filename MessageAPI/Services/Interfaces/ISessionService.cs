using InstagramApiSharp.API;
using MessageAPI.Models.Implementations;

namespace MessageAPI.Services.Interfaces
{
    public interface ISessionService
    {
        public Session this[Guid id] { get; }

        public Task AddSession(Session session);

        public Task RemoveSession(Guid id);

        public bool Exist(Guid id);
    }
}
