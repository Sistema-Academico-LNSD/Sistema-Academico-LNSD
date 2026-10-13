using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Recurso
{
    public class RecursoRepository : IRecursoRepository
    {
        private readonly AppDbContext _context;

        public RecursoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.RecursoAcademico>> Buscar(
            int? idDocente,
            int? idCurso,
            int? idGrado,
            string? titulo,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string? orden)
        {
            IQueryable<Entities.RecursoAcademico> consulta = _context.RecursosAcademicos
                .AsNoTracking()
                .Include(r => r.Docente)
                .Include(r => r.Curso)
                .Include(r => r.Grado);

            // Los filtros se acumulan: solo quedan los recursos que cumplen todos
            if (idDocente.HasValue)
                consulta = consulta.Where(r => r.IdDocente == idDocente.Value);

            if (idCurso.HasValue)
                consulta = consulta.Where(r => r.IdCurso == idCurso.Value);

            if (idGrado.HasValue)
                consulta = consulta.Where(r => r.IdGrado == idGrado.Value);

            if (!string.IsNullOrWhiteSpace(titulo))
            {
                string texto = titulo.Trim();
                consulta = consulta.Where(r => r.Titulo.Contains(texto));
            }

            if (fechaDesde.HasValue)
            {
                DateTime desde = fechaDesde.Value.Date;
                consulta = consulta.Where(r => r.FechaPublicacion >= desde);
            }

            if (fechaHasta.HasValue)
            {
                // Hasta el final del día indicado
                DateTime hasta = fechaHasta.Value.Date.AddDays(1);
                consulta = consulta.Where(r => r.FechaPublicacion < hasta);
            }

            consulta = orden switch
            {
                "fecha_asc" => consulta
                    .OrderBy(r => r.FechaPublicacion)
                    .ThenBy(r => r.IdRecurso),

                "titulo" => consulta
                    .OrderBy(r => r.Titulo)
                    .ThenByDescending(r => r.FechaPublicacion),

                "curso" => consulta
                    .OrderBy(r => r.Curso!.Nombre)
                    .ThenByDescending(r => r.FechaPublicacion),

                "docente" => consulta
                    .OrderBy(r => r.Docente!.Apellidos)
                    .ThenBy(r => r.Docente!.Nombre)
                    .ThenBy(r => r.Curso!.Nombre)
                    .ThenByDescending(r => r.FechaPublicacion),

                // Por defecto: los más recientes primero
                _ => consulta
                    .OrderByDescending(r => r.FechaPublicacion)
                    .ThenByDescending(r => r.IdRecurso)
            };

            return await consulta.ToListAsync();
        }

        public async Task<Entities.RecursoAcademico?> GetById(int idRecurso)
        {
            return await _context.RecursosAcademicos
                .AsNoTracking()
                .Include(r => r.Docente)
                .Include(r => r.Curso)
                .Include(r => r.Grado)
                .FirstOrDefaultAsync(r => r.IdRecurso == idRecurso);
        }

        public async Task<bool> ExisteHash(int idCurso, string hash, int? excluirIdRecurso = null)
        {
            return await _context.RecursosAcademicos.AnyAsync(r =>
                r.IdCurso == idCurso &&
                r.HashArchivo == hash &&
                (!excluirIdRecurso.HasValue || r.IdRecurso != excluirIdRecurso.Value));
        }

        public async Task<bool> ExisteUrl(int idCurso, string url, int? excluirIdRecurso = null)
        {
            return await _context.RecursosAcademicos.AnyAsync(r =>
                r.IdCurso == idCurso &&
                r.UrlEnlace == url &&
                (!excluirIdRecurso.HasValue || r.IdRecurso != excluirIdRecurso.Value));
        }

        public async Task<bool> CursoImpartidoEnGrado(int idCurso, int idGrado)
        {
            return await _context.CursosGrados.AnyAsync(cg =>
                cg.IdCurso == idCurso &&
                cg.IdGrado == idGrado);
        }

        public async Task<List<Entities.Grado>> GetGradosActivosDeCurso(int idCurso)
        {
            return await _context.CursosGrados
                .AsNoTracking()
                .Where(cg => cg.IdCurso == idCurso && cg.Grado!.Estado)
                .Select(cg => cg.Grado!)
                .OrderBy(g => g.Nombre)
                .ToListAsync();
        }

        public async Task<bool> Crear(Entities.RecursoAcademico recurso)
        {
            if (recurso == null)
                return false;

            try
            {
                await _context.RecursosAcademicos.AddAsync(recurso);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Archivo o enlace duplicado en el curso (índices únicos de la base),
                // por ejemplo si dos publicaciones idénticas llegan a la vez.
                _context.Entry(recurso).State = EntityState.Detached;

                return false;
            }
        }

        public async Task<bool> Actualizar(Entities.RecursoAcademico recurso)
        {
            // Se carga el registro y se copian solo los campos editables: así no se
            // arrastran por accidente el docente, el curso ni el grado cargados con Include.
            var actual = await _context.RecursosAcademicos
                .FirstOrDefaultAsync(r => r.IdRecurso == recurso.IdRecurso);

            if (actual == null)
                return false;

            actual.IdCurso = recurso.IdCurso;
            actual.IdGrado = recurso.IdGrado;
            actual.Titulo = recurso.Titulo;
            actual.Descripcion = recurso.Descripcion;
            actual.TipoRecurso = recurso.TipoRecurso;
            actual.NombreArchivoOriginal = recurso.NombreArchivoOriginal;
            actual.RutaArchivo = recurso.RutaArchivo;
            actual.Extension = recurso.Extension;
            actual.TamanoBytes = recurso.TamanoBytes;
            actual.HashArchivo = recurso.HashArchivo;
            actual.UrlEnlace = recurso.UrlEnlace;
            actual.FechaPublicacion = recurso.FechaPublicacion;
            actual.FechaModificacion = recurso.FechaModificacion;

            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException)
            {
                // Duplicado en el curso: se descartan los cambios pendientes
                _context.Entry(actual).State = EntityState.Detached;

                return false;
            }
        }

        public async Task<bool> Eliminar(int idRecurso)
        {
            int filas = await _context.RecursosAcademicos
                .Where(r => r.IdRecurso == idRecurso)
                .ExecuteDeleteAsync();

            return filas > 0;
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

        public async Task<List<(int Id, string Nombre)>> GetDocentesConRecursos()
        {
            var docentes = await _context.RecursosAcademicos
                .AsNoTracking()
                .Select(r => new { r.IdDocente, r.Docente!.Nombre, r.Docente.Apellidos })
                .Distinct()
                .OrderBy(d => d.Apellidos)
                .ThenBy(d => d.Nombre)
                .ToListAsync();

            return docentes
                .Select(d => (d.IdDocente, $"{d.Nombre} {d.Apellidos}"))
                .ToList();
        }

        public async Task<List<(int Id, string Nombre)>> GetCursosConRecursos()
        {
            var cursos = await _context.RecursosAcademicos
                .AsNoTracking()
                .Select(r => new { r.IdCurso, r.Curso!.Codigo, r.Curso.Nombre })
                .Distinct()
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return cursos
                .Select(c => (c.IdCurso, $"{c.Codigo} - {c.Nombre}"))
                .ToList();
        }
    }
}