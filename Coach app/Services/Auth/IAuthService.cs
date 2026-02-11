using Coach_app.Models.Domains.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> HasAnyCoachAsync(); // Pour savoir si on lance l'écran "Création" ou "Login"
        Task<List<Coach>> GetAvailableCoachesAsync();
        Task<bool> CreateCoachAsync(string name, string password);
        Task<Coach> LoginAsync(string name, string password);
    }
}