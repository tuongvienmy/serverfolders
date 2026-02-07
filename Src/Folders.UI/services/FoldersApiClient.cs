using Folders.Contracts;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Folders.UI.services;

public class FoldersApiClient
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    // Shared options with the converter
    private static readonly JsonSerializerOptions _jsonOptions;

    static FoldersApiClient()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new FolderItemDtoConverter());
    }
    public FoldersApiClient(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<FolderDto> NewRoot(string name)
    {
        var url = $"api/Folders/{Uri.EscapeDataString(name)}/";
        var response = await _http.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FolderDto>()
               ?? throw new InvalidOperationException("No folder returned");
    }

    public async Task<FolderDto?> GetFolderAsync(Guid id)
    {
        var response = await _http.GetAsync($"api/Folders/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FolderDto>(_jsonOptions);
    }
    
    public async Task<List<FolderDto>> GetFolderByNameAsync(string? name = null, bool rootOnly = true)
    {
        var queryString = !string.IsNullOrEmpty(name) 
                                    ? (rootOnly ? "name={Uri.EscapeDataString(name!)}&rootOnly=true" : "name={ Uri.EscapeDataString(name!)}") 
                                    : (rootOnly ? "rootOnly=true" : "rootOnly=false");

        var response = await _http.GetAsync($"api/Folders?{queryString}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return [];

        var folders = await response.Content.ReadFromJsonAsync< List<FolderDto>>(_jsonOptions);

        return folders ?? [];
    }   

    public async Task<FolderDto> AddFolderAsync(string parentFolderId, string folderName)
    {
        var url = $"api/Folders/{parentFolderId}/{Uri.EscapeDataString(folderName)}/";
        var response = await _http.PostAsync(url, null);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FolderDto>(_jsonOptions)
              ?? throw new InvalidOperationException("No folder returned");
    }

    public async Task<FileDto> UploadFileAsync(string folderId, IBrowserFile file, string storageProviderKey = "file")
    {
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxAllowedSize: 50_000_000); // 50 MB
        content.Add(new StreamContent(stream), "formFile", file.Name);
        content.Add(new StringContent(storageProviderKey), "storageProviderKey");

        var response = await _http.PostAsync($"/api/Files/{folderId}", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FileDto>()
               ?? throw new InvalidOperationException("No file returned");
    }

    public async Task DownloadFileAsync(Guid folderId, Guid fileId)
    {
        var response = await _http.GetAsync($"/api/Files/{folderId}/{fileId}");
        response.EnsureSuccessStatusCode();

        // Read content
        var bytes = await response.Content.ReadAsByteArrayAsync();

        var disposition = response.Content.Headers.ContentDisposition;
        var fileName = disposition?.FileName?.Trim('"') ?? "download.bin";

        // Convert to Base64
        var base64 = Convert.ToBase64String(bytes);

        // Call JS to save file
        await _js.InvokeVoidAsync("saveAsFile", fileName, base64);

        //var url = $"/api/Files/{folderId}/{fileId}?downLoading=true";
        //await _js.InvokeVoidAsync("downloadFileFromResponse", url);
    }

    public async Task<FileDto> AddFileAsync(string folderId, string fileName)
    {
        var response = await _http.PostAsJsonAsync($"/folders/{folderId}/files", new { fileName });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileDto>()
               ?? throw new InvalidOperationException("No file returned");
    }

    public async Task<FolderDto> RenameFolderAsync(Guid folderId, string newName)
    {
        var response = await _http.PutAsJsonAsync($"/api/Folders/{folderId}", new RenameFolderItemRequest(newName));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FolderDto>(_jsonOptions)
               ?? throw new InvalidOperationException("No folder returned");
    }
}

