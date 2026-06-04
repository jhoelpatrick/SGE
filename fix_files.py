import os

project_dir = r"c:\Users\Zaidu\Downloads\Nueva carpeta (10)\SGE\SGE"

def fix_file_content(content):
    # Fix the 👤 operator (make sure to replace both with spaces and handle potential cases)
    content = content.replace(" 👤 ", " ?? ")
    
    # Check if the file has the double newline issue (every even line is empty)
    # Let's split by system newlines
    lines = content.splitlines(keepends=True)
    if len(lines) > 4:
        # Check if lines [1, 3, 5, 7...] (0-indexed: 1, 3, 5) are all empty (only whitespace/newline)
        even_lines_empty = True
        for i in range(1, len(lines), 2):
            if lines[i].strip() != "":
                even_lines_empty = False
                break
        
        if even_lines_empty:
            # We reconstruct the file using only the odd lines (0-indexed: 0, 2, 4...)
            new_lines = [lines[i] for i in range(0, len(lines), 2)]
            content = "".join(new_lines)
            
    return content

count_fixed = 0
for root, dirs, files in os.walk(project_dir):
    for file in files:
        if file.endswith((".cshtml", ".cs", ".json")):
            file_path = os.path.join(root, file)
            # Try reading the file
            try:
                with open(file_path, "r", encoding="utf-8-sig") as f:
                    content = f.read()
                encoding = "utf-8-sig"
            except UnicodeDecodeError:
                try:
                    with open(file_path, "r", encoding="cp1252") as f:
                        content = f.read()
                    encoding = "cp1252"
                except Exception as e:
                    print(f"Error reading {file_path}: {e}")
                    continue
            
            # Apply fix
            fixed_content = fix_file_content(content)
            
            if fixed_content != content:
                try:
                    # Write it back using utf-8-sig (UTF-8 with BOM) as required by .NET Razor
                    with open(file_path, "w", encoding="utf-8-sig") as f:
                        f.write(fixed_content)
                    count_fixed += 1
                    print(f"Fixed file: {file_path}")
                except Exception as e:
                    print(f"Error writing {file_path}: {e}")

print(f"\nSuccessfully fixed {count_fixed} files!")
