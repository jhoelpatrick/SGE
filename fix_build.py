import os
import re

def fix_build_errors(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.cs'):
                filepath = os.path.join(root, file)
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()

                new_content = content
                # Fix NpgsqlTransaction
                new_content = new_content.replace('SqlTransaction', 'NpgsqlTransaction')

                # Fix PostgresException error code check
                # ex.Number == 2627 || ex.Number == 2601 is SQL Server for unique violation
                # In Postgres, unique violation is "23505"
                new_content = re.sub(
                    r'ex\.Number\s*==\s*2627\s*\|\|\s*ex\.Number\s*==\s*2601',
                    'ex.SqlState == "23505"', new_content)
                new_content = re.sub(
                    r'ex\.Number\s*==\s*547',
                    'ex.SqlState == "23503"', new_content) # Foreign key violation
                
                # Any other ex.Number
                new_content = re.sub(
                    r'ex\.Number\s*==\s*\d+',
                    'ex.SqlState == "23505"', new_content)

                if new_content != content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                    print(f"Fixed {file}")

fix_build_errors(r'c:\Users\alu_torre1\Desktop\SGE\SGE')
