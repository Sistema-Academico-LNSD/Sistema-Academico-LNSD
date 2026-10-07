using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.DocenteCurso
{
    public class DocenteCursoRepository : IDocenteCursoRepository
    {
        private readonly AppDbContext _context;
        public DocenteCursoRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Entities.DocenteCurso>> GetAsignaciones(int idDocente)
        {
            return await _context.DocentesCursos
                .AsNoTracking()
                .Include(dc => dc.Curso!).ThenInclude(c => c.Area)
                .Include(dc => dc.Periodo)
                .Where(dc => dc.IdDocente == idDocente)
                .OrderByDescending(dc => dc.Periodo!.FechaInicio)
                .ThenBy(dc => dc.Curso!.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.DocenteCurso?> GetById(int idDocenteCurso)
        {
            return await _context.DocentesCursos
                .AsNoTracking()
                .Include(dc => dc.Periodo)
                .FirstOrDefaultAsync(dc => dc.IdDocenteCurso == idDocenteCurso);
        }
        public async Task<bool> Existe(int idDocente, int idCurso, int idPeriodo)
        {
            return await _context.DocentesCursos.AnyAsync(dc =>
                dc.IdDocente == idDocente &&
                dc.IdCurso == idCurso &&
                dc.IdPeriodo == idPeriodo);
        }

        public async Task<List<Entities.Curso>> GetCursosDisponibles(int idDocente, int idPeriodo)
        {
            return await _context.Cursos
                .AsNoTracking()
                .Include(c => c.Area)
                .Where(c =>
                    c.Estado &&
                    !_context.DocentesCursos.Any(dc =>
                        dc.IdDocente == idDocente &&
                        dc.IdCurso == c.IdCurso &&
                        dc.IdPeriodo == idPeriodo))
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<bool> Crear(Entities.DocenteCurso asignacion)
        {
            asignacion.FechaAsignacion = DateTime.UtcNow;

            await _context.DocentesCursos.AddAsync(asignacion);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int idDocenteCurso)
        {
            var existente = await _context.DocentesCursos.FindAsync(idDocenteCurso);

            if (existente == null)
                return false;

            _context.DocentesCursos.Remove(existente);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}