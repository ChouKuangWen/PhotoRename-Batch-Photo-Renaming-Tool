using System;
using System.Collections.Generic;
using System.Linq;
using PhotoRename.Core.Models;

namespace PhotoRename.Core.Services;

public sealed class BatchRenameService
{
    private readonly SafeRenameService _safeRenameService;

    public BatchRenameService(
        SafeRenameService safeRenameService)
    {
        _safeRenameService =
            safeRenameService
            ?? throw new ArgumentNullException(
                nameof(safeRenameService));
    }

    public BatchRenameResult Rename(
        IEnumerable<FilePair> filePairs,
        RenameRequest request)
    {
        if (filePairs is null)
        {
            throw new ArgumentNullException(
                nameof(filePairs));
        }

        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        var results =
            new List<RenameResult>();

        // 第一張照片從 Index = 0 開始。
        request.Index = 0;

        foreach (var filePair in filePairs)
        {
            var result =
                _safeRenameService.Rename(
                    filePair,
                    request);

            results.Add(result);

            // 下一張照片。
            request.Index++;
        }

        var successCount =
            results.Count(result => result.Success);

        var failureCount =
            results.Count - successCount;

        return new BatchRenameResult
        {
            Success = failureCount == 0,

            TotalCount =
                results.Count,

            SuccessCount =
                successCount,

            FailureCount =
                failureCount,

            Results =
                results
        };
    }
}