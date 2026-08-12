using System;

namespace PhotoRename.Core.Models;

public sealed class RenameRequest
{
    public string Prefix { get; }

    public int? MidNumber { get; }

    public string MidText { get; }

    public int? EndNumber { get; }

    public IncrementMode IncrementMode { get; }

    public string NumericFormat { get; }

    public int Index { get; set; }

    public RenameRequest(
        string prefix,
        int? midNumber,
        string midText,
        int? endNumber,
        IncrementMode incrementMode = IncrementMode.None,
        string numericFormat = "D3")
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException(
                "Prefix must not be empty.",
                nameof(prefix));
        }

        if (midNumber is null && endNumber is null)
        {
            throw new ArgumentException(
                "Mid number and end number cannot both be empty.");
        }

        if (midNumber is not null && midNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(midNumber),
                "Mid number must not be negative.");
        }

        if (endNumber is not null && endNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endNumber),
                "End number must not be negative.");
        }

        if (string.IsNullOrWhiteSpace(numericFormat))
        {
            throw new ArgumentException(
                "Numeric format must not be empty.",
                nameof(numericFormat));
        }

        Prefix = prefix;
        MidNumber = midNumber;
        MidText = midText ?? string.Empty;
        EndNumber = endNumber;
        IncrementMode = incrementMode;
        NumericFormat = numericFormat;

        Index = 0;
    }
}