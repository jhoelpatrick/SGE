import os
import re

def translate_sql(filepath, out_filepath):
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # Basic replacements
    content = re.sub(r'(?i)\bGO\b', '', content)
    content = re.sub(r'(?i)USE master;', '', content)
    content = re.sub(r'(?i)USE sge_crm;', '', content)
    content = re.sub(r'(?i)alter database .*? set single_user with rollback immediate;', '', content)
    content = re.sub(r'(?i)drop database sge_crm;', '', content)
    content = re.sub(r'(?i)create database sge_crm;', '', content)
    content = re.sub(r'(?i)alter database .*? set read_committed_snapshot on;', '', content)
    content = re.sub(r'(?i)if exists \(select name from sys\.databases where name = \'sge_crm\'\)', '', content)
    content = re.sub(r'(?i)begin\s*end', '', content)

    # DataTypes
    content = re.sub(r'(?i)\bDATETIME2?\b', 'TIMESTAMP', content)
    content = re.sub(r'(?i)\bBIT\b', 'BOOLEAN', content)
    content = re.sub(r'(?i)VARCHAR\(MAX\)', 'TEXT', content)

    # Default values for BIT
    content = re.sub(r'(?i)DEFAULT 1(\s*,|\s*\)|\s*$|\s*--)', r'DEFAULT TRUE\1', content)
    content = re.sub(r'(?i)DEFAULT 0(\s*,|\s*\)|\s*$|\s*--)', r'DEFAULT FALSE\1', content)

    # Functions
    content = re.sub(r'(?i)\bGETDATE\(\)', 'CURRENT_TIMESTAMP', content)
    content = re.sub(r'(?i)\bISNULL\(', 'COALESCE(', content)

    # IDENTITY
    content = re.sub(r'(?i)IDENTITY\(\s*1\s*,\s*1\s*\)', 'GENERATED ALWAYS AS IDENTITY', content)

    # CREATE OR ALTER VIEW -> CREATE OR REPLACE VIEW
    content = re.sub(r'(?i)CREATE\s+OR\s+ALTER\s+VIEW', 'CREATE OR REPLACE VIEW', content)
    
    # Indexes with INCLUDE - Postgres doesn't strictly use INCLUDE this way but supports it since v11
    # leave as INCLUDE.
    
    # Nonclustered index
    content = re.sub(r'(?i)CREATE\s+NONCLUSTERED\s+INDEX', 'CREATE INDEX', content)

    # Some procedures use CREATE OR ALTER PROCEDURE, replace with CREATE OR REPLACE PROCEDURE 
    # But note: Postgres procedures/functions body syntax is different (LANGUAGE plpgsql AS $$ BEGIN ... END; $$)
    # The script has very few procedures and they are commented out mostly, or we can just leave them and warn the user.
    # Let's do a basic conversion for PROCEDURE
    content = re.sub(r'(?i)CREATE\s+OR\s+ALTER\s+PROCEDURE', 'CREATE OR REPLACE PROCEDURE', content)
    
    # Variables @ -> arg_
    # This is too complex for simple regex. We will let the user know they need to adjust procedures manually if they uncomment them.

    # Fix schema creation syntax (CREATE SCHEMA IF NOT EXISTS)
    content = re.sub(r'(?i)create schema (\w+);', r'CREATE SCHEMA IF NOT EXISTS \1;', content)

    with open(out_filepath, 'w', encoding='utf-8') as f:
        f.write(content)

translate_sql(r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\script_crm.sql', r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\script_crm_postgres.sql')
translate_sql(r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\artificial_data.sql', r'c:\Users\alu_torre1\Desktop\SGE\SGE\SQL\artificial_data_postgres.sql')

