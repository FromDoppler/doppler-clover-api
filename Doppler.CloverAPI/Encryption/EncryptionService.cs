using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Doppler.CloverAPI.Encryption
{
    public class EncryptionService : IEncryptionService, IDisposable
    {
        private readonly IOptions<EncryptionSettings> _options;
        private bool _initialized;
        private RijndaelManaged _symmetricKey;
        private Lazy<ICryptoTransform> _encryptor;
        private Lazy<ICryptoTransform> _decryptor;

        public EncryptionService(IOptions<EncryptionSettings> options)
        {
            _options = options;
        }

        public string DecryptAES256(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            EnsureInitialization();

            var cipherTextBytes = Convert.FromBase64String(input);

            using var memoryStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, _decryptor.Value, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        public string EncryptAES256(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            EnsureInitialization();

            var plainTextBytes = Encoding.UTF8.GetBytes(input);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, _encryptor.Value, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private void EnsureInitialization()
        {
            if (_initialized)
            {
                return;
            }

            var initVectorBytes = Encoding.ASCII.GetBytes(_options.Value.InitVectorAsAsciiString);
            var password = new PasswordDeriveBytes(
                _options.Value.Password,
                Encoding.ASCII.GetBytes(_options.Value.SaltValueAsAsciiString),
                "SHA1",
                5);

            var keyBytes = password.GetBytes(32); // KeySize = 256 bits = 32 bytes

            _symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC };
            _encryptor = new Lazy<ICryptoTransform>(() => _symmetricKey.CreateEncryptor(keyBytes, initVectorBytes));
            _decryptor = new Lazy<ICryptoTransform>(() => _symmetricKey.CreateDecryptor(keyBytes, initVectorBytes));
            _initialized = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_initialized)
                {
                    return;
                }

                if (_decryptor.IsValueCreated)
                {
                    _decryptor.Value.Dispose();
                }

                if (_encryptor.IsValueCreated)
                {
                    _encryptor.Value.Dispose();
                }
            }
        }
    }
}
