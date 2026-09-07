# Photo Rename Tool — Requirements

## 1. Project Overview

Photo Rename Tool 是一個照片批次重新命名工具。

本專案的第一階段目標，是將既有的 Python 照片命名工具重新實作為 C# / .NET 應用程式。

原始 Python 程式位於：

`legacy/photo_rename.py`

C# 版本應盡可能保留原始程式的核心功能與使用行為，同時改善程式架構、可測試性、錯誤處理與檔案安全性。

---

## 2. Project Goals

本專案第一階段目標：

1. 使用 C# / .NET 實作照片批次重新命名功能。
2. 支援 JPG、JPEG、PNG 圖片。
3. 支援圖片與同名 JSON 標記檔同步重新命名。
4. 支援自訂檔名前綴。
5. 支援中段數字與尾段數字。
6. 支援三種數字遞增模式。
7. 避免重新命名過程造成檔案遺失。
8. 建立可測試的商業邏輯。
9. 建立 Unit Tests。
10. 保持程式架構簡單、容易維護。

---

## 3. Supported Image Formats

第一階段支援：

* `.jpg`
* `.jpeg`
* `.png`

副檔名判斷應不區分大小寫。

例如：

```text
photo.jpg
photo.JPG
photo.jpeg
photo.JPEG
photo.png
photo.PNG
```

皆應視為支援的圖片格式。

其他檔案格式第一階段不處理。

---

## 4. JSON Pairing

如果圖片存在同名 JSON 標記檔，圖片與 JSON 必須視為一組。

例如：

```text
IMG001.jpg
IMG001.json
```

重新命名後：

```text
TY-LV-448_20240201_白菜_蚜蟲_有翅蟲體_48.jpg
TY-LV-448_20240201_白菜_蚜蟲_有翅蟲體_48.json
```

JSON 副檔名判斷應不區分大小寫。

例如：

```text
IMG001.json
IMG001.JSON
```

皆應視為可能的對應 JSON。

如果圖片沒有對應 JSON：

* 圖片仍然可以重新命名。
* 不應因為缺少 JSON 而導致整批處理失敗。

---

## 5. Rename Format

檔名格式：

```text
{prefix}{mid_num:000}{mid_text}{end_num:00}
```

例如：

```text
Prefix:
TY-LV-

Mid Number:
448

Mid Text:
_20240201_白菜_蚜蟲_有翅蟲體_

End Number:
48
```

產生：

```text
TY-LV-448_20240201_白菜_蚜蟲_有翅蟲體_48.jpg
```

其中：

### Prefix

使用者自訂的固定前綴。

例如：

```text
TY-LV-
```

### Mid Number

三位數格式。

例如：

```text
448
```

如果輸入：

```text
1
```

則應格式化為：

```text
001
```

### Mid Text

使用者自訂文字，可以為空。

### End Number

兩位數格式。

例如：

```text
48
```

如果輸入：

```text
1
```

則應格式化為：

```text
01
```

---

## 6. Increment Modes

系統提供三種遞增模式。

### Mode 1 — End Number

僅尾段數字遞增。

例如：

```text
448 / 48
448 / 49
448 / 50
```

產生：

```text
TY-LV-448_xxx_48.jpg
TY-LV-448_xxx_49.jpg
TY-LV-448_xxx_50.jpg
```

---

### Mode 2 — Mid Number

僅中段數字遞增。

例如：

```text
448 / 48
449 / 48
450 / 48
```

---

### Mode 3 — Both

中段與尾段同時遞增。

例如：

```text
448 / 48
449 / 49
450 / 50
```

---

## 7. File Processing

系統處理流程至少應包含：

1. 取得使用者指定的資料夾。
2. 掃描資料夾中的檔案。
3. 找出支援的圖片。
4. 依檔名排序圖片。
5. 找出圖片對應的 JSON。
6. 建立圖片與 JSON 的配對資訊。
7. 產生新的檔名。
8. 檢查目標檔名是否衝突。
9. 執行重新命名。
10. 顯示處理結果。

---

## 8. File Safety

重新命名功能不得因檔名衝突或處理錯誤造成不必要的資料遺失。

系統至少需要考慮：

* 目標檔名已存在。
* 圖片沒有對應 JSON。
* JSON 副檔名大小寫不同。
* 不支援的檔案。
* 檔案不存在。
* 檔案無法存取。
* 重新命名失敗。
* 資料夾沒有圖片。

第一階段不得直接使用不可逆的刪除行為來解決檔名衝突。

如遇到可能造成資料覆蓋的情況，應優先阻止操作並回報錯誤。

---

## 9. Error Handling

系統應提供清楚的錯誤資訊。

至少處理：

* 無法選擇資料夾。
* 找不到圖片。
* 輸入數字格式錯誤。
* 檔案存取權限不足。
* 目標檔名衝突。
* 檔案重新命名失敗。

錯誤不應以未處理的 Exception 直接終止整個應用程式。

---

## 10. Testing Requirements

核心商業邏輯必須建立 Unit Tests。

至少測試：

### Filename Generation

* Prefix 正確處理。
* Mid Number 三位數格式化。
* End Number 兩位數格式化。
* Mid Text 為空。
* Mid Text 包含中文。

### Increment Modes

* Mode 1 正確遞增。
* Mode 2 正確遞增。
* Mode 3 正確遞增。

### JSON Pairing

* 找到同名 JSON。
* JSON 不存在。
* JSON 副檔名大小寫不同。

### File Validation

* JPG。
* JPEG。
* PNG。
* 不支援的副檔名。

### Collision Handling

* 目標檔名已存在時應正確阻止危險操作。

---

## 11. Scope

### 第一階段包含

* 批次照片重新命名。
* JSON 同步重新命名。
* 三種遞增模式。
* 檔案安全檢查。
* 錯誤處理。
* Unit Tests。
* C# / .NET 實作。

### 第一階段不包含

* EXIF 解析。
* AI 圖片辨識。
* pHash 重複照片辨識。
* Web API。
* Database。
* 使用者登入。
* Cloud Storage。
* SignalR。
* 工作流引擎。

這些功能屬於後續 Photo Workflow Platform 的擴充範圍。

---

## 12. Reference Implementation

原始 Python 實作：

```text
legacy/photo_rename.py
```

C# 實作可以參考原始 Python 程式的行為，但不得直接假設 Python 的程式結構必須被複製到 C#。

C# 應依照適合 .NET 的架構重新設計。

---

## 13. Success Criteria

第一階段完成時：

1. 可以成功建立 C# / .NET 專案。
2. 可以掃描指定資料夾中的圖片。
3. 可以正確產生新的檔名。
4. 可以正確處理三種遞增模式。
5. 可以同步重新命名對應 JSON。
6. 可以處理檔名衝突。
7. 可以處理基本錯誤情況。
8. Unit Tests 通過。
9. 程式架構清楚且容易維護。
10. C# 版本的主要行為與原 Python 工具一致。
