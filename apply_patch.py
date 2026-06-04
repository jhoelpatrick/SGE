import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    print("Connecting to Supabase...")
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    print("Reading patch_db.sql...")
    with open("SGE/SQL/patch_db.sql", "r", encoding="utf-8") as f:
        sql = f.read()
        
    print("Executing SQL script...")
    cur.execute(sql)
    conn.commit()
    print("Schema updates and seed data applied successfully!")
    
    cur.close()
    conn.close()
except Exception as e:
    print("Error applying schema updates:", e)
