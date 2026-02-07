using Amazon;
using Amazon.S3;
using Folders.Application.Abstractions;
using Folders.Infrastructure.Persistence;
using Folders.Infrastructure.Persistence.DatabaseContexts;
using Folders.Infrastructure.Storage;
using Folders.Infrastructure.Storage.StoragePathStrategies;
using Folders.Infrastructure.Storage.StorageProviders;
using Folders.Infrastructure.Storage.StorageProviders.FileSystem;
using Folders.Infrastructure.Storage.StorageProviders.InMemory;
using Folders.Infrastructure.Storage.StorageProviders.S3;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Folders.Infrastructure.Extensions;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core DbContext
        services.AddDbContext<FoldersDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("FoldersDatabase")));

        // MediatR for domain events
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FoldersDbContext).Assembly));

        // Repository registrations
        services.AddScoped<IFolderRepository, FolderRepository>();

        // Storage manager + providers
        services.AddScoped<IStorageManager, StorageManager>();
        services.AddSingleton<IStorageProviderRegistry, StorageProviderRegistry>();
        services.AddSingleton<IStoragePathStrategy, DateBasedPathStrategy>();
        //services.AddSingleton<IStoragePathStrategy, DateTimeNowPathStrategy>();

        // Options pattern for storage providers
        services.Configure<FileSystemStorageOptions>(configuration.GetSection("Storage:Provider:FileSystem"));
        services.Configure<S3StorageOptions>(configuration.GetSection("Storage:Provider:S3"));
        services.Configure<InMemoryStorageOptions>(configuration.GetSection("Storage:Provider:InMemory"));

        // Register concrete storage providers (FileSystem, S3, etc.)
        services.AddSingleton<IStorageProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FileSystemStorageOptions>>().Value;
            return new FileSystemStorageProvider(options.BasePath, sp.GetRequiredService<IStoragePathStrategy>());
        });

        // Uncomment and configure additional providers as needed
        services.AddSingleton<IStorageProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<InMemoryStorageOptions>>().Value;            
            return new InMemoryStorageProvider("test-scope", sp.GetRequiredService<IStoragePathStrategy>());
        });

        services.AddSingleton<IStorageProvider>(sp =>
        {
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.APSoutheast2);
            var bucketName = Environment.GetEnvironmentVariable("BucketName") ?? @"fileapi1";
            return new S3StorageProvider(s3Client, bucketName, sp.GetRequiredService<IStoragePathStrategy>());
        });

        return services;
    }
}
