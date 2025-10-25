using Microsoft.AspNetCore.Http;

namespace EdoSign.Api.Models
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
