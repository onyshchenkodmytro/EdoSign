using System.Text;
using EdoSign.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EdoSign.UnitTests
{
    public class SignerTests
    {
        private ISigner CreateSigner()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            return new RsaSigner(config);
        }

        [Fact]
        public void SignAndVerify_ShouldReturnTrue()
        {
            var signer = CreateSigner();
            var data = Encoding.UTF8.GetBytes("Hello, test document");
            var signature = signer.Sign(data);
            var ok = signer.Verify(data, signature);
            Assert.True(ok);
        }

        [Fact]
        public void Verify_WrongData_ShouldReturnFalse()
        {
            var signer = CreateSigner();
            var data = Encoding.UTF8.GetBytes("Hello, test document");
            var signature = signer.Sign(data);
            var tampered = Encoding.UTF8.GetBytes("Hello, tampered");
            var ok = signer.Verify(tampered, signature);
            Assert.False(ok);
        }
    }
}
