using Folders.Application.Abstractions;

public class DateBasedPathStrategy : IStoragePathStrategy
{
    public string GenerateRelativePath()
    {
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid().ToString("N");
        return $"{now:yyyy/MM/dd}/{guid}";        
    }
}

