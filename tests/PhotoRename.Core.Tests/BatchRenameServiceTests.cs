using System;
using System.Collections.Generic;
using System.IO;
using PhotoRename.Core.Models;
using PhotoRename.Core.Services;
using Xunit;

namespace PhotoRename.Core.Tests;

public class BatchRenameServiceTests
{
    [Fact]
    public void Rename_AllSuccess_ReturnsSuccessAndRenamedFiles()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var filePairs = new List<FilePair>
        {
            tempDirectory.CreateImageFile("IMG001.jpg"),
            tempDirectory.CreateImageFile("IMG002.jpg"),
            tempDirectory.CreateImageFile("IMG003.jpg")
        };

        var request = new RenameRequest("NEW_NAME", IncrementMode.Numeric, startIndex: 1);
        var batchRenameService = new BatchRenameService(new SafeRenameService());

        var result = batchRenameService.Rename(filePairs, request);

        Assert.True(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);

        AssertFileExists(tempDirectory, "NEW_NAME001.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME002.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME003.jpg");
        AssertFileNotExists(tempDirectory, "IMG001.jpg");
        AssertFileNotExists(tempDirectory, "IMG002.jpg");
        AssertFileNotExists(tempDirectory, "IMG003.jpg");
    }

    [Fact]
    public void Rename_PartialSuccess_ReturnsPartialSuccessAndContinuesProcessing()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        tempDirectory.CreateImageFile("NEW_NAME002.jpg");

        var filePairs = new List<FilePair>
        {
            tempDirectory.CreateImageFile("IMG001.jpg"),
            tempDirectory.CreateImageFile("IMG002.jpg"),
            tempDirectory.CreateImageFile("IMG003.jpg")
        };

        var request = new RenameRequest("NEW_NAME", IncrementMode.Numeric, startIndex: 1);
        var batchRenameService = new BatchRenameService(new SafeRenameService());

        var result = batchRenameService.Rename(filePairs, request);

        Assert.False(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);

        AssertFileExists(tempDirectory, "NEW_NAME001.jpg");
        AssertFileExists(tempDirectory, "IMG002.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME003.jpg");
        AssertFileNotExists(tempDirectory, "IMG001.jpg");
        AssertFileNotExists(tempDirectory, "IMG003.jpg");
    }

    [Fact]
    public void Rename_AllFail_ReturnsFailureAndNoFilesRenamed()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        tempDirectory.CreateImageFile("NEW_NAME001.jpg");
        tempDirectory.CreateImageFile("NEW_NAME002.jpg");
        tempDirectory.CreateImageFile("NEW_NAME003.jpg");

        var filePairs = new List<FilePair>
        {
            tempDirectory.CreateImageFile("IMG001.jpg"),
            tempDirectory.CreateImageFile("IMG002.jpg"),
            tempDirectory.CreateImageFile("IMG003.jpg")
        };

        var request = new RenameRequest("NEW_NAME", IncrementMode.Numeric, startIndex: 1);
        var batchRenameService = new BatchRenameService(new SafeRenameService());

        var result = batchRenameService.Rename(filePairs, request);

        Assert.False(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(3, result.FailureCount);

        AssertFileExists(tempDirectory, "IMG001.jpg");
        AssertFileExists(tempDirectory, "IMG002.jpg");
        AssertFileExists(tempDirectory, "IMG003.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME001.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME002.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME003.jpg");
    }

    [Fact]
    public void Rename_EmptyCollection_ReturnsEmptyResult()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var request = new RenameRequest("NEW_NAME", IncrementMode.Numeric, startIndex: 1);
        var batchRenameService = new BatchRenameService(new SafeRenameService());

        var result = batchRenameService.Rename(Array.Empty<FilePair>(), request);

        Assert.True(result.Success);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Results);
    }

    [Fact]
    public void Rename_NullFilePairs_ThrowsArgumentNullException()
    {
        var request = new RenameRequest("NEW_NAME", IncrementMode.Numeric, startIndex: 1);
        var batchRenameService = new BatchRenameService(new SafeRenameService());

        Assert.Throws<ArgumentNullException>(() => batchRenameService.Rename(null!, request));
    }

    [Fact]
    public void Rename_NullRequest_ThrowsArgumentNullException()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var filePairs = new List<FilePair>
        {
            tempDirectory.CreateImageFile("IMG001.jpg")
        };

        var batchRenameService = new BatchRenameService(new SafeRenameService());

        Assert.Throws<ArgumentNullException>(() => batchRenameService.Rename(filePairs, null!));
    }

    [Fact]
    public void Rename_ImageAndJsonPair_RenamesBothFilesTogether()
    {
        using var tempDirectory = new TemporaryTestDirectory();

        var filePairs = new List<FilePair>
        {
            tempDirectory.CreateImageWithJsonPair("IMG001.jpg", "IMG001.json")
        };

        var request = new RenameRequest("NEW_NAME", IncrementMode.None);
        var batchRenameService = new BatchRenameService(new SafeRenameService());

        var result = batchRenameService.Rename(filePairs, request);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);

        AssertFileExists(tempDirectory, "NEW_NAME.jpg");
        AssertFileExists(tempDirectory, "NEW_NAME.json");
        AssertFileNotExists(tempDirectory, "IMG001.jpg");
        AssertFileNotExists(tempDirectory, "IMG001.json");
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

        public FilePair CreateImageWithJsonPair(string imageFileName, string jsonFileName)
        {
            var image = CreateImageFile(imageFileName);
            var jsonPath = System.IO.Path.Combine(Path, jsonFileName);
            File.WriteAllText(jsonPath, "{}");
            return new FilePair(image.ImageFile, new FileInfo(jsonPath));
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
