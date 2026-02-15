using Coach_app.Data.Repositories;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models.Domains.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ICoachRepository _coachRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(ICoachRepository coachRepository, IPasswordHasher passwordHasher)
        {
            _coachRepository = coachRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> HasAnyCoachAsync()
        {
            var coaches = await _coachRepository.GetAllCoachesAsync();
            return coaches.Count > 0;
        }

        public async Task<List<Coach>> GetAvailableCoachesAsync()
        {
            return await _coachRepository.GetAllCoachesAsync();
        }

        public async Task<bool> CreateCoachAsync(string name, string password)
        {
            // Vérifier si le coach existe déjà
            var existing = await _coachRepository.GetCoachByNameAsync(name);
            if (existing != null)
                return false;

            // Sécuriser le mot de passe
            var (hash, salt) = _passwordHasher.Hash(password);

            // Générer le nom de fichier unique pour sa DB perso
            // Guid pour être sûr qu'il n'y ait pas de conflit de noms de fichiers
            string dbFileName = $"coach_{Guid.NewGuid()}.db3";

            var newCoach = new Coach
            {
                Name = name,
                PasswordHash = hash,
                Salt = salt,
                DataFileName = dbFileName
            };

            return await _coachRepository.AddCoachAsync(newCoach);
        }

        public async Task<Coach> LoginAsync(string name, string password)
        {
            var coach = await _coachRepository.GetCoachByNameAsync(name);
            if (coach == null)
                return null;

            bool isValid = _passwordHasher.Verify(password, coach.PasswordHash, coach.Salt);
            return isValid ? coach : null;
        }
    }
}