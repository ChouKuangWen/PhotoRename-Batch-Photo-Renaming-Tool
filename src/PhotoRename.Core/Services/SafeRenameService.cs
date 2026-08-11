using System;
using System.IO;
using PhotoRename.Core.Models;

namespace PhotoRename.Core.Services;

public sealed class SafeRenameService
{
    private readonly FilenameGenerator _filenameGenerator;
    private readonly Action<string, string> _moveFile;

    public SafeRenameService(FilenameGenerator? filenameGenerator = null, Action<string, string>? moveFile = null)
    {
        _filenameGenerator = filenameGenerator ?? new FilenameGenerator();
        _moveFile = moveFile ?? MoveFile;
    }

    public RenameResult Rename(FilePair filePair, RenameRequest request)
    {
        if (filePair is null)
        {
            throw new ArgumentNullException(nameof(filePair));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var imageFile = filePair.ImageFile;
        var jsonFile = filePair.JsonFile;
        var extension = imageFile.Extension;
        var newFileName = _filenameGenerator.Generate(request);
        var newImageFilePath = Path.Combine(imageFile.DirectoryName!, newFileName + extension);
        var newJsonFilePath = jsonFile is null ? null : Path.Combine(jsonFile.DirectoryName!, newFileName + jsonFile.Extension);

        if (HasCollision(newImageFilePath, imageFile.FullName) || HasCollision(newJsonFilePath, jsonFile?.FullName))
        {
            return new RenameResult
            {
                Success = false,
                FilePair = filePair,
                ErrorMessage = "Target file already exists."
            };
        }

        var tempImageFilePath = Path.Combine(imageFile.DirectoryName!, Guid.NewGuid().ToString("N") + extension);

        try
        {
            _moveFile(imageFile.FullName, tempImageFilePath);

            if (jsonFile is null)
            {
                try
                {
                    _moveFile(tempImageFilePath, newImageFilePath);

                    return new RenameResult
                    {
                        Success = true,
                        FilePair = filePair,
                        NewImageFile = new FileInfo(newImageFilePath)
                    };
                }
                catch (Exception exception)
                {
                    RestoreOriginalState(imageFile.FullName, tempImageFilePath, newImageFilePath, null, null, null);

                    return new RenameResult
                    {
                        Success = false,
                        FilePair = filePair,
                        ErrorMessage = exception.Message,
                        Exception = exception
                    };
                }
            }

            var tempJsonFilePath = Path.Combine(jsonFile.DirectoryName!, Guid.NewGuid().ToString("N") + jsonFile.Extension);

            try
            {
                _moveFile(jsonFile.FullName, tempJsonFilePath);
            }
            catch (Exception exception)
            {
                RestoreOriginalState(imageFile.FullName, tempImageFilePath, null, jsonFile.FullName, null, null);

                return new RenameResult
                {
                    Success = false,
                    FilePair = filePair,
                    ErrorMessage = exception.Message,
                    Exception = exception
                };
            }

            try
            {
                _moveFile(tempImageFilePath, newImageFilePath);
            }
            catch (Exception exception)
            {
                RestoreOriginalState(imageFile.FullName, tempImageFilePath, null, jsonFile.FullName, tempJsonFilePath, null);

                return new RenameResult
                {
                    Success = false,
                    FilePair = filePair,
                    ErrorMessage = exception.Message,
                    Exception = exception
                };
            }

            try
            {
                _moveFile(tempJsonFilePath, newJsonFilePath!);

                return new RenameResult
                {
                    Success = true,
                    FilePair = filePair,
                    NewImageFile = new FileInfo(newImageFilePath),
                    NewJsonFile = new FileInfo(newJsonFilePath!)
                };
            }
            catch (Exception exception)
            {
                RestoreOriginalState(imageFile.FullName, tempImageFilePath, newImageFilePath, jsonFile.FullName, tempJsonFilePath, newJsonFilePath);

                return new RenameResult
                {
                    Success = false,
                    FilePair = filePair,
                    ErrorMessage = exception.Message,
                    Exception = exception
                };
            }
        }
        catch (Exception exception)
        {
            return new RenameResult
            {
                Success = false,
                FilePair = filePair,
                ErrorMessage = exception.Message,
                Exception = exception
            };
        }
    }

    private static bool HasCollision(string? targetPath, string? sourcePath)
    {
        if (string.IsNullOrEmpty(targetPath))
        {
            return false;
        }

        return File.Exists(targetPath) && !string.Equals(targetPath, sourcePath, StringComparison.OrdinalIgnoreCase);
    }

    private static void RestoreOriginalState(string originalImagePath, string tempImagePath, string? targetImagePath, string? originalJsonPath, string? tempJsonPath, string? targetJsonPath)
    {
        RestoreSingleFileToOriginal(originalImagePath, tempImagePath, targetImagePath);
        RestoreSingleFileToOriginal(originalJsonPath, tempJsonPath, targetJsonPath);
    }

    private static void RestoreSingleFileToOriginal(string? originalPath, string? tempPath, string? targetPath)
    {
        if (string.IsNullOrEmpty(originalPath))
        {
            return;
        }

        if (File.Exists(originalPath))
        {
            RemoveIfExists(tempPath);
            RemoveIfExists(targetPath);
            return;
        }

        if (targetPath is not null && File.Exists(targetPath))
        {
            File.Move(targetPath, originalPath);
            return;
        }

        if (tempPath is not null && File.Exists(tempPath))
        {
            File.Move(tempPath, originalPath);
        }
    }

    private static void RemoveIfExists(string? path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void MoveFile(string sourcePath, string destinationPath)
    {
        File.Move(sourcePath, destinationPath);
    }
}
