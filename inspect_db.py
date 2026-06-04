import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    # List tables in rrhh_recursos and rrhh_nomina
    cur.execute("""
        SELECT table_schema, table_name 
        FROM information_schema.tables 
        WHERE table_schema IN ('rrhh_recursos', 'rrhh_nomina') 
        ORDER BY table_schema, table_name;
    """)
    tables = cur.fetchall()
    print("=== TABLES FOUND ===")
    for s, t in tables:
        print(f"{s}.{t}")
        
    print("\n=== COLUMNS IN KEY TABLES ===")
    key_tables = [
        ("rrhh_recursos", "empleados"),
        ("rrhh_recursos", "contratos"),
        ("rrhh_recursos", "datos_laborales_empleados"),
        ("rrhh_nomina", "conceptos"),
        ("rrhh_nomina", "periodos_planillas"),
        ("rrhh_nomina", "planillas_cabeceras"),
        ("rrhh_nomina", "planillas_resumen"),
        ("rrhh_recursos", "feriados"),
        ("rrhh_recursos", "centros_costos")
    ]
    for schema, table in key_tables:
        cur.execute(f"""
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_schema = '{schema}' AND table_name = '{table}'
            ORDER BY ordinal_position;
        """)
        cols = cur.fetchall()
        print(f"\n{schema}.{table}:")
        if not cols:
            print("  (Does not exist)")
        for col, dtype, nullable in cols:
            print(f"  - {col} ({dtype}) - Nullable: {nullable}")
            
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
