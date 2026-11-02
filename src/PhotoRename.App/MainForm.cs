using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PhotoRename.App.Services;
using PhotoRename.Core.Models;

namespace PhotoRename.App;

public partial class MainForm : Form
{
    private readonly RenameFacade _facade;
    private readonly System.Windows.Forms.Timer _previewTimer;

    private List<FilePair>? _currentPhotos;

    public MainForm()
    {
        InitializeComponent();

        _facade = new RenameFacade();
        _previewTimer = new System.Windows.Forms.Timer
        {
            Interval = 250
        };

        _previewTimer.Tick += PreviewTimer_Tick;
        }

    private void BrowseButton_Click(
        object? sender,
        EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "選擇照片資料夾"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            FolderPathTextBox.Text =
                dialog.SelectedPath;
        }
    }

    private void ScanButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            var folderPath =
                FolderPathTextBox.Text;

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                MessageBox.Show(
                    "請先選擇資料夾。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            _currentPhotos = _facade.DiscoverPhotos(folderPath);

            if (_currentPhotos.Count == 0)
            {
                FileListGridView.DataSource = null;

                MessageBox.Show(
                    "未發現支援的照片檔案。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            FileListGridView.DataSource = _currentPhotos.Select(filePair => new

            {
                Original = filePair.ImageFile.Name,
                NewName = "尚未設定",
                JsonPaired = filePair.JsonFile != null ? "✓" : "-"
            })
            .ToList();

            MessageBox.Show(
                $"掃描完成，發現 {_currentPhotos.Count} 張照片。",
                "成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            var errorMsg =
                _facade.MapErrorToUserMessage(ex);

            MessageBox.Show(
                errorMsg,
                "錯誤",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void RefreshFileList()
    {
        if (_currentPhotos is null || _currentPhotos.Count == 0)
        {
            FileListGridView.DataSource = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(PrefixTextBox.Text))
        {
            return;
        }

        var midNumberText = MidNumberTextBox.Text.Trim();
        var endNumberText = EndNumberTextBox.Text.Trim();

        int? midNumber = null;
        int? endNumber = null;

        if (!string.IsNullOrWhiteSpace(midNumberText))
        {
            if (!int.TryParse(midNumberText, out var parsedMidNumber))
            {
                return;
            }

            midNumber = parsedMidNumber;
        }

        if (!string.IsNullOrWhiteSpace(endNumberText))
        {
            if (!int.TryParse(endNumberText, out var parsedEndNumber))
            {
                return;
            }

            endNumber = parsedEndNumber;
        }

        if (midNumber is null && endNumber is null)
        {
            return;
        }

        try
        {
            var displayList = _currentPhotos
                .Select((filePair, index) =>
                {
                    var newFileName = _facade.PreviewNewFilename(
                        filePair,
                        PrefixTextBox.Text.Trim(),
                        midNumber,
                        MidTextTextBox.Text,
                        endNumber,
                        GetIncrementMode(),
                        index);

                    return new
                    {
                        Original = filePair.ImageFile.Name,
                        NewName = newFileName,
                        JsonPaired = filePair.JsonFile != null ? "✓" : "-"
                    };
                })
                .ToList();

            FileListGridView.DataSource = displayList;
        }
        catch (Exception ex)
        {
            var errorMsg = _facade.MapErrorToUserMessage(ex);

            MessageBox.Show(
                errorMsg,
                "預覽錯誤",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateNumberInputState()
    {
        if (EndNumberRadioButton.Checked)
        {
            // 尾段數字遞增
            MidNumberTextBox.Enabled = false;
            EndNumberTextBox.Enabled = true;
        }
        else if (MidNumberRadioButton.Checked)
        {
            // 中段數字遞增
            MidNumberTextBox.Enabled = true;
            EndNumberTextBox.Enabled = false;
        }
        else if (BothRadioButton.Checked)
        {
            // 中段與尾段同時遞增
            MidNumberTextBox.Enabled = true;
            EndNumberTextBox.Enabled = true;
        }
    }

    private void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        _previewTimer.Stop();

        RefreshFileList();
    }

    private void OnRenameSettingChanged(object? sender, EventArgs e)
    {
        UpdateNumberInputState();
        if (_currentPhotos is null || _currentPhotos.Count == 0)
        {
            return;
        }

        // 使用者輸入尚未完成時，不顯示錯誤訊息
        if (string.IsNullOrWhiteSpace(PrefixTextBox.Text))
        {
            return;
        }

        _previewTimer.Stop();
        _previewTimer.Start();

    }

    private void RenameButton_Click(object? sender, EventArgs e)
    {
        if (_currentPhotos is null || _currentPhotos.Count == 0)
        {
            MessageBox.Show(
                "請先掃描照片。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        try
        {
            // 1. 驗證使用者輸入
            if (!ValidateRenameSettings(out var errorMsg))
            {
                MessageBox.Show(
                    errorMsg,
                    "設定錯誤",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // 2. 讀取中段數字
            var midNumberText = MidNumberTextBox.Text.Trim();

            int? midNumber = string.IsNullOrWhiteSpace(midNumberText)
                ? null
                : int.Parse(midNumberText);

            // 3. 讀取尾段數字
            var endNumberText = EndNumberTextBox.Text.Trim();

            int? endNumber = string.IsNullOrWhiteSpace(endNumberText)
                ? null
                : int.Parse(endNumberText);

            // 4. 取得目前遞增模式
            var incrementMode = GetIncrementMode();

            // 5. 執行批次重新命名
            var result = _facade.ExecuteRename(
                _currentPhotos,
                PrefixTextBox.Text.Trim(),
                midNumber,
                MidTextTextBox.Text,
                endNumber,
                incrementMode);

            // 6. 顯示重新命名結果
            DisplayRenameResult(result);
        }
        catch (Exception ex)
        {
            var errorMsg = _facade.MapErrorToUserMessage(ex);

            MessageBox.Show(
                errorMsg,
                "重新命名失敗",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private bool ValidateRenameSettings(out string errorMsg)
    {
        errorMsg = string.Empty;

        if (string.IsNullOrWhiteSpace(PrefixTextBox.Text))
        {
            errorMsg = "前綴不能為空。";
            return false;
        }

        var midText = MidTextTextBox.Text;

        var midNumberText = MidNumberTextBox.Text.Trim();
        var endNumberText = EndNumberTextBox.Text.Trim();

        var midNumberEmpty = string.IsNullOrWhiteSpace(midNumberText);
        var endNumberEmpty = string.IsNullOrWhiteSpace(endNumberText);

        if (midNumberEmpty && endNumberEmpty)
        {
            errorMsg = "中段數字與尾段數字至少需要輸入一個。";
            return false;
        }

        if (!midNumberEmpty &&
            !int.TryParse(midNumberText, out _))
        {
            errorMsg = "中段數字必須為有效的整數。";
            return false;
        }

        if (!endNumberEmpty &&
            !int.TryParse(endNumberText, out _))
        {
            errorMsg = "起始尾段數字必須為有效的整數。";
            return false;
        }

        return true;
    }

    private void DisplayRenameResult(
        BatchRenameResult result)
    {
        var message =
            result.Success
                ? $"重新命名完成！\n\n" +
                  $"成功: {result.SuccessCount}\n" +
                  $"失敗: {result.FailureCount}"
                : $"重新命名部分失敗。\n\n" +
                  $"成功: {result.SuccessCount}\n" +
                  $"失敗: {result.FailureCount}";

        if (result.FailureCount > 0)
        {
            var failedItems =
                result.Results
                    .Where(r => !r.Success)
                    .Select(r =>
                        $"• {r.FilePair.ImageFile.Name}: " +
                        $"{r.ErrorMessage}")
                    .Take(5);

            message +=
                "\n\n失敗的檔案:\n" +
                string.Join("\n", failedItems);

            if (result.FailureCount > 5)
            {
                message +=
                    $"\n... 還有 " +
                    $"{result.FailureCount - 5} 個檔案失敗";
            }
        }

        MessageBox.Show(
            message,
            "結果",
            MessageBoxButtons.OK,
            result.Success
                ? MessageBoxIcon.Information
                : MessageBoxIcon.Warning);

        if (result.SuccessCount > 0)
        {
            RefreshFileList();
        }
    }

    private IncrementMode GetIncrementMode()
    {
        if (BothRadioButton.Checked)
        {
            return IncrementMode.BothNumeric;
        }

        if (MidNumberRadioButton.Checked)
        {
            return IncrementMode.MidNumeric;
        }

        if (EndNumberRadioButton.Checked)
        {
            return IncrementMode.Numeric;
        }

        return IncrementMode.None;
    }

    private void InitializeComponent()
    {
        var mainPanel =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10),
                AutoSize = true
            };

        mainPanel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        mainPanel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        mainPanel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        mainPanel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        var folderPanel =
            CreateFolderSelectionPanel();

        var settingsPanel =
            CreateRenameSettingsPanel();

        var fileListPanel =
            CreateFileListPanel();

        var buttonPanel =
            CreateButtonPanel();

        mainPanel.Controls.Add(
            folderPanel,
            0,
            0);

        mainPanel.Controls.Add(
            settingsPanel,
            0,
            1);

        mainPanel.Controls.Add(
            fileListPanel,
            0,
            2);

        mainPanel.Controls.Add(
            buttonPanel,
            0,
            3);

        Controls.Add(mainPanel);

        Text = "Photo Rename Tool";

        Size =
            new System.Drawing.Size(
                900,
                700);

        StartPosition =
            FormStartPosition.CenterScreen;

        AutoScaleMode =
            AutoScaleMode.Font;
    }

    private Control CreateFolderSelectionPanel()
    {
        var panel =
            new GroupBox
            {
                Text = "來源資料夾",
                Dock = DockStyle.Fill,
                Height = 105
            };

        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(5)
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.AutoSize));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.AutoSize));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.AutoSize));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.AutoSize));

        FolderPathTextBox =
            new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };

        var browseButton =
            new Button
            {
                Text = "瀏覽...",
                Width = 80,
                Dock = DockStyle.Right
        };

        browseButton.Click += BrowseButton_Click;

        var scanButton =
            new Button
            {
                Text = "掃描照片",
                Width = 120,
                Dock = DockStyle.Right
            };

        scanButton.Click += ScanButton_Click;

        layout.Controls.Add(FolderPathTextBox, 0, 0);

        layout.Controls.Add(browseButton, 1, 0);

        layout.Controls.Add(scanButton, 2, 0);

        var hintLabel =
            new Label
            {
                Text = "提示：重新命名後若要再次修改，請重新掃描以更新照片清單。",
                AutoSize = true,
                ForeColor = System.Drawing.SystemColors.GrayText,
                Margin = new Padding(0, 5, 0, 0)
            };

        layout.Controls.Add(hintLabel, 0, 1);

        layout.SetColumnSpan(hintLabel, 3);

        panel.Controls.Add(layout);

        return panel;
    }

    private Control CreateRenameSettingsPanel()
    {
        var panel =
            new GroupBox
            {
                Text = "重新命名設定",
                Dock = DockStyle.Fill,
                Height = 180
            };

        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(5)
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50));

        layout.Controls.Add(
            new Label
            {
                Text = "前綴:",
                AutoSize = true
            },
            0,
            0);

        PrefixTextBox =
            new TextBox
            {
                Dock = DockStyle.Fill
            };

        PrefixTextBox.TextChanged +=
            OnRenameSettingChanged;

        layout.Controls.Add(
            PrefixTextBox,
            1,
            0);

        layout.Controls.Add(
            new Label
            {
                Text = "中段數字:",
                AutoSize = true
            },
            2,
            0);

        MidNumberTextBox =
            new TextBox
            {
                Dock = DockStyle.Fill,
                Text = "1"
            };

        MidNumberTextBox.TextChanged +=
            OnRenameSettingChanged;

        layout.Controls.Add(
            MidNumberTextBox,
            3,
            0);

        layout.Controls.Add(
            new Label
            {
                Text = "中段文字:",
                AutoSize = true
            },
            0,
            1);

        MidTextTextBox =
            new TextBox
            {
                Dock = DockStyle.Fill
            };

        MidTextTextBox.TextChanged +=
            OnRenameSettingChanged;

        layout.Controls.Add(
            MidTextTextBox,
            1,
            1);

        layout.Controls.Add(
            new Label
            {
                Text = "起始尾段數字:",
                AutoSize = true
            },
            2,
            1);

        EndNumberTextBox =
            new TextBox
            {
                Dock = DockStyle.Fill,
                Text = "1"
            };

        EndNumberTextBox.TextChanged +=
            OnRenameSettingChanged;

        layout.Controls.Add(
            EndNumberTextBox,
            3,
            1);

        var modePanel =
            new GroupBox
            {
                Text = "遞增模式",
                Dock = DockStyle.Fill
            };

        var modeLayout =
            new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection =
                    FlowDirection.LeftToRight,
                AutoSize = true
            };

        EndNumberRadioButton =
            new RadioButton
            {
                Text = "尾段數字遞增",
                Checked = true,
                AutoSize = true
            };

        EndNumberRadioButton.CheckedChanged +=
            OnRenameSettingChanged;

        MidNumberRadioButton =
            new RadioButton
            {
                Text = "中段數字遞增",
                AutoSize = true
            };

        MidNumberRadioButton.CheckedChanged +=
            OnRenameSettingChanged;

        BothRadioButton =
            new RadioButton
            {
                Text = "中段與尾段同時遞增",
                AutoSize = true
            };

        BothRadioButton.CheckedChanged +=
            OnRenameSettingChanged;

        modeLayout.Controls.Add(
            EndNumberRadioButton);

        modeLayout.Controls.Add(
            MidNumberRadioButton);

        modeLayout.Controls.Add(
            BothRadioButton);

        modePanel.Controls.Add(
            modeLayout);

        UpdateNumberInputState();

        layout.Controls.Add(
            modePanel,
            0,
            2);

        layout.SetColumnSpan(
            modePanel,
            4);

        panel.Controls.Add(layout);

        return panel;
    }

    private Control CreateFileListPanel()
    {
        var panel =
            new GroupBox
            {
                Text = "照片清單與預覽",
                Dock = DockStyle.Fill
            };

        FileListGridView =
            new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect
            };

        var colOriginal =
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Original",
                HeaderText = "原始檔名",
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            };

        var colNew =
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NewName",
                HeaderText = "新檔名",
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            };

        var colJson =
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "JsonPaired",
                HeaderText = "JSON配對",
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.AllCells
            };

        FileListGridView.Columns.Add(
            colOriginal);

        FileListGridView.Columns.Add(
            colNew);

        FileListGridView.Columns.Add(
            colJson);

        panel.Controls.Add(
            FileListGridView);

        return panel;
    }

    private Panel CreateButtonPanel()
    {
        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50
            };

        var layout =
            new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection =
                    FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

        var renameButton =
            new Button
            {
                Text = "重新命名照片",
                Width = 150,
                Height = 40
            };

        renameButton.Click +=
            RenameButton_Click;

        layout.Controls.Add(
            renameButton);

        panel.Controls.Add(layout);

        return panel;
    }

    private TextBox FolderPathTextBox = null!;

    private TextBox PrefixTextBox = null!;

    private TextBox MidNumberTextBox = null!;

    private TextBox MidTextTextBox = null!;

    private TextBox EndNumberTextBox = null!;

    private RadioButton EndNumberRadioButton = null!;

    private RadioButton MidNumberRadioButton = null!;

    private RadioButton BothRadioButton = null!;

    private DataGridView FileListGridView = null!;
}