using Folders.Application.UseCases.AddFileToFolder;
using Folders.Application.UseCases.GetFileFromFolder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Folders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FilesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("{parentFolderId:guid}")]        
        public async Task<IActionResult> AddFileToFolder(Guid parentFolderId, [FromForm] IFormFile formFile, [FromForm] string storageProviderKey)
        {
            if (formFile == null || formFile.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(storageProviderKey))
                return BadRequest("Storage provider key is required.");

            await using var stream = formFile.OpenReadStream();


            var command = new AddFileToFolderCommand(
                parentFolderId,
                formFile.FileName,
                stream,
                storageProviderKey
            );

            var handler = ActivatorUtilities.CreateInstance<AddFileToFolderHandler>(HttpContext.RequestServices);
            var file = await handler.Handle(command, default);

            return CreatedAtRoute("GetFileFromFolder", new { ParentFolderId = parentFolderId, fileId = file.Id }, file);
        }
        
        [HttpGet("{parentFolderId:guid}/{fileId:guid}", Name = "GetFileFromFolder")]
        public async Task<ActionResult> GetFileFromFolder(Guid parentFolderId, Guid fileId, [FromQuery]bool downLoading = true)
        {
            var result = await _mediator.Send(new GetFileFromFolderCommand(parentFolderId, fileId, downLoading));

            if (downLoading)
            {
                if (result?.Data is null || result.Data.Length == 0)
                    return NotFound("File content is empty or missing.");
                
                var contentType = result.File.MimeType ?? "application/octet-stream";
                return File(result.Data, contentType, result.File.Name);
            }
            else
            {
                return Ok(result.File);
            }
        }        
    }
}
