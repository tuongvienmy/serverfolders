using Folders.Core.Values;

namespace Folders.Infrastructure.Storage.StorageProviders.S3;
internal static class Extensions
{
    internal static string GetBucketNameFrom(this StorageId storageId)
    {
        if (storageId.Provider != "s3")
            throw new InvalidOperationException("StorageId is not an S3 ID.");

        // strip "s3://" from the key
        var bucket = storageId.Value.Replace("s3://", string.Empty);

        // strip after the first "/"
        if (bucket.Contains('/'))
            bucket = bucket.Substring(0, bucket.IndexOf('/'));

        return bucket;
    }
}
