import os
import uuid
import tkinter as tk
from tkinter import filedialog, messagebox

def rename_images(entries):
    # 讀取介面輸入數值
    prefix = entries['prefix'].get().strip()
    mid_text = entries['mid_text'].get().strip()
    
    try:
        mid_num = int(entries['mid_num'].get().strip())
        end_num = int(entries['end_num'].get().strip())
    except ValueError:
        messagebox.showwarning("錯誤", "請確認『中段起始數字』與『尾段起始數字』皆為純數字！")
        return

    # 讀取遞增模式
    mode = entries['mode'].get()

    # 選擇資料夾
    folder_path = filedialog.askdirectory(title="請選擇圖片與標記資料夾")
    if not folder_path:
        return

    image_extensions = ('.jpg', '.jpeg', '.png')

    try:
        # 1. 先抓出所有圖片檔名
        all_files = os.listdir(folder_path)
        image_files = sorted([
            f for f in all_files if f.lower().endswith(image_extensions)
        ])

        if not image_files:
            messagebox.showwarning("沒有圖片", "選取的資料夾中沒有圖片檔案。")
            return

        # ----------------------------------------------------
        # 第一階段：改為臨時唯一名稱（連同對應的 JSON 一起處理）
        # ----------------------------------------------------
        # 結構: [(img_temp_path, img_ext, json_old_temp_path_or_None), ...]
        temp_tasks = []
        
        for filename in image_files:
            base_no_ext, img_ext = os.path.splitext(filename)
            old_img_path = os.path.join(folder_path, filename)
            
            # 檢查是否有同名的 json 標記檔
            possible_json_name = base_no_ext + '.json'
            # 考慮到可能有人寫大寫 .JSON，做個比對
            json_filename = next((f for f in all_files if f.lower() == possible_json_name.lower()), None)
            
            unique_id = uuid.uuid4().hex
            
            # 處理圖片臨時改名
            temp_img_name = f"temp_{unique_id}{img_ext}"
            temp_img_path = os.path.join(folder_path, temp_img_name)
            os.rename(old_img_path, temp_img_path)
            
            # 處理 JSON 臨時改名 (如果存在的話)
            temp_json_path = None
            if json_filename:
                old_json_path = os.path.join(folder_path, json_filename)
                temp_json_name = f"temp_{unique_id}.json"
                temp_json_path = os.path.join(folder_path, temp_json_name)
                os.rename(old_json_path, temp_json_path)
                
            temp_tasks.append((temp_img_path, img_ext, temp_json_path))

        # ----------------------------------------------------
        # 第二階段：正式命名為新格式
        # ----------------------------------------------------
        current_mid = mid_num
        current_end = end_num

        for temp_img_path, img_ext, temp_json_path in temp_tasks:
            # 產生新檔名（不含副檔名）
            new_base_name = f"{prefix}{current_mid:03d}{mid_text}{current_end:02d}"
            
            # 1. 變更圖片名稱
            new_img_full_path = os.path.join(folder_path, f"{new_base_name}{img_ext}")
            if os.path.exists(new_img_full_path):
                os.remove(new_img_full_path)
            os.rename(temp_img_path, new_img_full_path)
            
            # 2. 變更 JSON 名稱 (如果有標記檔的話)
            if temp_json_path:
                new_json_full_path = os.path.join(folder_path, f"{new_base_name}.json")
                if os.path.exists(new_json_full_path):
                    os.remove(new_json_full_path)
                os.rename(temp_json_path, new_json_full_path)

            # 依模式遞增數字
            if mode == 1:
                current_end += 1
            elif mode == 2:
                current_mid += 1
            elif mode == 3:
                current_mid += 1
                current_end += 1

        messagebox.showinfo("完成", f"成功重新命名 {len(image_files)} 張圖片及其對應的標記檔！")

    except Exception as e:
        messagebox.showerror("錯誤", f"發生預期之外的錯誤：\n{str(e)}")

def main():
    window = tk.Tk()
    window.title("自訂照片與標記連動命名器 v4.0")
    window.geometry("500x450")
    window.eval('tk::PlaceWindow . center')

    entries = {}

    # 1. 固定前綴
    tk.Label(window, text="1. 固定前綴 (例如: TY-LV-):", anchor="w").pack(fill="x", padx=20, pady=(10,0))
    entries['prefix'] = tk.Entry(window, width=55)
    entries['prefix'].insert(0, "TY-LV-")
    entries['prefix'].pack(padx=20)

    # 2. 中段起始數字
    tk.Label(window, text="2. 中段起始數字 (例如: 448):", anchor="w").pack(fill="x", padx=20, pady=(10,0))
    entries['mid_num'] = tk.Entry(window, width=55)
    entries['mid_num'].insert(0, "448")
    entries['mid_num'].pack(padx=20)

    # 3. 中段自訂文字
    tk.Label(window, text="3. 中段自訂文字 (可自由修改或留空):", anchor="w").pack(fill="x", padx=20, pady=(10,0))
    entries['mid_text'] = tk.Entry(window, width=55)
    entries['mid_text'].insert(0, "_20240201_白菜_蚜蟲_有翅蟲體_")
    entries['mid_text'].pack(padx=20)

    # 4. 尾段起始數字
    tk.Label(window, text="4. 尾段起始數字 (例如: 48):", anchor="w").pack(fill="x", padx=20, pady=(10,0))
    entries['end_num'] = tk.Entry(window, width=55)
    entries['end_num'].insert(0, "48")
    entries['end_num'].pack(padx=20)

    # 5. 遞增模式選擇
    tk.Label(window, text="5. 請選擇隨圖片順序遞增的欄位:", anchor="w").pack(fill="x", padx=20, pady=(15,0))
    mode_var = tk.IntVar(value=1)
    entries['mode'] = mode_var
    
    tk.Radiobutton(window, text="僅尾段數字遞增 (48 ➔ 49 ➔ 50)", variable=mode_var, value=1).pack(anchor="w", padx=30)
    tk.Radiobutton(window, text="僅中段數字遞增 (448 ➔ 449 ➔ 450)", variable=mode_var, value=2).pack(anchor="w", padx=30)
    tk.Radiobutton(window, text="兩者同時遞增", variable=mode_var, value=3).pack(anchor="w", padx=30)

    # 執行按鈕
    button = tk.Button(window, text="選擇資料夾並開始同步重命名",
                       command=lambda: rename_images(entries),
                       width=30, height=2, bg="#d1ecf1") # 改為粉藍色按鈕區隔
    button.pack(pady=20)

    window.mainloop()

if __name__ == "__main__":
    main()