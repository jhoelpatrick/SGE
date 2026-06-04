import os

project_dir = r"c:\Users\Zaidu\Downloads\Nueva carpeta (10)\SGE\SGE"

replacements = {
    "PlanillasPagadías": "PlanillasPagadas",
    "PlanillasAnuladías": "PlanillasAnuladas",
    "StatsPagadías": "StatsPagadas",
    "UÚÚÚltimasPlanillas": "UltimasPlanillas",
    "UÚÚltimasPlanillas": "UltimasPlanillas",
    "UÚltimasPlanillas": "UltimasPlanillas",
    "ÚltimasPlanillas": "UltimasPlanillas"
}

count_fixed = 0
for root, dirs, files in os.walk(project_dir):
    for file in files:
        if file.endswith(".cshtml"):
            file_path = os.path.join(root, file)
            try:
                with open(file_path, "r", encoding="utf-8-sig") as f:
                    content = f.read()
            except UnicodeDecodeError:
                try:
                    with open(file_path, "r", encoding="cp1252") as f:
                        content = f.read()
                except Exception as e:
                    print(f"Error reading {file_path}: {e}")
                    continue
            
            modified = False
            for k, v in replacements.items():
                if k in content:
                    content = content.replace(k, v)
                    modified = True
            
            if modified:
                try:
                    with open(file_path, "w", encoding="utf-8-sig") as f:
                        f.write(content)
                    count_fixed += 1
                    print(f"Fixed spelling bug in: {file_path}")
                except Exception as e:
                    print(f"Error writing {file_path}: {e}")

print(f"\nSuccessfully fixed {count_fixed} files!")
