using Coach_app.Models.Domains.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface ICoachRepository
    {
        Task InitializeAsync(); // Création de la table si elle n'existe pas
        Task<List<Coach>> GetAllCoachesAsync();
        Task<Coach> GetCoachByNameAsync(string name);
        Task<bool> AddCoachAsync(Coach coach);
        Task<bool> DeleteCoachAsync(int id);
    }
}