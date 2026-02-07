using Folders.API.Mappers;
using Folders.API.Models;
using Folders.Application.UseCases.GetFolders;
using Folders.Application.UseCases.AddSubFolder;
using Folders.Application.UseCases.CreateRoot;
using Folders.Application.UseCases.GetFolderById;
using Folders.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Folders.Application.UseCases.RenameFolder;

namespace Folders.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;
    public FoldersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("{name}/", Name = "NewRoot")]
    public async Task<IActionResult> NewRoot(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Name cannot be null or empty.");
        }
        var command = new CreateRootCommand(name);
        var folder = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetFolderById), new { id = folder.Id }, folder.ToDto());
    }
    
    [HttpGet("{id:guid}", Name = "GetFolderById")]
    public async Task<ActionResult<FolderDto>> GetFolderById(Guid id, CancellationToken cancellationToken)
    {
        var folder = await _mediator.Send(new GetFolderByIdQuery(id), cancellationToken);

        if (folder is null)
            return NotFound();

        return Ok(folder.ToDto());
    }
    
    [HttpPost("{parentFolderId:guid}/{name}")]
    public async Task<IActionResult> AddFolder(Guid parentFolderId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Subfolder name is required.");

        var command = new AddSubFolderCommand(parentFolderId, name);
        var folder = await _mediator.Send(command);

        if (folder is null)
            return NotFound();

        return CreatedAtAction(nameof(GetFolderById), new { id = folder.Id }, folder.ToDto());
    }

    [HttpGet(Name = "GetFoldersByName")]
    public async Task<ActionResult<List<FolderDto>>> GetFolders([FromQuery]string? name, [FromQuery]bool rootOnly,  CancellationToken cancellationToken)
    {
        var folders = await _mediator.Send(new GetFoldersByNameQuery(name, rootOnly), cancellationToken);
        if (folders is null || folders.Count() ==  0)
            return NotFound();
        return Ok(folders.Select(f => f.ToDto()));
    }

    [HttpPut("{id:guid}", Name = "RenameFolderItem")]
    public async Task<IActionResult> RenameFolderItem(Guid id, [FromBody] RenameFolderItemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
            return BadRequest("New name is required.");
        var command = new RenameFolderItemCommand(id, request.NewName);
        var folderItem = await _mediator.Send(command, cancellationToken);
        if (folderItem is null)
            return NotFound();
        return Ok(folderItem.ToDto());
    }


}
