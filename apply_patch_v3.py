import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    with open("SGE/SQL/patch_db_v3.sql", "r", encoding="utf-8") as f:
        sql = f.read()
        
    cur.execute(sql)
    conn.commit()
    print("Database patch applied successfully!")
    
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
