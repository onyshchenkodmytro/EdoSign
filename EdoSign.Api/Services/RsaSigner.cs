using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace EdoSign.Api.Services
{
    public class RsaSigner : ISigner, IDisposable
    {
        private static RSA? _sharedRsa; // статичний спільний RSA для всіх інстансів
        private static readonly object _lock = new();

        private readonly RSA _rsa;

        public RsaSigner(IConfiguration config)
        {
            lock (_lock)
            {
                if (_sharedRsa == null)
                {
                    var keyType = config.GetValue<string>("Signer:KeyType") ?? "Generate";
                    _sharedRsa = RSA.Create(2048);

                    if (keyType == "PemPath")
                    {
                        var pemPath = config.GetValue<string>("Signer:PemPrivateKeyPath");
                        if (!string.IsNullOrEmpty(pemPath) && File.Exists(pemPath))
                        {
                            var pem = File.ReadAllText(pemPath);
                            _sharedRsa.ImportFromPem(pem.ToCharArray());
                        }
                    }
                }
            }

            // посилання на спільний RSA
            _rsa = _sharedRsa!;
        }

        public byte[] Sign(byte[] data)
        {
            return _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            return _rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public string GetPublicKeyPem()
        {
            var pub = _rsa.ExportSubjectPublicKeyInfo();
            var b64 = Convert.ToBase64String(pub);
            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN PUBLIC KEY-----");
            for (int i = 0; i < b64.Length; i += 64)
            {
                sb.AppendLine(b64.Substring(i, Math.Min(64, b64.Length - i)));
            }
            sb.AppendLine("-----END PUBLIC KEY-----");
            return sb.ToString();
        }

        public void Dispose()
        {
            // нічого не робимо — ключ спільний для всіх
        }
    }
}

