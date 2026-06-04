import psycopg2

conn_str = "postgresql://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    cur.execute("SELECT COUNT(*) FROM comercial.clientes;")
    print(f"Total clients: {cur.fetchone()[0]}")
    
    cur.execute("SELECT COUNT(*) FROM finanzas.activosfijos;")
    print(f"Total fixed assets: {cur.fetchone()[0]}")
    
    cur.close()
    conn.close()
except Exception as ex:
    print(f"Error: {ex}")
