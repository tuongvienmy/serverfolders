using Folders.Core.FileSystem;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Folders.Core;

[TestClass]
public class FoldersTests
{
    static readonly string ImportLocation = Environment.GetEnvironmentVariable("ImportLocation") ?? @"E:\Imports";

    static readonly FileSystemStorageProvider storage = new(Environment.GetEnvironmentVariable("StorageLocation") ?? @"E:\Storage");

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {        
        TestUitility.GenerateTextFiles(10, ImportLocation);
        TestUitility.GenerateImageFiles(10, ImportLocation);
        TestUitility.GenerateBinaryFiles(10, ImportLocation);
    }

    [TestMethod]
    public void CreateFolder()
    {
        var folder = Folder.NewFolder("TestServerCode");
        Assert.IsNotNull(folder);
        Assert.IsNull(folder.Parent);        
    }
    [TestMethod]
    public void CreateFolderTree_AddFileToSub_FindFromRoot()
    {
        var folder = Folder.NewFolder("TestServerCode");
        Assert.IsNotNull(folder);
        
        var subOne = folder.AddFolder("SubOne");
        Assert.IsNotNull(subOne);
        Assert.IsTrue(subOne.Name == "SubOne");
        
        Assert.IsTrue(subOne.Parent?.Name == "TestServerCode");

        var subTwo = folder.AddFolder("SubTwo");
        Assert.IsNotNull(subTwo);
        Assert.IsTrue(subTwo.Parent == subOne.Parent);

        var fileName = TestUitility.GenerateRandomSentence(true);

        var file = subOne.AddFile(fileName, "text/plain", new MemoryStream(Encoding.UTF8.GetBytes("Hello World")), storage);
        Assert.IsNotNull(file);
        Assert.IsTrue(file.Name == fileName);
        Assert.IsTrue(file.Parent == subOne);

        var items = folder.FindAll(fileName);
        Assert.IsTrue(items.Count() == 1);
    }
    [TestMethod]
    public void CreateFolder_AddFirstTextFile_UpdateIt()
    {
        var folder = Folder.NewFolder("TestServerCode");
        Assert.IsNotNull(folder);        

        var dirInfo = new DirectoryInfo(ImportLocation);
        foreach (var fileInfo in dirInfo.GetFiles())
        {

            var file = folder.AddFile(fileInfo, storage);
            
            Assert.IsNotNull(file);
            Assert.IsTrue(file.CreatedAt == file.ModifiedAt);

        }

        Debug.WriteLine("===============================");
        foreach (FolderItem i in folder.Items)
            Debug.WriteLine(i.Name);

        Debug.WriteLine("===============================");
        foreach (FolderItem serverFolderItem in folder.Files.OrderByDescending(f => f.MimeType))
            Debug.WriteLine(serverFolderItem.Name);

        Debug.WriteLine("===============================");
        foreach (FolderItem serverFolderItem in folder.Items.OrderBy(i => i.CreatedAt))
            Debug.WriteLine(serverFolderItem.Name);        
    }
}

public class TestUitility
{
    public static string GenerateRandomSentence(bool toBeFilename = false)
    {
        string[] Nouns = { "dog", "cat", "apple", "car", "computer", "house", "mountain", "river", "city", "phone", "cyclone", "ocean", "printer", "building", "man", "girl", "road", "theatre", "happiness" };
        string[] Verbs = { "runs", "jumps", "eats", "drives", "sings", "swims", "plays", "writes", "paints", "sleeps", "lies", "sings", "shouts", "declars", "looks", "dives", "types", "thinks", "breaths" };
        string[] Adjectives = { "quick", "lazy", "beautiful", "shiny", "bright", "small", "large", "old", "new", "fast", "happy", "lucky", "empty", "rich", "full", "skinny", "fat", "lovely", "amazing", "stubbon" };
        string[] Adverbs = { "quickly", "slowly", "carefully", "loudly", "happily", "gracefully", "easily", "sadly", "boldly", "loudly", "quietly", "extremely", "steadily", "rationally", "beautifully", "gradually" };

        Random random = new();
        if (toBeFilename)
        {
            return $"{CapitalizeFirstLetter(Adjectives[random.Next(Adjectives.Length)])}_{Nouns[random.Next(Nouns.Length)]}_{Verbs[random.Next(Verbs.Length)]}_{Adverbs[random.Next(Adverbs.Length)]}";
        }
        else
        {
            return $"{CapitalizeFirstLetter(Adjectives[random.Next(Adjectives.Length)])} {Nouns[random.Next(Nouns.Length)]} {Verbs[random.Next(Verbs.Length)]} {Adverbs[random.Next(Adverbs.Length)]}.";
        }

    }

    private static string CapitalizeFirstLetter(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        return char.ToUpper(word[0]) + word.Substring(1);
    }

    public static void GenerateTextFiles(int number, string location)
    {
        Random random = new();
        StringBuilder sentences = new();

        for (int count = 0; count < number; count++)
        {
            for (int i = 0; i < random.Next(3, 5); i++)
            {
                sentences.Append(GenerateRandomSentence(true));
            }
            var filename = sentences.ToString();
            sentences.Clear();

            for (int i = 0; i < random.Next(5, 20); i++)
            {
                sentences.AppendLine(GenerateRandomSentence(false));
            }

            System.IO.File.WriteAllText(Path.Combine(location, $"{filename}.txt"), sentences.ToString());
            sentences.Clear();
        }
    }
    public static void GenerateImageFiles(int number, string location)
    {
        Random random = new(50);
        for (int count = 0; count < number; ++count)
        {
            var width = random.Next(100, 500);
            var height = random.Next(100, 500);
            using Bitmap bitmap = new(width,height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.AliceBlue);
                g.DrawString(GenerateRandomSentence(), new Font("Arial", 10), Brushes.Black, new PointF(10, 40)); // Add some text
            }
            var format = count % 2 == 0 ? ImageFormat.Bmp : ImageFormat.Png;
            bitmap.Save(Path.Combine(location, $"{GenerateRandomSentence(true)}.{format.ToString()}"), format);
        }
    }

    public static void GenerateBinaryFiles(int number, string location)
    {
        Random random = new();
        byte[] randomData = new byte[random.Next(1, 10) * 1024]; // 1KB of random data
        random.NextBytes(randomData); // Fill the array with random bytes
        System.IO.File.WriteAllBytes(Path.Combine(location, $"{GenerateRandomSentence()}.bin"), randomData);
    }
}
