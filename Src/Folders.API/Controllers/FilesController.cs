using Folders.API.Mappers;
using Folders.API.Models;
using Folders.Application.UseCases.AddFileToFolder;
using Folders.Application.UseCases.GetFileFromFolder;
using Folders.Application.UseCases.GetFolderById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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

            byte[] data;
            using (var ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                data = ms.ToArray();
            }

            var folder = await _mediator.Send(new GetFolderByIdQuery(parentFolderId));
            if (folder is null)
                return NotFound("Folder not found.");
            

            var command = new AddFileToFolderCommand(
                folder,
                formFile.FileName,
                data,
                storageProviderKey
            );
            var file = await _mediator.Send(command);
            
            if (file is null)
                return NotFound("File could not be created.");
            
            return CreatedAtRoute(
                routeName: "GetFolderById",
                routeValues: new { id = parentFolderId },
                value: file.ToDto());
        }
        
        [HttpGet("{parentFolderId:guid}/{fileId:guid}", Name = "GetFileFromFolder")]
        public async Task<ActionResult> GetFileFromFolder(Guid parentFolderId, Guid fileId, [FromQuery]bool downLoading = true)
        {
            var folder = await _mediator.Send(new GetFolderByIdQuery(parentFolderId));

            if (folder is null)
                return NotFound("Folder not found");
                        
            var result = await _mediator.Send(new GetFileFromFolderCommand(folder, fileId, downLoading));

            if (downLoading)
            {
                if (result?.Data is null || result.Data.Length == 0)
                    return NotFound("File content is empty or missing.");
                
                var contentType = result.File.MimeType.Value ?? "application/octet-stream";
                return File(result.Data, contentType, result.File.Name);
            }
            else
            {
                return Ok(result.File.ToDto());
            }
        }        
    }
}
