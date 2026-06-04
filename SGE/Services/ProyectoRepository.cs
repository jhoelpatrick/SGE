﻿﻿using Npgsql;
using SGE.Models;

namespace SGE.Services
{
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly string _connectionString;

        public ProyectoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<List<Proyecto>> GetAllAsync()
        {
            var lista = new List<Proyecto>();
            const string sql = @"
                SELECT proyectoid, nombreproyecto, clientenombre, clienteruc,
                       presupuestototal, costoreallogrado, fechainicio, fechafin,
                       estadoproyecto, progresopromedio, totaltareas
                FROM   operaciones.vw_operaciones_dashboard_proyectos
                ORDER BY proyectoid DESC";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new Proyecto
                {
                    ProyectoId = rd.GetInt32(0),
                    NombreProyecto = rd.GetString(1),
                    ClienteNombre = rd.IsDBNull(2) ? "" : rd.GetString(2),
                    ClienteRuc = rd.IsDBNull(3) ? "" : rd.GetString(3),
                    PresupuestoTotal = rd.GetDecimal(4),
                    CostoRealLogrado = rd.GetDecimal(5),
                    FechaInicio = rd.GetDateTime(6),
                    FechaFin = rd.IsDBNull(7) ? null : rd.GetDateTime(7),
                    Estado = rd.GetString(8),
                    ProgresoPromedio = rd.GetDecimal(9),
                    TotalTareas = rd.GetInt32(10)
                });
            }
            return lista;
        }

        public async Task<Proyecto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.proyectoid, p.nombreproyecto, c.razonsocial, c.numerodocumento,
                       p.presupuestototal, p.costoreallogrado, p.fechainicio, p.fechafin,
                       p.estado, p.descripcion, p.clienteid
                FROM   operaciones.proyectos p
                INNER JOIN comercial.clientes c ON p.clienteid = c.clienteid
                WHERE  p.proyectoid = @id";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rd = await cmd.ExecuteReaderAsync();

            if (await rd.ReadAsync())
            {
                return new Proyecto
                {
                    ProyectoId = rd.GetInt32(0),
                    NombreProyecto = rd.GetString(1),
                    ClienteNombre = rd.GetString(2),
                    ClienteRuc = rd.GetString(3),
                    PresupuestoTotal = rd.GetDecimal(4),
                    CostoRealLogrado = rd.GetDecimal(5),
                    FechaInicio = rd.GetDateTime(6),
                    FechaFin = rd.IsDBNull(7) ? null : rd.GetDateTime(7),
                    Estado = rd.GetString(8),
                    Descripcion = rd.IsDBNull(9) ? null : rd.GetString(9),
                    ClienteId = rd.GetInt32(10)
                };
            }
            return null;
        }

        public async Task<int> CreateAsync(Proyecto p)
        {
            const string sql = @"
                INSERT INTO operaciones.proyectos
                    (clienteid, nombreproyecto, descripcion, presupuestototal, costoreallogrado, fechainicio, fechafin, estado)
                VALUES
                    (@clienteid, @nombreproyecto, @descripcion, @presupuestototal, 0.0000, @fechainicio, @fechafin, @estado)
                RETURNING proyectoid;";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@clienteid", p.ClienteId);
            cmd.Parameters.AddWithValue("@nombreproyecto", p.NombreProyecto);
            cmd.Parameters.AddWithValue("@descripcion", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@presupuestototal", p.PresupuestoTotal);
            cmd.Parameters.AddWithValue("@fechainicio", p.FechaInicio);
            cmd.Parameters.AddWithValue("@fechafin", (object?)p.FechaFin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado", p.Estado);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<List<ProyectoTarea>> GetTareasByProyectoIdAsync(int proyectoId)
        {
            var lista = new List<ProyectoTarea>();
            const string sql = @"
                SELECT tareaid, proyectoid, nombretarea, fechainicio, fechafin, porcentajeprogreso, costoestimado, estado
                FROM   operaciones.proyectotareas
                WHERE  proyectoid = @proyectoId
                ORDER BY tareaid";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@proyectoId", proyectoId);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new ProyectoTarea
                {
                    TareaId = rd.GetInt32(0),
                    ProyectoId = rd.GetInt32(1),
                    NombreTarea = rd.GetString(2),
                    FechaInicio = rd.GetDateTime(3),
                    FechaFin = rd.GetDateTime(4),
                    PorcentajeProgreso = rd.GetDecimal(5),
                    CostoEstimado = rd.GetDecimal(6),
                    Estado = rd.GetString(7)
                });
            }
            return lista;
        }

        public async Task<int> CreateTareaAsync(ProyectoTarea t)
        {
            const string sql = @"
                INSERT INTO operaciones.proyectotareas
                    (proyectoid, nombretarea, fechainicio, fechafin, porcentajeprogreso, costoestimado, estado)
                VALUES
                    (@proyectoid, @nombretarea, @fechainicio, @fechafin, 0.00, @costoestimado, @estado)
                RETURNING tareaid;";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@proyectoid", t.ProyectoId);
            cmd.Parameters.AddWithValue("@nombretarea", t.NombreTarea);
            cmd.Parameters.AddWithValue("@fechainicio", t.FechaInicio);
            cmd.Parameters.AddWithValue("@fechafin", t.FechaFin);
            cmd.Parameters.AddWithValue("@costoestimado", t.CostoEstimado);
            cmd.Parameters.AddWithValue("@estado", t.Estado);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task UpdateTareaEstadoAsync(int tareaId, decimal progreso, string estado)
        {
            const string sql = @"
                UPDATE operaciones.proyectotareas
                SET    porcentajeprogreso = @progreso,
                       estado = @estado
                WHERE  tareaid = @tareaId";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@progreso", progreso);
            cmd.Parameters.AddWithValue("@estado", estado);
            cmd.Parameters.AddWithValue("@tareaId", tareaId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
