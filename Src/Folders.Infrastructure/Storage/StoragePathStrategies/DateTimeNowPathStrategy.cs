using Folders.Application.Abstractions;

namespace Folders.Infrastructure.Storage.StoragePathStrategies;

public class DateTimeNowPathStrategy : IStoragePathStrategy
{
    public string GenerateRelativePath()
    {
        return DateTime.UtcNow.ToString("O");
    }
}
