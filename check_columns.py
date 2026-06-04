import psycopg2

conn_str = "postgresql://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

def check_table(schema, table):
    try:
        conn = psycopg2.connect(conn_str)
        cur = conn.cursor()
        cur.execute(f"""
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_schema = '{schema}' AND table_name = '{table}';
        """)
        print(f"\nColumns in {schema}.{table}:")
        cols = cur.fetchall()
        if not cols:
            print("  Table not found or has no columns.")
        for row in cols:
            print(f"  {row[0]} ({row[1]}) - Nullable: {row[2]}")
        cur.close()
        conn.close()
    except Exception as ex:
        print(f"Error checking {schema}.{table}: {ex}")

check_table("rrhh_nomina", "conceptos")
check_table("rrhh_recursos", "empleados")
check_table("rrhh_nomina", "periodos_planillas")
check_table("rrhh_nomina", "planillas_cabeceras")
check_table("rrhh_nomina", "planillas_resumen")
