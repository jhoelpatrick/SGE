import os

project_dir = r"c:\Users\alu_torre1\Desktop\SGE"

replacements = {
    "Díashboard": "Dashboard",
    "díasboard": "dashboard",
    "Díasboard": "Dashboard",
    "urlDíashboard": "urlDashboard"
}

count = 0
for root, dirs, files in os.walk(project_dir):
    for file in files:
        if file.endswith((".cshtml", ".cs")):
            file_path = os.path.join(root, file)
            try:
                with open(file_path, "r", encoding="utf-8-sig") as f:
                    content = f.read()
            except UnicodeDecodeError:
                try:
                    with open(file_path, "r", encoding="cp1252") as f:
                        content = f.read()
                except Exception:
                    continue

            modified = False
            for k, v in replacements.items():
                if k in content:
                    content = content.replace(k, v)
                    modified = True
            
            if modified:
                with open(file_path, "w", encoding="utf-8-sig") as f:
                    f.write(content)
                count += 1
                print(f"Fixed Dashboard spelling in: {file_path}")

print(f"Finished fixing spelling in {count} files.")
