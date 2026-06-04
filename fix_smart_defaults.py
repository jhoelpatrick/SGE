"""
Smart fix: for each column definition in the CREATE TABLE block,
check if the column type is BOOLEAN -> use TRUE/FALSE as default
If the column type is INT/DECIMAL/NUMERIC -> use 0 as default
"""
import re

filepath = r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\script_crm_postgres.sql'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

def fix_line_default(line):
    # We only touch lines that have DEFAULT TRUE or DEFAULT FALSE
    # and also numeric types
    upper = line.upper()
    
    has_default_true  = 'DEFAULT TRUE'  in upper
    has_default_false = 'DEFAULT FALSE' in upper
    
    if not has_default_true and not has_default_false:
        return line
    
    # Determine the column type
    is_boolean = bool(re.search(r'\bBOOLEAN\b', upper))
    is_numeric = bool(re.search(r'\b(INT|INTEGER|BIGINT|SMALLINT|DECIMAL|NUMERIC|FLOAT|REAL)\b', upper))
    
    if is_boolean:
        # Keep TRUE/FALSE - this is correct already
        return line
    elif is_numeric:
        # Replace TRUE -> 1, FALSE -> 0
        line = re.sub(r'\bDEFAULT\s+TRUE\b', 'DEFAULT 1', line, flags=re.IGNORECASE)
        line = re.sub(r'\bDEFAULT\s+FALSE\b', 'DEFAULT 0', line, flags=re.IGNORECASE)
        return line
    else:
        # Unknown type — keep as-is (likely a varchar or char with a boolean default, which is probably wrong but let's not break it)
        return line

fixed_lines = [fix_line_default(l) for l in content.splitlines(keepends=True)]
fixed_content = ''.join(fixed_lines)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(fixed_content)

print("Done. Verifying residual issues...")

# Print any remaining suspect lines (BOOLEAN with DEFAULT 0 or DEFAULT 1)
for i, line in enumerate(fixed_content.splitlines(), 1):
    u = line.upper()
    if 'BOOLEAN' in u and ('DEFAULT 0' in u or 'DEFAULT 1' in u):
        print(f"  Line {i} [BOOLEAN+INT_DEFAULT]: {line.strip()}")
    if ('INT' in u or 'NUMERIC' in u or 'DECIMAL' in u) and ('DEFAULT TRUE' in u or 'DEFAULT FALSE' in u):
        print(f"  Line {i} [NUMERIC+BOOL_DEFAULT]: {line.strip()}")

print("Verification complete.")
