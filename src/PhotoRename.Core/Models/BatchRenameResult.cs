using System.Collections.Generic;

namespace PhotoRename.Core.Models;

public sealed class BatchRenameResult
{
    public bool Success { get; init; }

    public int TotalCount { get; init; }

    public int SuccessCount { get; init; }

    public int FailureCount { get; init; }

    public IReadOnlyList<RenameResult> Results { get; init; } = new List<RenameResult>();
}
