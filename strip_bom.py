import os

def strip_bom(file_path):
    with open(file_path, 'rb') as f:
        content = f.read()
    if content.startswith(b'\xef\xbb\xbf'):
        print(f"Stripping BOM from {file_path}")
        with open(file_path, 'wb') as f:
            f.write(content[3:])

views_dir = r"c:\Users\Zaidu\Downloads\Nueva carpeta (10)\SGE\SGE\Views"
for root, dirs, files in os.walk(views_dir):
    for file in files:
        if file.endswith('.cshtml'):
            strip_bom(os.path.join(root, file))
