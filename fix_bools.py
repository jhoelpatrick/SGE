import os
import re

def fix_booleans(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # Find occurrences of "estado = 1" or "estado = 0"
    # Also "estaactivo = 1" etc
    # Let's just do a blanket regex for "estado = 1", "estado = 0", "estaactivo = 1", "estaactivo = 0"
    
    content = re.sub(r'(?i)\bestado\s*=\s*1\b', 'estado = TRUE', content)
    content = re.sub(r'(?i)\bestado\s*=\s*0\b', 'estado = FALSE', content)
    content = re.sub(r'(?i)\bestaactivo\s*=\s*1\b', 'estaactivo = TRUE', content)
    content = re.sub(r'(?i)\bestaactivo\s*=\s*0\b', 'estaactivo = FALSE', content)
    
    # Let's also catch some others that might exist like "esfijo = 1"
    content = re.sub(r'(?i)\besfijo\s*=\s*1\b', 'esfijo = TRUE', content)
    content = re.sub(r'(?i)\besfijo\s*=\s*0\b', 'esfijo = FALSE', content)
    
    content = re.sub(r'(?i)\bes_activa\s*=\s*1\b', 'es_activa = TRUE', content)
    content = re.sub(r'(?i)\bes_activa\s*=\s*0\b', 'es_activa = FALSE', content)
    
    # General boolean column names used in the script
    bool_columns = ['estado', 'estaactivo', 'esfijo', 'es_activa', 'es_usado', 'es_revocado', 'exito', 'es_activo', 'estasesionactiva', 'congocehaber', 'essubsidiado', 'estaabierto']
    for col in bool_columns:
        content = re.sub(fr'(?i)\b{col}\s*=\s*1\b', f'{col} = TRUE', content)
        content = re.sub(fr'(?i)\b{col}\s*=\s*0\b', f'{col} = FALSE', content)

    # Let's handle inserts into boolean columns in artificial_data.sql if there are any 1 or 0 values
    # Actually it's very hard to do that generically. But usually inserts in the script might be:
    # insert into ... values (..., 1, ...) 
    # Hopefully artificial_data doesn't fail, or if it does we'll fix it.

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

fix_booleans(r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\script_crm_postgres.sql')
try:
    fix_booleans(r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\artificial_data_postgres.sql')
except:
    pass

