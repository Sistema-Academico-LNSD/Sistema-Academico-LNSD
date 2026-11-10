using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Horario
{
    public class HorarioRepository : IHorarioRepository
    {
        private readonly AppDbContext _context;

        public HorarioRepository(AppDbContext context)
        {
            _context = context;
        }

        // Todo horario se devuelve con los datos que las pantallas necesitan mostrar
        private IQueryable<Entities.Horario> ConDetalle()
        {
            return _context.Horarios
                .AsNoTracking()
                .Include(h => h.Periodo)
                .Include(h => h.Seccion).ThenInclude(s => s!.Grado)
                .Include(h => h.Grupo)
                .Include(h => h.Curso)
                .Include(h => h.Docente);
        }

        // ---------- Consultas ----------

        public async Task<List<Entities.Horario>> Buscar(HorarioFiltro filtro)
        {
            var consulta = ConDetalle();

            // Los filtros se acumulan: solo quedan los horarios que cumplen todos
            if (filtro.IdPeriodo.HasValue)
                consulta = consulta.Where(h => h.IdPeriodo == filtro.IdPeriodo.Value);

            if (filtro.IdGrado.HasValue)
                consulta = consulta.Where(h => h.Seccion!.IdGrado == filtro.IdGrado.Value);

            if (filtro.IdSeccion.HasValue)
                consulta = consulta.Where(h => h.IdSeccion == filtro.IdSeccion.Value);

            if (filtro.IdGrupo.HasValue)
                consulta = consulta.Where(h => h.IdGrupo == filtro.IdGrupo.Value);

            if (filtro.IdCurso.HasValue)
                consulta = consulta.Where(h => h.IdCurso == filtro.IdCurso.Value);

            if (filtro.IdDocente.HasValue)
                consulta = consulta.Where(h => h.IdDocente == filtro.IdDocente.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Dia))
            {
                string dia = filtro.Dia.Trim();
                consulta = consulta.Where(h => h.Dia == dia);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Aula))
            {
                string aula = filtro.Aula.Trim();
                consulta = consulta.Where(h => h.Aula.Contains(aula));
            }

            if (filtro.Estado.HasValue)
                consulta = consulta.Where(h => h.Estado == filtro.Estado.Value);

            if (filtro.IdsCursos != null)
                consulta = consulta.Where(h => filtro.IdsCursos.Contains(h.IdCurso));

            // El orden por día de la semana lo aplica el servicio (el nombre del día no se ordena bien como texto)
            return await consulta
                .OrderBy(h => h.HoraInicio)
                .ThenBy(h => h.Seccion!.Grado!.Nombre)
                .ThenBy(h => h.Seccion!.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.Horario?> GetById(int idHorario)
        {
            return await ConDetalle()
                .FirstOrDefaultAsync(h => h.IdHorario == idHorario);
        }

        // ---------- Conflictos ----------
        // Dos bloques se traslapan si uno empieza antes de que el otro termine y termina
        // después de que el otro empiece: inicio < otroFin  Y  fin > otroInicio.
        // Así 8:00-9:00 y 9:00-10:00 NO chocan, pero 8:00-9:00 y 8:30-9:30 sí.

        public async Task<Entities.Horario?> BuscarConflictoDocente(
            int idPeriodo, int idDocente, string dia, TimeOnly inicio, TimeOnly fin, int? excluirId = null)
        {
            return await ConDetalle()
                .Where(h => h.IdPeriodo == idPeriodo &&
                            h.Estado &&
                            h.IdDocente == idDocente &&
                            h.Dia == dia &&
                            h.HoraInicio < fin &&
                            h.HoraFin > inicio &&
                            (!excluirId.HasValue || h.IdHorario != excluirId.Value))
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        public async Task<Entities.Horario?> BuscarConflictoGrupo(
            int idPeriodo, int idSeccion, int idGrupo, string dia, TimeOnly inicio, TimeOnly fin, int? excluirId = null)
        {
            // Una clase queda definida por la pareja sección + grupo, igual que la matrícula del estudiante
            return await ConDetalle()
                .Where(h => h.IdPeriodo == idPeriodo &&
                            h.Estado &&
                            h.IdSeccion == idSeccion &&
                            h.IdGrupo == idGrupo &&
                            h.Dia == dia &&
                            h.HoraInicio < fin &&
                            h.HoraFin > inicio &&
                            (!excluirId.HasValue || h.IdHorario != excluirId.Value))
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        public async Task<Entities.Horario?> BuscarConflictoAula(
            int idPeriodo, string aula, string dia, TimeOnly inicio, TimeOnly fin, int? excluirId = null)
        {
            return await ConDetalle()
                .Where(h => h.IdPeriodo == idPeriodo &&
                            h.Estado &&
                            h.Aula == aula &&
                            h.Dia == dia &&
                            h.HoraInicio < fin &&
                            h.HoraFin > inicio &&
                            (!excluirId.HasValue || h.IdHorario != excluirId.Value))
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        // ---------- Datos para validar y para los formularios ----------

        public async Task<Entities.Seccion?> GetSeccion(int idSeccion)
        {
            return await _context.Secciones
                .AsNoTracking()
                .Include(s => s.Grado)
                .FirstOrDefaultAsync(s => s.IdSeccion == idSeccion);
        }

        public async Task<Entities.Grupo?> GetGrupo(int idGrupo)
        {
            return await _context.Grupos
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo);
        }

        public async Task<bool> CursoImpartidoEnGrado(int idCurso, int idGrado)
        {
            return await _context.CursosGrados.AnyAsync(cg =>
                cg.IdCurso == idCurso &&
                cg.IdGrado == idGrado);
        }

        public async Task<List<Entities.Seccion>> GetSeccionesActivas()
        {
            return await _context.Secciones
                .AsNoTracking()
                .Include(s => s.Grado)
                .Where(s => s.Estado && s.Grado!.Estado)
                .OrderBy(s => s.Grado!.Nombre)
                .ThenBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Grupo>> GetGruposPorNivel(string nivel)
        {
            return await _context.Grupos
                .AsNoTracking()
                .Where(g => g.Nivel == nivel)
                .OrderBy(g => g.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Curso>> GetCursosActivosDeGrado(int idGrado)
        {
            return await _context.CursosGrados
                .AsNoTracking()
                .Where(cg => cg.IdGrado == idGrado && cg.Curso!.Estado)
                .Select(cg => cg.Curso!)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Docente>> GetDocentesDeCurso(int idCurso, int idPeriodo)
        {
            // Docentes activos que tienen el curso asignado en ese período
            return await _context.DocentesCursos
                .AsNoTracking()
                .Where(dc => dc.IdCurso == idCurso &&
                             dc.IdPeriodo == idPeriodo &&
                             dc.Docente!.Estado)
                .Select(dc => dc.Docente!)
                .OrderBy(d => d.Apellidos)
                .ThenBy(d => d.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.PeriodoLectivo>> GetPeriodos()
        {
            return await _context.PeriodosLectivos
                .AsNoTracking()
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        // ---------- Escritura ----------

        public async Task<bool> Crear(Entities.Horario horario)
        {
            if (horario == null)
                return false;

            try
            {
                await _context.Horarios.AddAsync(horario);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Un índice único de la base lo rechazó (por ejemplo, dos registros idénticos a la vez)
                _context.Entry(horario).State = EntityState.Detached;

                return false;
            }
        }

        public async Task<bool> Actualizar(Entities.Horario horario)
        {
            // Se carga el registro y se copian solo los campos editables. El período y el
            // estado no se tocan aquí: el estado tiene su propia operación.
            var actual = await _context.Horarios
                .FirstOrDefaultAsync(h => h.IdHorario == horario.IdHorario);

            if (actual == null)
                return false;

            actual.IdSeccion = horario.IdSeccion;
            actual.IdGrupo = horario.IdGrupo;
            actual.IdCurso = horario.IdCurso;
            actual.IdDocente = horario.IdDocente;
            actual.Dia = horario.Dia;
            actual.HoraInicio = horario.HoraInicio;
            actual.HoraFin = horario.HoraFin;
            actual.Aula = horario.Aula;

            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException)
            {
                _context.Entry(actual).State = EntityState.Detached;

                return false;
            }
        }

        public async Task<bool> CambiarEstado(int idHorario, bool estado)
        {
            int filas = await _context.Horarios
                .Where(h => h.IdHorario == idHorario)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(h => h.Estado, estado));

            return filas > 0;
        }
    }
}