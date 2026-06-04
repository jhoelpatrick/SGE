import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    cur.execute("SELECT usuario, nombrecompleto, correo, estaactivo FROM rrhh_recursos.usuarios_nomina;")
    print("Registered users in DB:")
    for row in cur.fetchall():
        print(f"  User: {row[0]}, Name: {row[1]}, Email: {row[2]}, Active: {row[3]}")
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
