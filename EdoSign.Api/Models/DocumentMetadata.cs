using System;

namespace EdoSign.Api.Models
{
    public class DocumentMetadata
    {
        public string Id { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public long Size { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsSigned { get; set; }
        public string? SignatureFile { get; set; }
    }
}

