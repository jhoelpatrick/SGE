using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using SGE.Models;

namespace SGE.Repositories
{
    public class CompaniaRepository
    {
        // Aquí usarías la cadena de conexión de tu clase SQL.ConexionSql
        private readonly string _connectionString;

        public CompaniaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // SELECT - Obtiene los datos exactos para el formulario y el panel derecho
        public async Task<ModelCompania> ObtenerCompaniaActivaAsync()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                const string sql = @"
                    SELECT 
                        c.id_comp,
                        c.razon_social,
                        c.RUC,
                        c.Direc_Fiscal,
                        c.Telef,
                        c.Celular,
                        c.Correo,
                        c.Sitio_web,
                        c.logo,
                        c.id_pais,
                        c.id_idioma,
                        c.zona_horaria,
                        p.descrip AS Pais,
                        i.descrip AS Idioma
                    FROM seguridad.compañia c
                    INNER JOIN seguridad.pais p ON c.id_pais = p.id_pais
                    INNER JOIN seguridad.idioma i ON c.id_idioma = i.id_idioma
                    WHERE c.estado = 'A';";

                return await db.QueryFirstOrDefaultAsync<ModelCompania>(sql);
            }
        }

        // INSERT / UPDATE (Con fec_act automática si ya existe)
        public async Task<bool> GuardarCompaniaAsync(ModelCompania compañia)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                if (compañia.id_comp == 0)
                {
                    // Solo INSERT si es un registro nuevo
                    const string sqlInsert = @"
                        INSERT INTO seguridad.compañia (
                            razon_social, RUC, Direc_Fiscal, Telef, Celular, 
                            Correo, Sitio_web, logo, id_pais, id_idioma, 
                            zona_horaria, fec_crea, estado
                        ) 
                        VALUES (
                            @razon_social, @RUC, @Direc_Fiscal, @Telef, @Celular, 
                            @Correo, @Sitio_web, @logo, @id_pais, @id_idioma, 
                            @zona_horaria, GETDATE(), 'A');";

                    int rows = await db.ExecuteAsync(sqlInsert, compañia);
                    return rows > 0;
                }
                else
                {
                    // UPDATE si ya existe, agregando tu condición de fec_act
                    const string sqlUpdate = @"
                        UPDATE seguridad.compañia
                        SET razon_social = @razon_social,
                            RUC = @RUC,
                            Direc_Fiscal = @Direc_Fiscal,
                            Telef = @Telef,
                            Celular = @Celular,
                            Correo = @Correo,
                            Sitio_web = @Sitio_web,
                            logo = @logo,
                            id_pais = @id_pais,
                            id_idioma = @id_idioma,
                            zona_horaria = @zona_horaria,
                            fec_act = GETDATE()
                        WHERE id_comp = @id_comp AND estado = 'A';";

                    int rows = await db.ExecuteAsync(sqlUpdate, compañia);
                    return rows > 0;
                }
            }
        }
    }
}