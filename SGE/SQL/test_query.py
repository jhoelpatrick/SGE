import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    queries = [
        "SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones",
        "SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Pendiente'",
        "SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Enviada'",
        "SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Aceptada'",
        "SELECT SUM(aporteessalud) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Aceptada'",
        "SELECT declaracionid, codigo, periodo, trabajadores, remuneracionasignable, aporteessalud, fechaenvio, estado, nroordensunat, observacion, subsidios, totalpagar, tipo FROM rrhh_nomina.essalud_declaraciones",
        "SELECT empleadoid, nombres, apellidopaterno, apellidomaterno, numerodocumento, COALESCE(cargo, 'Colaborador') AS cargo, COALESCE(departamento, 'Administración') AS departamento, estaactivo FROM rrhh_recursos.empleados"
    ]
    
    for i, q in enumerate(queries, 1):
        print(f"Executing query {i}: {q}")
        cur.execute(q)
        res = cur.fetchall()
        print(f"Result {i}: {res}")
        
    cur.close()
    conn.close()
except Exception as e:
    print("Database Query Error:", e)
