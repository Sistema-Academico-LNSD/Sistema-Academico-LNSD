using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Curso
{
    public class CursoRepository : ICursoRepository
    {
        private readonly AppDbContext _context;

        public CursoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Curso>> GetCursos(
            string? texto,
            int? idGrado,
            int? idArea,
            bool? estado)
        {
            IQueryable<Entities.Curso> consulta = _context.Cursos
                .Include(c => c.Area)
                .Include(c => c.CursosGrados)
                    .ThenInclude(cg => cg.Grado);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                string busqueda = texto.Trim().ToLower();

                consulta = consulta.Where(c =>
                    c.Nombre.ToLower().Contains(busqueda) ||
                    c.Codigo.ToLower().Contains(busqueda));
            }

            if (idGrado.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.CursosGrados.Any(cg =>
                        cg.IdGrado == idGrado.Value));
            }

            if (idArea.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.IdArea == idArea.Value);
            }

            if (estado.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.Estado == estado.Value);
            }

            return await consulta
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.AreaAcademica>> GetAreasActivas()
        {
            return await _context.AreasAcademicas
                .Where(a => a.Estado)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Grado>> GetGradosActivos()
        {
            return await _context.Grados
                .Where(g => g.Estado)
                .OrderBy(g => g.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.Curso?> GetCursoById(int id)
        {
            return await _context.Cursos
                .Include(c => c.Area)
                .Include(c => c.CursosGrados)
                    .ThenInclude(cg => cg.Grado)
                .FirstOrDefaultAsync(c => c.IdCurso == id);
        }

        public async Task<Entities.Curso?> GetCursoDetalle(int id)
        {
            return await _context.Cursos
                .Include(c => c.Area)
                .Include(c => c.CursosGrados)
                    .ThenInclude(cg => cg.Grado)
                .FirstOrDefaultAsync(c => c.IdCurso == id);
        }

        public async Task<bool> CreateCurso(
            Entities.Curso curso,
            List<int> idsGrados)
        {
            if (curso == null)
                return false;

            await _context.Cursos.AddAsync(curso);
            await _context.SaveChangesAsync();

            if (idsGrados != null && idsGrados.Count > 0)
            {
                List<Entities.CursoGrado> relaciones = idsGrados
                    .Distinct()
                    .Select(idGrado => new Entities.CursoGrado
                    {
                        IdCurso = curso.IdCurso,
                        IdGrado = idGrado
                    })
                    .ToList();

                await _context.CursosGrados.AddRangeAsync(relaciones);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpdateCurso(
            Entities.Curso curso,
            List<int> idsGrados)
        {
            Entities.Curso? existente = await _context.Cursos
                .Include(c => c.CursosGrados)
                .FirstOrDefaultAsync(c => c.IdCurso == curso.IdCurso);

            if (existente == null)
                return false;

            existente.Codigo = curso.Codigo;
            existente.Nombre = curso.Nombre;
            existente.Descripcion = curso.Descripcion;
            existente.IdArea = curso.IdArea;
            existente.Estado = curso.Estado;

            _context.CursosGrados.RemoveRange(existente.CursosGrados);

            List<Entities.CursoGrado> relaciones = (idsGrados ?? new List<int>())
                .Distinct()
                .Select(idGrado => new Entities.CursoGrado
                {
                    IdCurso = existente.IdCurso,
                    IdGrado = idGrado
                })
                .ToList();

            if (relaciones.Count > 0)
                await _context.CursosGrados.AddRangeAsync(relaciones);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CambiarEstado(int id, bool estado)
        {
            Entities.Curso? existente = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (existente == null)
                return false;

            existente.Estado = estado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarCurso(int id)
        {
            Entities.Curso? existente = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (existente == null)
                return false;

            _context.Cursos.Remove(existente);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExisteCodigo(
            string codigo,
            int? excluirId = null)
        {
            string codigoLimpio = codigo.Trim().ToLower();

            return await _context.Cursos.AnyAsync(c =>
                c.Codigo.ToLower() == codigoLimpio &&
                (!excluirId.HasValue ||
                 c.IdCurso != excluirId.Value));
        }

        public async Task<bool> ExisteNombre(
            string nombre,
            int idArea,
            int? excluirId = null)
        {
            string nombreLimpio = nombre.Trim().ToLower();

            return await _context.Cursos.AnyAsync(c =>
                c.Nombre.ToLower() == nombreLimpio &&
                c.IdArea == idArea &&
                (!excluirId.HasValue ||
                 c.IdCurso != excluirId.Value));
        }
    }
}