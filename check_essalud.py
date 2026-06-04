import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    cur.execute("SELECT table_name FROM information_schema.tables WHERE table_schema = 'rrhh_nomina'")
    tables = cur.fetchall()
    print("Tables in rrhh_nomina:")
    for t in tables:
        print(" -", t[0])
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
