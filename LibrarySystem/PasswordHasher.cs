using System;
using System.Security.Cryptography;

namespace LibrarySystem
{
    internal static class PasswordHasher
    {
        public static string Hash(string password, string salt)
        {
            using (var derive = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 120000, HashAlgorithmName.SHA256))
                return Convert.ToBase64String(derive.GetBytes(32));
        }

        public static string NewSalt()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
