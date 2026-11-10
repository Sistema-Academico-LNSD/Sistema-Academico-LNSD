using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Calificacion
{
    public class CalificacionRepository : ICalificacionRepository
    {
        private readonly AppDbContext _context;

        public CalificacionRepository(AppDbContext context)
        {
            _context = context;
        }

        // ---------- Clases del docente ----------

        public async Task<List<Entities.Horario>> GetClasesActivasDeDocente(int idDocente, int idPeriodo)
        {
            return await _context.Horarios
                .AsNoTracking()
                .Include(h => h.Curso)
                .Include(h => h.Seccion).ThenInclude(s => s!.Grado)
                .Include(h => h.Grupo)
                .Where(h => h.IdDocente == idDocente
                         && h.IdPeriodo == idPeriodo
                         && h.Estado
                         && _context.DocentesCursos.Any(dc => dc.IdDocente == h.IdDocente
                                                           && dc.IdCurso == h.IdCurso
                                                           && dc.IdPeriodo == h.IdPeriodo))
                .ToListAsync();
        }

        public async Task<bool> DocenteImparteClase(int idDocente, int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            return await _context.Horarios.AnyAsync(h => h.IdDocente == idDocente
                         && h.IdPeriodo == idPeriodo
                         && h.IdCurso == idCurso
                         && h.IdSeccion == idSeccion
                         && h.IdGrupo == idGrupo
                         && h.Estado
                         && _context.DocentesCursos.Any(dc => dc.IdDocente == h.IdDocente
                                                           && dc.IdCurso == h.IdCurso
                                                           && dc.IdPeriodo == h.IdPeriodo));
        }

        // ---------- Planilla ----------

        public async Task<List<Entities.Matricula>> GetMatriculasDeClase(int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            return await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Estudiante).ThenInclude(e => e!.Usuario)
                .Where(m => m.IdPeriodo == idPeriodo
                         && m.IdSeccion == idSeccion
                         && m.IdGrupo == idGrupo
                         && m.Estado == "Activa"
                         && m.MatriculasCursos.Any(mc => mc.IdCurso == idCurso))
                .OrderBy(m => m.Estudiante!.Usuario!.Apellido)
                .ThenBy(m => m.Estudiante!.Usuario!.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.RubroEvaluacion>> GetRubros()
        {
            return await _context.RubrosEvaluacion
                .AsNoTracking()
                .Where(r => r.Estado)
                .OrderBy(r => r.Orden)
                .ToListAsync();
        }

        public async Task<List<Entities.ActividadEvaluacion>> GetActividadesDeClase(
            int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            return await _context.ActividadesEvaluacion
                .AsNoTracking()
                .Where(a => a.Estado
                         && a.IdPeriodo == idPeriodo
                         && a.IdCurso == idCurso
                         && a.IdSeccion == idSeccion
                         && a.IdGrupo == idGrupo)
                .OrderBy(a => a.Fecha == null)
                .ThenBy(a => a.Fecha)
                .ThenBy(a => a.IdActividad)
                .ToListAsync();
        }

        public async Task<List<Entities.Nota>> GetNotasDeActividades(List<int> idsActividad)
        {
            return await _context.Notas
                .AsNoTracking()
                .Where(n => idsActividad.Contains(n.IdActividad))
                .ToListAsync();
        }

        // ---------- Actividades ----------

        public async Task<Entities.RubroEvaluacion?> GetRubro(int idRubro)
        {
            return await _context.RubrosEvaluacion
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRubro == idRubro);
        }

        public async Task<bool> ExisteActividadConNombre(
            int idPeriodo, int idCurso, int idSeccion, int idGrupo, int idRubro, string nombre)
        {
            return await _context.ActividadesEvaluacion.AnyAsync(a => a.Estado
                         && a.IdPeriodo == idPeriodo
                         && a.IdCurso == idCurso
                         && a.IdSeccion == idSeccion
                         && a.IdGrupo == idGrupo
                         && a.IdRubro == idRubro
                         && a.Nombre == nombre);
        }

        public async Task<Entities.ActividadEvaluacion> CrearActividad(Entities.ActividadEvaluacion actividad)
        {
            _context.ActividadesEvaluacion.Add(actividad);
            await _context.SaveChangesAsync();

            return actividad;
        }

        public async Task<Entities.ActividadEvaluacion?> GetActividad(int idActividad)
        {
            return await _context.ActividadesEvaluacion
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdActividad == idActividad);
        }

        // ---------- Notas ----------

        public async Task<Entities.Matricula?> GetMatricula(int idMatricula)
        {
            return await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.MatriculasCursos)
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula);
        }

        public async Task<Entities.Nota?> GetNota(int idMatricula, int idActividad)
        {
            return await _context.Notas
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.IdMatricula == idMatricula && n.IdActividad == idActividad);
        }

        public async Task<bool> CrearNota(Entities.Nota nota, Entities.NotaHistorial historial)
        {
            // La nota y su historial se guardan juntos: o quedan los dos o ninguno
            historial.Nota = nota;

            _context.Notas.Add(nota);
            _context.NotasHistorial.Add(historial);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
