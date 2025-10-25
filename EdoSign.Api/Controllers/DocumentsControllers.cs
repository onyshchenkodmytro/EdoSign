using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EdoSign.Api.Services;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.ComponentModel.Design;
using EdoSign.Api.Models;

namespace EdoSign.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _docService;
        private readonly ISigner _signer;

        public DocumentsController(IDocumentService docService, ISigner signer)
        {
            _docService = docService;
            _signer = signer;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("File missing");

            var meta = await _docService.SaveDocumentAsync(file);
            return CreatedAtAction(nameof(GetMetadata), new { id = meta.Id }, meta);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(string id)
        {
            var bytes = await _docService.GetDocumentBytesAsync(id);
            var meta = await _docService.GetMetadataAsync(id);
            if (bytes == null || meta == null) return NotFound();
            return File(bytes, MediaTypeNames.Application.Octet, meta.FileName);
        }

        [HttpGet("{id}/metadata")]
        public async Task<IActionResult> GetMetadata(string id)
        {
            var meta = await _docService.GetMetadataAsync(id);
            if (meta == null) return NotFound();
            return Ok(meta);
        }

        [HttpPost("{id}/sign")]
        public async Task<IActionResult> Sign(string id)
        {
            var bytes = await _docService.GetDocumentBytesAsync(id);
            if (bytes == null) return NotFound();
            var signature = _signer.Sign(bytes);
            var saved = await _docService.SaveSignatureAsync(id, signature);
            if (!saved) return StatusCode(500);
            return Ok(new { id, signature = System.Convert.ToBase64String(signature) });
        }

        [HttpGet("{id}/verify")]
        public async Task<IActionResult> Verify(string id)
        {
            var bytes = await _docService.GetDocumentBytesAsync(id);
            var sig = await _docService.GetSignatureAsync(id);
            if (bytes == null || sig == null) return NotFound();
            var ok = _signer.Verify(bytes, sig);
            return Ok(new { id, valid = ok, publicKey = _signer.GetPublicKeyPem() });
        }
    }
}

