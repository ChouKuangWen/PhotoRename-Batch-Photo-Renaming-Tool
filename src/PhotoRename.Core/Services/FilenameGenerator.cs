using System;
using PhotoRename.Core.Models;

namespace PhotoRename.Core.Services;

public sealed class FilenameGenerator
{
    public string Generate(RenameRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return request.IncrementMode == IncrementMode.Numeric
            ? string.Concat(request.BaseName, request.Index.ToString(request.NumericFormat))
            : request.BaseName;
    }
}
