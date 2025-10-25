using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EdoSign.IntegrationTests
{
    public class DocumentsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public DocumentsApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Upload_Sign_Verify_Flow()
        {
            var client = _factory.CreateClient();

            // create multipart content
            var content = new MultipartFormDataContent();
            var bytes = System.Text.Encoding.UTF8.GetBytes("Integration test document");
            var byteArrayContent = new ByteArrayContent(bytes);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(byteArrayContent, "file", "test.txt");

            // Upload
            var uploadResp = await client.PostAsync("/api/documents/upload", content);
            uploadResp.EnsureSuccessStatusCode();
            var meta = System.Text.Json.JsonDocument.Parse(await uploadResp.Content.ReadAsStringAsync());
            var id = meta.RootElement.GetProperty("id").GetString();

            Assert.False(string.IsNullOrEmpty(id));

            // Sign
            var signResp = await client.PostAsync($"/api/documents/{id}/sign", null);
            signResp.EnsureSuccessStatusCode();

            // Verify
            var verifyResp = await client.GetAsync($"/api/documents/{id}/verify");
            verifyResp.EnsureSuccessStatusCode();
            var verifyJson = System.Text.Json.JsonDocument.Parse(await verifyResp.Content.ReadAsStringAsync());
            var valid = verifyJson.RootElement.GetProperty("valid").GetBoolean();
            Assert.True(valid);
        }
    }
}
