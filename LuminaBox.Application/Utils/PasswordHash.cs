using System.Security.Cryptography;

namespace LuminaBox.Application.Utils
{
    public static class PasswordHash
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        private const char Delimiter = ':';
        private const int MinIterations = 10_000;
        private const int MaxIterations = 1_000_000;
        private const int MinSaltSize = 16;
        private const int MinHashSize = 16;
        private const int MaxHashSize = 64;

        public static string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password);

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

            return string.Join(Delimiter, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string? hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            string[] parts = hashedPassword.Split(Delimiter);
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out int iterations) || iterations < MinIterations || iterations > MaxIterations)
                return false;

            byte[] salt;
            byte[] storedHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                storedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length < MinSaltSize || storedHash.Length < MinHashSize || storedHash.Length > MaxHashSize)
                return false;

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, storedHash.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
}
