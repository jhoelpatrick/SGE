import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    cur.execute("SELECT usuarionominaid, correo, nombrecompleto, estaactivo, otpcode FROM rrhh_recursos.usuarios_nomina")
    rows = cur.fetchall()
    print("Registered users inusuarios_nomina:")
    for r in rows:
        print(f" - Email: {r[1]} | Nombre: {r[2]} | Activo: {r[3]} | Last OTP: {r[4]}")
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
