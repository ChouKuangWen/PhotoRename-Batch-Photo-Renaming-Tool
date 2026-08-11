using System;
using System.IO;
using System.Linq;
using PhotoRename.Core.Models;
using PhotoRename.Core.Services;
using Xunit;

namespace PhotoRename.Core.Tests;

public class SafeRenameServiceTests
{
    [Fact]
    public void Rename_UsesInjectedFilenameGenerator_ForTargetName()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var filePair = tempDirectory.CreateImageFile("IMG001.jpg");
        var request = new RenameRequest("CUSTOM", IncrementMode.Numeric, startIndex: 7, numericFormat: "D2");
        var service = new SafeRenameService(new FilenameGenerator());

        var result = service.Rename(filePair, request);

        Assert.True(result.Success);
        Assert.Equal("CUSTOM07.jpg", result.NewImageFile!.Name);
        Assert.True(File.Exists(Path.Combine(tempDirectory.Path, "CUSTOM07.jpg")));
    }

    [Fact]
    public void Rename_WhenJsonTargetCollides_DoesNotMoveOriginalFiles()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var imageFile = tempDirectory.CreateImageFile("IMG001.jpg");
        var jsonFile = tempDirectory.CreateJsonFile("IMG001.json");
        tempDirectory.CreateJsonFile("NEW.json");

        var filePair = new FilePair(imageFile.ImageFile, jsonFile);
        var request = new RenameRequest("NEW", IncrementMode.None);
        var moveCalls = 0;
        var service = new SafeRenameService(new FilenameGenerator(), (source, destination) =>
        {
            moveCalls++;
            File.Move(source, destination);
        });

        var result = service.Rename(filePair, request);

        Assert.False(result.Success);
        Assert.Equal(0, moveCalls);
        AssertFileExists(tempDirectory, "IMG001.jpg");
        AssertFileExists(tempDirectory, "IMG001.json");
        AssertFileExists(tempDirectory, "NEW.json");
        AssertFileNotExists(tempDirectory, "NEW.jpg");
    }

    [Fact]
    public void Rename_WhenJsonTempMoveFails_RestoresImageToOriginalPath()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var imageFile = tempDirectory.CreateImageFile("IMG001.jpg");
        var jsonFile = tempDirectory.CreateJsonFile("IMG001.json");
        var filePair = new FilePair(imageFile.ImageFile, jsonFile);
        var request = new RenameRequest("NEW", IncrementMode.None);
        var moveCalls = 0;
        var service = new SafeRenameService(new FilenameGenerator(), (source, destination) =>
        {
            moveCalls++;
            if (moveCalls == 2 && string.Equals(source, jsonFile.FullName, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("simulated json temp move failure");
            }

            File.Move(source, destination);
        });

        var result = service.Rename(filePair, request);

        Assert.False(result.Success);
        AssertFileExists(tempDirectory, "IMG001.jpg");
        AssertFileExists(tempDirectory, "IMG001.json");
        AssertFileNotExists(tempDirectory, "NEW.jpg");
        AssertFileNotExists(tempDirectory, "NEW.json");
        Assert.Equal(new[] { "IMG001.jpg", "IMG001.json" }, Directory.GetFiles(tempDirectory.Path).Select(Path.GetFileName).OrderBy(x => x).ToArray());
    }

    [Fact]
    public void Rename_WhenJsonFinalMoveFails_RestoresImageAndJsonToOriginalState()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var imageFile = tempDirectory.CreateImageFile("IMG001.jpg");
        var jsonFile = tempDirectory.CreateJsonFile("IMG001.json");
        var filePair = new FilePair(imageFile.ImageFile, jsonFile);
        var request = new RenameRequest("NEW", IncrementMode.None);
        var moveCalls = 0;
        var service = new SafeRenameService(new FilenameGenerator(), (source, destination) =>
        {
            moveCalls++;
            if (moveCalls == 4)
            {
                throw new IOException("simulated json final move failure");
            }

            File.Move(source, destination);
        });

        var result = service.Rename(filePair, request);

        Assert.False(result.Success);
        AssertFileExists(tempDirectory, "IMG001.jpg");
        AssertFileExists(tempDirectory, "IMG001.json");
        AssertFileNotExists(tempDirectory, "NEW.jpg");
        AssertFileNotExists(tempDirectory, "NEW.json");
        Assert.Equal(new[] { "IMG001.jpg", "IMG001.json" }, Directory.GetFiles(tempDirectory.Path).Select(Path.GetFileName).OrderBy(x => x).ToArray());
    }

    [Fact]
    public void Rename_WithImageOnlyPair_Succeeds()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var filePair = tempDirectory.CreateImageFile("IMG001.jpg");
        var request = new RenameRequest("NEW", IncrementMode.None);
        var service = new SafeRenameService(new FilenameGenerator());

        var result = service.Rename(filePair, request);

        Assert.True(result.Success);
        AssertFileExists(tempDirectory, "NEW.jpg");
        AssertFileNotExists(tempDirectory, "IMG001.jpg");
    }

    [Fact]
    public void Rename_WithImageAndJsonPair_Succeeds()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var imageFile = tempDirectory.CreateImageFile("IMG001.jpg");
        var jsonFile = tempDirectory.CreateJsonFile("IMG001.json");
        var filePair = new FilePair(imageFile.ImageFile, jsonFile);
        var request = new RenameRequest("NEW", IncrementMode.None);
        var service = new SafeRenameService(new FilenameGenerator());

        var result = service.Rename(filePair, request);

        Assert.True(result.Success);
        AssertFileExists(tempDirectory, "NEW.jpg");
        AssertFileExists(tempDirectory, "NEW.json");
        AssertFileNotExists(tempDirectory, "IMG001.jpg");
        AssertFileNotExists(tempDirectory, "IMG001.json");
    }

    [Fact]
    public void Rename_WhenImageTargetCollides_FailsAndLeavesOriginalFile()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var filePair = tempDirectory.CreateImageFile("IMG001.jpg");
        tempDirectory.CreateImageFile("NEW.jpg");
        var request = new RenameRequest("NEW", IncrementMode.None);
        var service = new SafeRenameService(new FilenameGenerator());

        var result = service.Rename(filePair, request);

        Assert.False(result.Success);
        AssertFileExists(tempDirectory, "IMG001.jpg");
        AssertFileExists(tempDirectory, "NEW.jpg");
    }

    private static void AssertFileExists(TemporaryTestDirectory tempDirectory, string fileName)
    {
        var path = Path.Combine(tempDirectory.Path, fileName);
        Assert.True(File.Exists(path), $"Expected file to exist: {path}");
    }

    private static void AssertFileNotExists(TemporaryTestDirectory tempDirectory, string fileName)
    {
        var path = Path.Combine(tempDirectory.Path, fileName);
        Assert.False(File.Exists(path), $"Expected file to not exist: {path}");
    }

    private sealed class TemporaryTestDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryTestDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public FilePair CreateImageFile(string fileName)
        {
            var path = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(path, "image");
            return new FilePair(new FileInfo(path));
        }

        public FileInfo CreateJsonFile(string fileName)
        {
            var path = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(path, "{}");
            return new FileInfo(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
