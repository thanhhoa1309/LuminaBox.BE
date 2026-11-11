using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Upload;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    public class UploadsController : ControllerBase
    {
        private const long MaxRequestBytes = 11 * 1024 * 1024;

        private readonly IUploadService _uploadService;

        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        public class UploadFileRequest
        {
            public IFormFile? File { get; set; }
            public UploadPurpose Purpose { get; set; }
        }

        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        [SwaggerOperation(
            Summary = "Upload a file",
            Description = "Stores one file in Supabase Storage and returns its path, plus a public url for images. "
                + "Images (jpg, png, webp, up to 5 MB) for ProductImage, BoxImage, EventImage (Organization), BannerImage (Organization, Manager) and FeedbackImage (any user). "
                + "RegistrationDocument also accepts pdf (up to 10 MB) and goes to the private bucket, so only the path is returned.")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
        {
            if (request.File is null)
                throw ErrorHelper.BadRequest("A file is required.", "E20.3");

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            await using var stream = request.File.OpenReadStream();
            var result = await _uploadService.UploadAsync(request.Purpose, new UploadFile
            {
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                Length = request.File.Length,
                Content = stream
            }, roles, cancellationToken);

            return Ok(ApiResult<UploadResponse>.Success(result));
        }
    }
}
