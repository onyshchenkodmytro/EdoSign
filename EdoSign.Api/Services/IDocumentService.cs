using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using EdoSign.Api.Models;

namespace EdoSign.Api.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly string _basePath;
        private readonly string _metaPath;

        public DocumentService(IConfiguration config)
        {
            _basePath = config.GetValue<string>("Storage:BasePath") ?? "Data/storage";
            _metaPath = Path.Combine(_basePath, "meta");
            Directory.CreateDirectory(_basePath);
            Directory.CreateDirectory(_metaPath);
        }

        public async Task<DocumentMetadata> SaveDocumentAsync(IFormFile file)
        {
            var id = Guid.NewGuid().ToString("N");
            var fileExt = Path.GetExtension(file.FileName);
            var storageFileName = $"{id}{fileExt}";
            var storagePath = Path.Combine(_basePath, storageFileName);

            await using (var fs = new FileStream(storagePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var meta = new DocumentMetadata
            {
                Id = id,
                FileName = file.FileName,
                Size = file.Length,
                CreatedAt = DateTimeOffset.UtcNow,
                IsSigned = false,
                SignatureFile = null
            };

            var metaFile = Path.Combine(_metaPath, $"{id}.json");
            await File.WriteAllTextAsync(metaFile, JsonSerializer.Serialize(meta));

            return meta;
        }

        public async Task<byte[]?> GetDocumentBytesAsync(string id)
        {
            var file = FindStorageFileById(id);
            if (file == null) return null;
            return await File.ReadAllBytesAsync(file);
        }

        public Task<DocumentMetadata?> GetMetadataAsync(string id)
        {
            var metaFile = Path.Combine(_metaPath, $"{id}.json");
            if (!File.Exists(metaFile)) return Task.FromResult<DocumentMetadata?>(null);
            var json = File.ReadAllText(metaFile);
            var meta = JsonSerializer.Deserialize<DocumentMetadata>(json);
            return Task.FromResult(meta);
        }

        public async Task<bool> SaveSignatureAsync(string id, byte[] signature)
        {
            var baseFile = FindStorageFileById(id);
            if (baseFile == null) return false;

            var sigFile = Path.Combine(_basePath, $"{id}.sig");
            await File.WriteAllBytesAsync(sigFile, signature);

            var metaFile = Path.Combine(_metaPath, $"{id}.json");
            if (!File.Exists(metaFile)) return false;
            var json = File.ReadAllText(metaFile);
            var meta = JsonSerializer.Deserialize<DocumentMetadata>(json)!;
            meta.IsSigned = true;
            meta.SignatureFile = Path.GetFileName(sigFile);
            await File.WriteAllTextAsync(metaFile, JsonSerializer.Serialize(meta));
            return true;
        }

        public Task<byte[]?> GetSignatureAsync(string id)
        {
            var sigFile = Path.Combine(_basePath, $"{id}.sig");
            if (!File.Exists(sigFile)) return Task.FromResult<byte[]?>(null);
            var bytes = File.ReadAllBytes(sigFile);
            return Task.FromResult<byte[]?>(bytes);
        }

        private string? FindStorageFileById(string id)
        {
            var di = new DirectoryInfo(_basePath);
            foreach (var f in di.GetFiles($"{id}.*"))
            {
                if (f.Extension == ".sig" || f.Extension == ".json") continue;
                return f.FullName;
            }
            return null;
        }
    }
}
