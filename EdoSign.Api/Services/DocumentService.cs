using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using EdoSign.Api.Models;

namespace EdoSign.Api.Services
{
    public interface IDocumentService
    {
        Task<DocumentMetadata> SaveDocumentAsync(IFormFile file);
        Task<byte[]?> GetDocumentBytesAsync(string id);
        Task<DocumentMetadata?> GetMetadataAsync(string id);
        Task<bool> SaveSignatureAsync(string id, byte[] signature);
        Task<byte[]?> GetSignatureAsync(string id);
    }
}
