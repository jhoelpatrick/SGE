import os
import re

def fix_concat(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # In SQL Server, concat with + is common. 
    # Example: (cf.serie + '-' + cf.correlativo) -> (cf.serie || '-' || cf.correlativo)
    # This is tricky because + is also used for math (e.g., stockactual = stockactual + @p_cantidad).
    # But usually, string concatenation involves quotes like '-' or ' '.
    
    # We can replace ` + '` with ` || '`
    content = re.sub(r'\s*\+\s*\'', " || '", content)
    # And `' + ` with `' || `
    content = re.sub(r'\'\s*\+\s*', "' || ", content)
    
    # What about column + column?
    # e.g., (serie + '-' + correlativo) is handled above because of the '-'.
    # But if there's `col1 + col2` where both are strings, it won't be caught.
    # Looking at the SQL Server script, the only concatenations usually are with literals or inside views.
    # We also replaced `ISNULL` with `COALESCE`.
    
    # Let's also check for `BIT` fields inside the script. We might have missed some default 0/1.
    content = re.sub(r'(?i)DEFAULT\s+0(\s*[,)])', r'DEFAULT FALSE\1', content)
    content = re.sub(r'(?i)DEFAULT\s+1(\s*[,)])', r'DEFAULT TRUE\1', content)
    
    # Let's check for any `GETDATE()` left
    content = re.sub(r'(?i)\bGETDATE\(\)', 'CURRENT_TIMESTAMP', content)

    # Let's make sure NOCOUNT ON is removed, as it's not valid in Postgres
    content = re.sub(r'(?i)set\s+nocount\s+on;', '', content)
    
    # Let's make sure `IF NOT EXISTS` is not used in table creation if not necessary, but here it's fine.

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

fix_concat(r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\script_crm_postgres.sql')
try:
    fix_concat(r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\artificial_data_postgres.sql')
except:
    pass

