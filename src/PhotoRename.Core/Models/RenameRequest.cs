using System;

namespace PhotoRename.Core.Models;

public sealed class RenameRequest
{
    public string BaseName { get; }

    public IncrementMode IncrementMode { get; }

    public int StartIndex { get; }

    public string NumericFormat { get; }

    public int Index { get; set; }

    public RenameRequest(
        string baseName,
        IncrementMode incrementMode = IncrementMode.None,
        int startIndex = 1,
        string numericFormat = "D3")
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            throw new ArgumentException("Base name must not be empty.", nameof(baseName));
        }

        if (startIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(numericFormat))
        {
            throw new ArgumentException("Numeric format must not be empty.", nameof(numericFormat));
        }

        BaseName = baseName;
        IncrementMode = incrementMode;
        StartIndex = startIndex;
        NumericFormat = numericFormat;
        Index = startIndex;
    }
}
