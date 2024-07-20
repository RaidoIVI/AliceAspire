using MessageAPI.Models.Implementations;
using MessageAPI.Services.Interfaces;

namespace MessageAPI.Services.Implementations
{
    public class SessionService : ISessionService
    {
        private readonly Dictionary<Guid, Session> _sessions = [];
        private readonly Mutex _mutex = new();

        public Session this[Guid id]
        {
            get
            {
                _mutex.WaitOne();
                Session session = _sessions[id];
                _mutex.ReleaseMutex();

                return session;
            }
        }

        public Task AddSession(Session session)
        {
            _mutex.WaitOne();

            _sessions.Add(session.Id, session);

            _mutex.ReleaseMutex();

            return Task.CompletedTask;
        }

        public Task RemoveSession(Guid sessionId)
        {
            _mutex.WaitOne();

            _sessions.Remove(sessionId);

            _mutex.ReleaseMutex();

            return Task.CompletedTask;
        }

        public bool Exist(Guid id)
        {
            _mutex.WaitOne();
            bool exist = _sessions.ContainsKey(id);
            _mutex.ReleaseMutex();

            return exist;
        }
    }
}
