import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    cur.execute("SELECT COUNT(*) FROM comercial.clientes")
    print("comercial.clientes row count:", cur.fetchone()[0])
    
    cur.execute("SELECT COUNT(*) FROM comercial.vw_crm_clientes_bandeja")
    print("comercial.vw_crm_clientes_bandeja row count:", cur.fetchone()[0])
    
    cur.execute("SELECT COUNT(*) FROM comercial.proveedores")
    print("comercial.proveedores row count:", cur.fetchone()[0])
    
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
