using Folders.Application.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Folders.Contracts;
public class FolderItemDtoConverter : JsonConverter<FolderItemDto>
{
    public FolderItemDtoConverter()
    {
    }
    public override FolderItemDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.GetProperty("type").GetString();

        return type switch
        {
            "folder" => JsonSerializer.Deserialize<FolderDto>(root.GetRawText(), options),
            "file" => JsonSerializer.Deserialize<FileDto>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, FolderItemDto value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}