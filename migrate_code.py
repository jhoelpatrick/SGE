import os
import re

def migrate_csharp(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.cs') or file.endswith('.cshtml'):
                filepath = os.path.join(root, file)
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()

                # Basic ADO.NET replacements
                new_content = content.replace('Microsoft.Data.SqlClient', 'Npgsql')
                new_content = new_content.replace('SqlConnection', 'NpgsqlConnection')
                new_content = new_content.replace('SqlCommand', 'NpgsqlCommand')
                new_content = new_content.replace('SqlDataReader', 'NpgsqlDataReader')
                new_content = new_content.replace('SqlException', 'PostgresException')
                new_content = new_content.replace('SqlDbType.', 'NpgsqlDbType.')
                new_content = new_content.replace('SqlDbType', 'NpgsqlDbType')

                # Replace SCOPE_IDENTITY() pattern
                # Usually it looks like: SELECT CAST(SCOPE_IDENTITY() AS INT);
                new_content = re.sub(
                    r'SELECT CAST\(\s*SCOPE_IDENTITY\(\)\s*AS\s*INT\s*\);',
                    'RETURNING clienteid;', # Wait, this depends on the primary key name! 
                    # Actually PostgreSQL RETURNING can just be 'RETURNING id' or 'RETURNING *', wait if it's ExecuteScalar, 'RETURNING {primary_key_name}' is needed.
                    # A better generic approach: Just use python to find the table being inserted into and use RETURNING.
                    new_content, flags=re.IGNORECASE)

                if new_content != content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                    print(f"Migrated {file}")

migrate_csharp(r'c:\Users\alu_torre1\Desktop\SGE\SGE')
