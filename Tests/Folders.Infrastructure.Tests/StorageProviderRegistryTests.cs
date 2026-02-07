namespace Folders.Infrastructure.Tests;

using Folders.Application.Abstractions;
using Folders.Application.Exceptions;
using Folders.Core.Values;
using Folders.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

[TestClass]
public class StorageProviderRegistryTests
{
    private static IStorageProvider CreateProvider(string keyValue)
    {
        var key = StorageProviderKey.From(keyValue);
        var provider = Substitute.For<IStorageProvider>();
        provider.StorageProviderKey.Returns(key);
        return provider;
    }

    [TestMethod]
    public void Constructor_ShouldRegisterAllProvidedProviders()
    {
        // Arrange
        var provider1 = CreateProvider("provider1");
        var provider2 = CreateProvider("provider2");

        // Act
        var registry = new StorageProviderRegistry([provider1, provider2]);

        // Assert
        var registeredKeys = registry.RegisteredKeys;
        CollectionAssert.Contains(registeredKeys.ToList(), provider1.StorageProviderKey);
        CollectionAssert.Contains(registeredKeys.ToList(), provider2.StorageProviderKey);
    }

    [TestMethod]
    public void Resolve_ShouldReturnProvider_WhenKeyIsRegistered()
    {
        // Arrange
        var provider = CreateProvider("provider1");
        var registry = new StorageProviderRegistry([provider]);

        // Act
        var resolved = registry.Resolve(provider.StorageProviderKey);

        // Assert
        Assert.AreSame(provider, resolved);
    }

    [TestMethod]    
    public void Resolve_ShouldThrow_WhenKeyIsNotRegistered()
    {
        // Arrange
        var provider = CreateProvider("provider1");
        var registry = new StorageProviderRegistry([provider]);
        var missingKey = StorageProviderKey.From("missing");

        // Act
        Assert.ThrowsExactly<KeyNotFoundException>(() => registry.Resolve(missingKey));        

        // Assert handled by ExpectedException
    }

    [TestMethod]
    public void IsRegistered_ShouldReturnTrue_ForRegisteredKey()
    {
        // Arrange
        var provider = CreateProvider("provider1");
        var registry = new StorageProviderRegistry([provider]);

        // Act
        var result = registry.IsRegistered(provider.StorageProviderKey);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsRegistered_ShouldReturnFalse_ForUnregisteredKey()
    {
        // Arrange
        var provider = CreateProvider("provider1");
        var registry = new StorageProviderRegistry([provider]);
        var missingKey = StorageProviderKey.From("missing");

        // Act
        var result = registry.IsRegistered(missingKey);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RegisteredKeys_ShouldReturnAllKeys()
    {
        // Arrange
        var provider1 = CreateProvider("provider1");
        var provider2 = CreateProvider("provider2");
        var registry = new StorageProviderRegistry([provider1, provider2]);

        // Act
        var keys = registry.RegisteredKeys;

        // Assert
        Assert.AreEqual(2, keys.Count);
        CollectionAssert.Contains(keys.ToList(), provider1.StorageProviderKey);
        CollectionAssert.Contains(keys.ToList(), provider2.StorageProviderKey);
    }
}