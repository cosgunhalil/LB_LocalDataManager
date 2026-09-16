using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Encrypts saved data with AES-CBC and authenticates it with HMAC-SHA256, using keys
    /// derived from a password you supply.
    /// <code>
    /// LocalData.Processor = new AesDataProcessor("a passphrase from your game");
    /// </code>
    /// <para>
    /// <b>This is protection against casual save editing, not security.</b> The password
    /// ships inside your build, and anyone willing to open it in a disassembler can read it.
    /// It stops a player editing coins in a text editor; it does not stop a determined one.
    /// Never put anything here you would not hand the player outright.
    /// </para>
    /// <para>
    /// Layout of the written text: base64 of salt (16 bytes), IV (16 bytes), HMAC (32 bytes)
    /// and the ciphertext. Salt and IV are random per save, so saving the same object twice
    /// produces two different files.
    /// </para>
    /// </summary>
    public sealed class AesDataProcessor : IDataProcessor
    {
        private const int SaltSize = 16;
        private const int IvSize = 16;
        private const int MacSize = 32;
        private const int KeySize = 32;
        private const int DefaultIterations = 10000;

        private readonly string _password;
        private readonly int _iterations;

        /// <param name="password">
        /// Passphrase the keys are derived from. Files written with one password cannot be
        /// read with another.
        /// </param>
        public AesDataProcessor(string password) : this(password, DefaultIterations)
        {
        }

        /// <param name="password">Passphrase the keys are derived from.</param>
        /// <param name="iterations">
        /// PBKDF2 iterations. Higher is slower to attack and slower to save; the default
        /// (10000) is chosen to be tolerable on every save on a phone.
        /// </param>
        public AesDataProcessor(string password, int iterations)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password must not be empty.", "password");
            }

            if (iterations < 1000)
            {
                throw new ArgumentOutOfRangeException("iterations", "Use at least 1000 iterations.");
            }

            _password = password;
            _iterations = iterations;
        }

        public string Encode(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var salt = RandomBytes(SaltSize);
            var iv = RandomBytes(IvSize);

            byte[] encryptionKey;
            byte[] macKey;
            DeriveKeys(salt, out encryptionKey, out macKey);

            var cipherText = Transform(Encoding.UTF8.GetBytes(data), encryptionKey, iv, true);
            var mac = ComputeMac(macKey, salt, iv, cipherText);

            var output = new byte[SaltSize + IvSize + MacSize + cipherText.Length];
            Buffer.BlockCopy(salt, 0, output, 0, SaltSize);
            Buffer.BlockCopy(iv, 0, output, SaltSize, IvSize);
            Buffer.BlockCopy(mac, 0, output, SaltSize + IvSize, MacSize);
            Buffer.BlockCopy(cipherText, 0, output, SaltSize + IvSize + MacSize, cipherText.Length);

            return Convert.ToBase64String(output);
        }

        public string Decode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("There is nothing to decrypt.", "data");
            }

            var input = Convert.FromBase64String(data);

            if (input.Length < SaltSize + IvSize + MacSize)
            {
                throw new CryptographicException("Encrypted data is too short to be valid.");
            }

            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            var mac = new byte[MacSize];
            var cipherText = new byte[input.Length - SaltSize - IvSize - MacSize];

            Buffer.BlockCopy(input, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(input, SaltSize, iv, 0, IvSize);
            Buffer.BlockCopy(input, SaltSize + IvSize, mac, 0, MacSize);
            Buffer.BlockCopy(input, SaltSize + IvSize + MacSize, cipherText, 0, cipherText.Length);

            byte[] encryptionKey;
            byte[] macKey;
            DeriveKeys(salt, out encryptionKey, out macKey);

            // Verified before decrypting, so a tampered or wrong-key file fails here rather
            // than decrypting into something that only looks like data.
            if (!AreEqual(mac, ComputeMac(macKey, salt, iv, cipherText)))
            {
                throw new CryptographicException(
                    "Encrypted data failed its integrity check: wrong password, or the file was modified.");
            }

            return Encoding.UTF8.GetString(Transform(cipherText, encryptionKey, iv, false));
        }

        private void DeriveKeys(byte[] salt, out byte[] encryptionKey, out byte[] macKey)
        {
            using (var derive = new Rfc2898DeriveBytes(_password, salt, _iterations))
            {
                encryptionKey = derive.GetBytes(KeySize);
                macKey = derive.GetBytes(KeySize);
            }
        }

        private static byte[] Transform(byte[] input, byte[] key, byte[] iv, bool encrypt)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize * 8;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using (var transform = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor())
                using (var buffer = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                    {
                        crypto.Write(input, 0, input.Length);
                    }

                    return buffer.ToArray();
                }
            }
        }

        private static byte[] ComputeMac(byte[] macKey, byte[] salt, byte[] iv, byte[] cipherText)
        {
            using (var hmac = new HMACSHA256(macKey))
            using (var buffer = new MemoryStream())
            {
                buffer.Write(salt, 0, salt.Length);
                buffer.Write(iv, 0, iv.Length);
                buffer.Write(cipherText, 0, cipherText.Length);
                return hmac.ComputeHash(buffer.ToArray());
            }
        }

        private static byte[] RandomBytes(int length)
        {
            var bytes = new byte[length];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return bytes;
        }

        /// <summary>Compares in constant time, so a failure does not leak where it differed.</summary>
        private static bool AreEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;

            for (var i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }
    }
}
