using System;
using System.IO;

namespace PhotoRename.Core.Models;

public sealed class FilePair
{
    public FileInfo ImageFile { get; }

    public FileInfo? JsonFile { get; }

    public FilePair(FileInfo imageFile, FileInfo? jsonFile = null)
    {
        ImageFile = imageFile ?? throw new ArgumentNullException(nameof(imageFile));
        JsonFile = jsonFile;
    }
}
