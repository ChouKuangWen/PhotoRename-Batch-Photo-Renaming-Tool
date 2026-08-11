using System;
using System.IO;

namespace PhotoRename.Core.Models;

public sealed class RenameResult
{
    public bool Success { get; init; }

    public FilePair FilePair { get; init; } = default!;

    public FileInfo? NewImageFile { get; init; }

    public FileInfo? NewJsonFile { get; init; }

    public string? ErrorMessage { get; init; }

    public Exception? Exception { get; init; }
}
