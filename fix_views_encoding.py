import os

views_dir = r"c:\Users\alu_torre1\Desktop\SGE\SGE\Views"

replacements = {
    "ltimas": "Últimas",
    "ltimos": "Últimos",
    "ltimo": "Último",
    "nicas": "Únicas",
    "nico": "Único",
    "tula": "Útula",
    "NMINA": "NÓMINA",
    "Nmina": "Nómina",
    "nmina": "nómina",
    "Configuracin": "Configuración",
    "configuracin": "configuración",
    "Facturacin": "Facturación",
    "facturacin": "facturación",
    "Regmenes": "Regímenes",
    "regmenes": "regímenes",
    "Rgimen": "Régimen",
    "rgimen": "régimen",
    "Das": "Días",
    "das": "días",
    "Categora": "Categoría",
    "categora": "categoría",
    "Bsqueda": "Búsqueda",
    "bsqueda": "búsqueda",
    "Perodo": "Período",
    "perodo": "período",
    "Cdigo": "Código",
    "cdigo": "código",
    "Telfono": "Teléfono",
    "telfono": "teléfono",
    "Artculo": "Artículo",
    "artculo": "artículo",
    "Crdito": "Crédito",
    "crdito": "crédito",
    "Dbito": "Débito",
    "dbito": "débito",
    "": ""
}

count_fixed = 0

for root, dirs, files in os.walk(views_dir):
    for file in files:
        if file.endswith(".cshtml"):
            file_path = os.path.join(root, file)
            # Try reading as UTF-8
            try:
                with open(file_path, "r", encoding="utf-8") as f:
                    content = f.read()
            except UnicodeDecodeError:
                # Try reading as Windows-1252
                try:
                    with open(file_path, "r", encoding="cp1252") as f:
                        content = f.read()
                except Exception as e:
                    print(f"Error reading {file_path}: {e}")
                    continue
            
            modified = False
            # Check if any key exists in the content
            # Also check for \uFFFD directly
            if "\uFFFD" in content:
                modified = True
                for k, v in replacements.items():
                    content = content.replace(k, v)
                content = content.replace("\uFFFD", "")
            
            # Even if not modified by replacement character, we still want to save as UTF-8 with BOM
            # to prevent IIS/Kestrel/Razor from misinterpreting it.
            # Let's save every file as UTF-8 with BOM (utf-8-sig).
            try:
                with open(file_path, "w", encoding="utf-8-sig") as f:
                    f.write(content)
                if modified:
                    count_fixed += 1
                    print(f"Fixed encoding and characters in: {file_path}")
            except Exception as e:
                print(f"Error writing {file_path}: {e}")

print(f"Completed! Total files modified for replacement characters: {count_fixed}")
