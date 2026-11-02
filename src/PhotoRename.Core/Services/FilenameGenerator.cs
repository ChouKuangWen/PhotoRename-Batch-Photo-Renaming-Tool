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

        var midPart = request.MidNumber.HasValue
            ? request.MidNumber.Value.ToString(request.NumericFormat)
            : string.Empty;

        var endPart = request.EndNumber.HasValue
            ? request.EndNumber.Value.ToString(request.NumericFormat)
            : string.Empty;

        return request.IncrementMode switch
        {
            IncrementMode.None =>
                $"{request.Prefix}{midPart}{request.MidText}{endPart}",

            IncrementMode.MidNumeric =>
                $"{request.Prefix}" +
                $"{GetMidNumber(request)}" +
                $"{request.MidText}" +
                $"{endPart}",

            IncrementMode.Numeric =>
                $"{request.Prefix}" +
                $"{midPart}" +
                $"{request.MidText}" +
                $"{GetEndNumber(request)}",

            IncrementMode.BothNumeric =>
                $"{request.Prefix}" +
                $"{GetMidNumber(request)}" +
                $"{request.MidText}" +
                $"{GetEndNumber(request)}",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(request.IncrementMode),
                    request.IncrementMode,
                    "Unsupported increment mode.")
        };
    }

    private static string GetMidNumber(RenameRequest request)
    {
        if (!request.MidNumber.HasValue)
        {
            return string.Empty;
        }

        var value =
            request.MidNumber.Value + request.Index;

        return value.ToString(request.NumericFormat);
    }

    private static string GetEndNumber(RenameRequest request)
    {
        if (!request.EndNumber.HasValue)
        {
            return string.Empty;
        }

        var value =
            request.EndNumber.Value + request.Index;

        return value.ToString(request.NumericFormat);
    }
}