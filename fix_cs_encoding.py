import os

project_dir = r"c:\Users\alu_torre1\Desktop\SGE\SGE"

for root, dirs, files in os.walk(project_dir):
    for file in files:
        if file.endswith(".cs"):
            file_path = os.path.join(root, file)
            try:
                with open(file_path, "r", encoding="utf-8") as f:
                    content = f.read()
            except UnicodeDecodeError:
                try:
                    with open(file_path, "r", encoding="cp1252") as f:
                        content = f.read()
                except Exception as e:
                    print(f"Error reading {file_path}: {e}")
                    continue
            
            try:
                with open(file_path, "w", encoding="utf-8-sig") as f:
                    f.write(content)
            except Exception as e:
                print(f"Error writing {file_path}: {e}")

print("Saved all .cs files with UTF-8 BOM encoding.")
