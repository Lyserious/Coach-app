using Coach_app.Models;

namespace Coach_app.Services.Auth
{
    public interface ISessionService
    {
        Coach CurrentCoach { get; }
        void SetSession(Coach coach);
        void ClearSession();
    }

    public class SessionService : ISessionService
    {
        public Coach CurrentCoach { get; private set; }

        public void SetSession(Coach coach)
        {
            CurrentCoach = coach;
        }

        public void ClearSession()
        {
            CurrentCoach = null;
        }
    }
}