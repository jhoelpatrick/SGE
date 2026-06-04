import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    print("Connecting to DB...")
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    print("\n1. Testing: SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = TRUE")
    cur.execute("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = TRUE")
    print("Result:", cur.fetchone())
    
    print("\n2. Testing: SELECT COUNT(*) FROM rrhh_recursos.empleados")
    cur.execute("SELECT COUNT(*) FROM rrhh_recursos.empleados")
    print("Result:", cur.fetchone())
    
    print("\n3. Testing: SELECT SUM(sueldobase) FROM rrhh_recursos.contratos WHERE estaactivo = TRUE")
    cur.execute("SELECT SUM(sueldobase) FROM rrhh_recursos.contratos WHERE estaactivo = TRUE")
    print("Result:", cur.fetchone())
    
    print("\n4. Testing: SELECT COUNT(DISTINCT pv.empleadoid) ...")
    cur.execute("""
        SELECT COUNT(DISTINCT pv.empleadoid)
        FROM rrhh_recursos.periodos_vacacionales pv
        JOIN rrhh_recursos.programacion_vacaciones pv2 ON pv.periodovacacionalid = pv2.periodovacacionalid
        WHERE pv2.estadosolicitud = 'aprobada' AND CURRENT_DATE BETWEEN pv2.fechainicio AND pv2.fechafin
    """)
    print("Result:", cur.fetchone())

    print("\n5. Testing employee preview query:")
    cur.execute("""
        SELECT
            e.empleadoid,
            e.nombres,
            e.apellidopaterno,
            e.apellidomaterno,
            e.numerodocumento,
            COALESCE(cc.nombre, 'Sin cargo') AS cargo,
            COALESCE(c.sueldobase, 0) AS sueldobase,
            COALESCE(c.tipocontrato, 'Indefinido') AS tipocontrato,
            COALESCE(ap.nombre, 'ONP') AS sistemaprevisional,
            CASE WHEN e.estaactivo = TRUE THEN 0 ELSE 3 END AS estado
        FROM rrhh_recursos.empleados e
        LEFT JOIN rrhh_recursos.centros_costos cc
            ON e.centrocostoid = cc.centrocostoid
        LEFT JOIN rrhh_recursos.contratos c
            ON c.empleadoid = e.empleadoid AND c.estaactivo = TRUE
        LEFT JOIN rrhh_recursos.datos_laborales_empleados dle
            ON dle.empleadoid = e.empleadoid
        LEFT JOIN rrhh_recursos.administradoras_pensiones ap
            ON ap.afpid = dle.afpid
        ORDER BY e.empleadoid DESC
        LIMIT 10
    """)
    print("Result row 1:", cur.fetchone())

    cur.close()
    conn.close()
    print("\nAll queries succeeded!")
except Exception as e:
    print("\nERROR OCCURRED:", e)
