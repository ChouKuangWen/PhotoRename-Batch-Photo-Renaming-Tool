using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhotoRename.Core.Models;

namespace PhotoRename.App.Services;

public sealed class PhotoDiscoveryService
{
    private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png"];

    public List<FilePair> DiscoverPhotos(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new ArgumentException("Folder path must not be empty.", nameof(folderPath));
        }

        var directory = new DirectoryInfo(folderPath);
        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
        }

        var result = new List<FilePair>();
        var imageFiles = directory
            .GetFiles()
            .Where(f => SupportedExtensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase))
            .OrderBy(f => f.Name)
            .ToList();

        var jsonFilesByName = directory
            .GetFiles("*.json")
            .ToDictionary(f => Path.GetFileNameWithoutExtension(f.Name), StringComparer.OrdinalIgnoreCase);

        foreach (var imageFile in imageFiles)
        {
            var imageNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFile.Name);
            var jsonFile = jsonFilesByName.TryGetValue(imageNameWithoutExtension, out var json) ? json : null;
            result.Add(new FilePair(imageFile, jsonFile));
        }

        return result;
    }
}
