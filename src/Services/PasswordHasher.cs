
using System;
using System.Security.Cryptography;

namespace CafeNetManager.Services
{
    public static class PasswordHasher
    {
        private const int SoLanBam = 600000;

        public static string BamMatKhau(string matKhau)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                matKhau,
                salt,
                SoLanBam,
                HashAlgorithmName.SHA256,
                32);

            return $"{SoLanBam}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool KiemTraMatKhau(
            string matKhau, string matKhauDaBam)
        {
            string[] parts = matKhauDaBam.Split('$');

            if (parts.Length != 3 ||
                !int.TryParse(parts[0], out int soLan) ||
                soLan <= 0 || soLan > 1000000)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hashCu = Convert.FromBase64String(parts[2]);

                if (salt.Length != 16 || hashCu.Length != 32)
                    return false;

                byte[] hashMoi = Rfc2898DeriveBytes.Pbkdf2(
                    matKhau,
                    salt,
                    soLan,
                    HashAlgorithmName.SHA256,
                    32);

                return CryptographicOperations.FixedTimeEquals(
                    hashCu, hashMoi);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
