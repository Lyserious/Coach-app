namespace Coach_app.Services.Auth
{
    public interface IPasswordHasher
    {
        (string Hash, string Salt) Hash(string password);
        bool Verify(string password, string storedHash, string storedSalt);
    }
}