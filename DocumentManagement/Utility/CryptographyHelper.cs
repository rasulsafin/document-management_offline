using System;
using System.Security.Cryptography;

namespace DocumentManagement.Utility
{
    internal static class CryptographyHelper
    {
        public static bool VerifyPasswordHash(string password, ReadOnlySpan<byte> passwordHash, ReadOnlySpan<byte> passwordSalt)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            using (var hmac = new HMACSHA512(passwordSalt.ToArray()))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return passwordHash.SequenceEqual(computedHash);
            }
        }

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password must not be empty", nameof(password));

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
