using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Asistencia
{
    public class AsistenciaRepository : IAsistenciaRepository
    {
        private const string MatriculaActiva = "Activa";

        private readonly AppDbContext _context;

        public AsistenciaRepository(AppDbContext context)
        {
            _context = context;
        }

        // ---------- Lista de clase ----------

        public async Task<List<Entities.Matricula>> GetMatriculasDeClase(
            int idPeriodo, int idSeccion, int idGrupo, int idCurso)
        {
            return await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Estudiante).ThenInclude(e => e!.Usuario)
                .Where(m => m.IdPeriodo == idPeriodo &&
                            m.IdSeccion == idSeccion &&
                            m.IdGrupo == idGrupo &&
                            m.Estado == MatriculaActiva &&
                            m.MatriculasCursos.Any(mc => mc.IdCurso == idCurso))
                .OrderBy(m => m.Estudiante!.Usuario!.Apellido)
                .ThenBy(m => m.Estudiante!.Usuario!.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Asistencia>> GetDeSesion(int idHorario, DateOnly fecha)
        {
            return await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.IdHorario == idHorario && a.Fecha == fecha)
                .ToListAsync();
        }

        // ---------- Escritura ----------

        public async Task<(int Nuevos, int Modificados)> GuardarSesion(
            int idHorario, DateOnly fecha, IReadOnlyList<Entities.Asistencia> registros, int idUsuario, DateTime ahora)
        {
            // Se cargan con seguimiento para que EF actualice solo lo que cambió
            var existentes = await _context.Asistencias
                .Where(a => a.IdHorario == idHorario && a.Fecha == fecha)
                .ToDictionaryAsync(a => a.IdMatricula);

            int nuevos = 0;
            int modificados = 0;

            foreach (var r in registros)
            {
                if (existentes.TryGetValue(r.IdMatricula, out var actual))
                {
                    if (actual.Estado == r.Estado && actual.Observacion == r.Observacion)
                        continue;

                    actual.Estado = r.Estado;
                    actual.Observacion = r.Observacion;
                    actual.IdUsuarioModifica = idUsuario;
                    actual.FechaModificacion = ahora;
                    modificados++;
                }
                else
                {
                    _context.Asistencias.Add(new Entities.Asistencia
                    {
                        IdHorario = idHorario,
                        IdMatricula = r.IdMatricula,
                        Fecha = fecha,
                        Estado = r.Estado,
                        Observacion = r.Observacion,
                        IdUsuarioRegistra = idUsuario,
                        FechaRegistro = ahora
                    });
                    nuevos++;
                }
            }

            // Un solo SaveChanges: se guarda toda la lista o nada
            if (nuevos + modificados > 0)
                await _context.SaveChangesAsync();

            return (nuevos, modificados);
        }

        // ---------- Consultas ----------

        public async Task<List<Entities.Asistencia>> Buscar(AsistenciaFiltro filtro, int limite)
        {
            return await Filtrar(filtro)
                .Include(a => a.Horario).ThenInclude(h => h!.Curso)
                .Include(a => a.Horario).ThenInclude(h => h!.Docente)
                .Include(a => a.Horario).ThenInclude(h => h!.Grupo)
                .Include(a => a.Horario).ThenInclude(h => h!.Periodo)
                .Include(a => a.Horario).ThenInclude(h => h!.Seccion).ThenInclude(s => s!.Grado)
                .Include(a => a.Matricula).ThenInclude(m => m!.Estudiante).ThenInclude(e => e!.Usuario)
                .OrderByDescending(a => a.Fecha)
                .ThenBy(a => a.Horario!.HoraInicio)
                .ThenBy(a => a.Matricula!.Estudiante!.Usuario!.Apellido)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<List<ConteoAsistencia>> Contar(AsistenciaFiltro filtro)
        {
            // GROUP BY en SQL Server: no se traen a memoria todos los registros del período
            return await Filtrar(filtro)
                .GroupBy(a => new { a.IdMatricula, a.Horario!.IdCurso })
                .Select(g => new ConteoAsistencia
                {
                    IdMatricula = g.Key.IdMatricula,
                    IdCurso = g.Key.IdCurso,
                    Presentes = g.Count(a => a.Estado == EstadoAsistencia.Presente),
                    Ausentes = g.Count(a => a.Estado == EstadoAsistencia.Ausente),
                    Tardias = g.Count(a => a.Estado == EstadoAsistencia.Tardia),
                    Justificadas = g.Count(a => a.Estado == EstadoAsistencia.Justificada)
                })
                .ToListAsync();
        }

        public async Task<List<Entities.Matricula>> GetMatriculas(IEnumerable<int> idsMatriculas)
        {
            var ids = idsMatriculas.Distinct().ToList();

            return await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Estudiante).ThenInclude(e => e!.Usuario)
                .Include(m => m.Seccion).ThenInclude(s => s!.Grado)
                .Include(m => m.Grupo)
                .Include(m => m.PeriodoLectivo)
                .Where(m => ids.Contains(m.IdMatricula))
                .ToListAsync();
        }

        public async Task<List<Entities.Curso>> GetCursos(IEnumerable<int> idsCursos)
        {
            var ids = idsCursos.Distinct().ToList();

            return await _context.Cursos
                .AsNoTracking()
                .Where(c => ids.Contains(c.IdCurso))
                .ToListAsync();
        }

        // ---------- Cierre ----------

        public async Task<Entities.AsistenciaCierre?> GetCierre(int idPeriodo)
        {
            return await _context.AsistenciasCierres
                .AsNoTracking()
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.IdPeriodo == idPeriodo);
        }

        public async Task<bool> Cerrar(Entities.AsistenciaCierre cierre)
        {
            if (await _context.AsistenciasCierres.AnyAsync(c => c.IdPeriodo == cierre.IdPeriodo))
                return false;

            _context.AsistenciasCierres.Add(cierre);
            await _context.SaveChangesAsync();

            return true;
        }

        // ---------- Auxiliares ----------

        private IQueryable<Entities.Asistencia> Filtrar(AsistenciaFiltro f)
        {
            var q = _context.Asistencias.AsNoTracking().AsQueryable();

            if (f.IdPeriodo.HasValue)
                q = q.Where(a => a.Horario!.IdPeriodo == f.IdPeriodo.Value);

            if (f.IdGrado.HasValue)
                q = q.Where(a => a.Horario!.Seccion!.IdGrado == f.IdGrado.Value);

            if (f.IdSeccion.HasValue)
                q = q.Where(a => a.Horario!.IdSeccion == f.IdSeccion.Value);

            if (f.IdGrupo.HasValue)
                q = q.Where(a => a.Horario!.IdGrupo == f.IdGrupo.Value);

            if (f.IdCurso.HasValue)
                q = q.Where(a => a.Horario!.IdCurso == f.IdCurso.Value);

            if (f.IdDocente.HasValue)
                q = q.Where(a => a.Horario!.IdDocente == f.IdDocente.Value);

            if (f.IdMatricula.HasValue)
                q = q.Where(a => a.IdMatricula == f.IdMatricula.Value);

            if (f.IdsMatriculas != null)
                q = q.Where(a => f.IdsMatriculas.Contains(a.IdMatricula));

            if (f.Desde.HasValue)
                q = q.Where(a => a.Fecha >= f.Desde.Value);

            if (f.Hasta.HasValue)
                q = q.Where(a => a.Fecha <= f.Hasta.Value);

            if (!string.IsNullOrWhiteSpace(f.Estado))
                q = q.Where(a => a.Estado == f.Estado);

            if (f.SoloIncidencias)
                q = q.Where(a => a.Estado != EstadoAsistencia.Presente);

            if (!string.IsNullOrWhiteSpace(f.Texto))
            {
                string texto = f.Texto.Trim();

                q = q.Where(a =>
                    a.Matricula!.Estudiante!.Usuario!.Nombre.Contains(texto) ||
                    a.Matricula!.Estudiante!.Usuario!.Apellido.Contains(texto) ||
                    a.Matricula!.Estudiante!.Carnet.Contains(texto) ||
                    a.Matricula!.Estudiante!.Identificacion.Contains(texto));
            }

            return q;
        }
    }
}
