using SequentialGuid;
using System.Text.RegularExpressions;

namespace Tests.Folders.Core.SequentialGuid;

[TestClass]
public class SequentialGuidGeneratorTests
{
    [TestMethod]
    public void ShouldGenerateUniqueGuids()
    {
        // Arrange
        var generator = SequentialGuidGenerator.Instance;
        var guids = new HashSet<Guid>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var guid = generator.NewGuid();
            guids.Add(guid);
        }

        // Assert
        Assert.AreEqual(1000, guids.Count);
    }

    [TestMethod]
    public void ShouldGenerateSequentialGuids()
    {
        // Arrange
        var generator = SequentialGuidGenerator.Instance;
        var guids = new List<Guid>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            guids.Add(generator.NewGuid());
        }

        // Assert
        for (int i = 1; i < guids.Count; i++)
        {
            Assert.IsTrue(guids[i].CompareTo(guids[i - 1]) > 0, $"GUID at index {i} is not greater than GUID at index {i - 1}");
        }
    }

    [TestMethod]
    public void ShouldGenerateGuidsQuickly()
    {
        // Arrange
        var generator = SequentialGuidGenerator.Instance;
        int numberOfGuids = 100000;
        var startTime = DateTime.UtcNow;

        // Act
        for (int i = 0; i < numberOfGuids; i++)
        {
            generator.NewGuid();
        }

        var endTime = DateTime.UtcNow;
        var elapsed = endTime - startTime;

        // Assert
        Assert.IsTrue(elapsed.TotalSeconds < 5, "GUID generation took too long");
    }

    [TestMethod]
    public void ShouldGenerateGuidsWithCorrectFormat()
    {
        // Arrange
        var generator = SequentialGuidGenerator.Instance;

        // Act
        var guid = generator.NewGuid();
        var guidString = guid.ToString();

        // Assert
        Assert.IsTrue(Regex.IsMatch(guidString,@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"));
    }
}
