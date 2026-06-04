import psycopg2

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

try:
    conn = psycopg2.connect(conn_str)
    cur = conn.cursor()
    
    tables = [
        "rrhh_recursos.administradoras_pensiones",
        "rrhh_recursos.regimenes_laborales",
        "rrhh_recursos.centros_costos",
        "rrhh_recursos.usuarios_nomina",
        "rrhh_recursos.ubigeos"
    ]
    
    for table in tables:
        print(f"\n=== Row count & sample for {table} ===")
        try:
            cur.execute(f"SELECT COUNT(*) FROM {table}")
            count = cur.fetchone()[0]
            print(f"Row count: {count}")
            if count > 0:
                cur.execute(f"SELECT * FROM {table} LIMIT 5")
                rows = cur.fetchall()
                for row in rows:
                    print("  ", row)
        except Exception as table_ex:
            print(f"Error reading {table}: {table_ex}")
            conn.rollback()
            
    cur.close()
    conn.close()
except Exception as e:
    print("Error:", e)
