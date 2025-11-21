using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TPV_Gastronomico.Utils
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Genera un hash seguro a partir de la contraseña
        /// </summary>
        public static string Hash(string password)
        {
            // Generar un salt aleatorio
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derivar la clave usando PBKDF2
            var subkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            // Formato final: 0x00 + salt (16 bytes) + subkey (32 bytes)
            var outputBytes = new byte[49];
            outputBytes[0] = 0x00;
            Buffer.BlockCopy(salt, 0, outputBytes, 1, 16);
            Buffer.BlockCopy(subkey, 0, outputBytes, 17, 32);

            return Convert.ToBase64String(outputBytes);
        }

        /// <summary>
        /// Verifica que la contraseña coincida con el hash
        /// </summary>
        public static bool Verify(string hashedPassword, string password)
        {
            var bytes = Convert.FromBase64String(hashedPassword);

            // Extraer salt y subkey almacenados
            var salt = new byte[16];
            Buffer.BlockCopy(bytes, 1, salt, 0, 16);

            var storedSubkey = new byte[32];
            Buffer.BlockCopy(bytes, 17, storedSubkey, 0, 32);

            // Derivar subkey a partir de la contraseña ingresada
            var generatedSubkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            // Comparación segura
            return CryptographicOperations.FixedTimeEquals(storedSubkey, generatedSubkey);
        }
    }
}
