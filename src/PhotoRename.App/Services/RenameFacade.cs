using System;
using System.Collections.Generic;
using PhotoRename.Core.Models;
using PhotoRename.Core.Services;

namespace PhotoRename.App.Services;

public sealed class RenameFacade
{
    private readonly PhotoDiscoveryService _discoveryService;
    private readonly FilenameGenerator _filenameGenerator;
    private readonly BatchRenameService _batchRenameService;
    private readonly SafeRenameService _safeRenameService;

    public RenameFacade()
    {
        _discoveryService = new PhotoDiscoveryService();

        _filenameGenerator = new FilenameGenerator();

        _safeRenameService =
            new SafeRenameService(_filenameGenerator);

        _batchRenameService =
            new BatchRenameService(_safeRenameService);
    }

    public List<FilePair> DiscoverPhotos(string folderPath)
    {
        return _discoveryService.DiscoverPhotos(folderPath);
    }

    /// <summary>
    /// 產生單張照片的預覽檔名。
    /// Index 從 0 開始，因此第一張照片使用使用者輸入的起始數字。
    /// </summary>
    public string PreviewNewFilename(
        FilePair filePair,
        string prefix,
        int? midNumber,
        string midText,
        int? endNumber,
        IncrementMode incrementMode,
        int index)
    {
        if (filePair is null)
        {
            throw new ArgumentNullException(nameof(filePair));
        }

        var request = new RenameRequest(
            prefix,
            midNumber,
            midText,
            endNumber,
            incrementMode);

        request.Index = index;

        var newFileName =
            _filenameGenerator.Generate(request);

        var extension =
            filePair.ImageFile.Extension;

        return newFileName + extension;
    }

    /// <summary>
    /// 執行批次重新命名。
    /// </summary>
    public BatchRenameResult ExecuteRename(
        IEnumerable<FilePair> filePairs,
        string prefix,
        int? midNumber,
        string midText,
        int? endNumber,
        IncrementMode incrementMode)
    {
        if (filePairs is null)
        {
            throw new ArgumentNullException(nameof(filePairs));
        }

        var request = new RenameRequest(
            prefix,
            midNumber,
            midText,
            endNumber,
            incrementMode);

        return _batchRenameService.Rename(
            filePairs,
            request);
    }

    /// <summary>
    /// 將系統例外轉換成使用者看得懂的訊息。
    /// </summary>
    public string? MapErrorToUserMessage(
        Exception? exception)
    {
        if (exception is null)
        {
            return null;
        }

        return exception switch
        {
            ArgumentException =>
                $"設定錯誤: {exception.Message}",

            DirectoryNotFoundException =>
                "資料夾不存在。請重新選擇資料夾。",

            UnauthorizedAccessException =>
                "無法存取資料夾。請檢查您的存取權限。",

            FormatException =>
                "格式錯誤。請檢查數字輸入。",

            IOException ioException =>
                ioException.Message.Contains(
                    "already exists",
                    StringComparison.OrdinalIgnoreCase)
                    ? "目標檔名已存在。請修改重新命名規則。"
                    : $"檔案操作失敗: {ioException.Message}",

            _ =>
                $"發生錯誤: {exception.Message}"
        };
    }
}