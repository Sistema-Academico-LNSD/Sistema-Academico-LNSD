using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Encargado
{
    public class EncargadoRepository : IEncargadoRepository
    {
        private const int MaximoResultadosBusqueda = 20;

        private readonly AppDbContext _context;

        public EncargadoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.EncargadoEstudiante>> GetPorEstudiante(int idEstudiante)
        {
            return await _context.EncargadosEstudiantes
                .AsNoTracking()
                .Include(v => v.Encargado)
                    .ThenInclude(e => e!.Usuario)
                .Where(v => v.IdEstudiante == idEstudiante)
                .OrderByDescending(v => v.EsPrincipal)
                .ThenBy(v => v.Encargado!.Usuario!.Apellido)
                .ThenBy(v => v.Encargado!.Usuario!.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.EncargadoEstudiante>> GetPorUsuario(int idUsuario)
        {
            return await _context.EncargadosEstudiantes
                .AsNoTracking()
                .Include(v => v.Estudiante)
                    .ThenInclude(e => e!.Usuario)
                .Include(v => v.Estudiante)
                    .ThenInclude(e => e!.Grado)
                .Where(v => v.Encargado!.IdUsuario == idUsuario)
                .OrderByDescending(v => v.EsPrincipal)
                .ThenBy(v => v.Estudiante!.Usuario!.Apellido)
                .ThenBy(v => v.Estudiante!.Usuario!.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Encargado>> Buscar(string? texto, int idEstudiante)
        {
            var query = _context.Encargados
                .AsNoTracking()
                .Include(e => e.Usuario)
                .Where(e =>
                    e.Usuario!.Estado &&
                    !e.EncargadosEstudiantes.Any(v => v.IdEstudiante == idEstudiante));

            if (!string.IsNullOrWhiteSpace(texto))
            {
                string t = texto.Trim();

                query = query.Where(e =>
                    e.Usuario!.Nombre.Contains(t) ||
                    e.Usuario.Apellido.Contains(t) ||
                    e.Usuario.Correo.Contains(t));
            }

            return await query
                .OrderBy(e => e.Usuario!.Apellido)
                .ThenBy(e => e.Usuario!.Nombre)
                .Take(MaximoResultadosBusqueda)
                .ToListAsync();
        }

        public async Task<Entities.Encargado?> GetById(int idEncargado)
        {
            return await _context.Encargados
                .AsNoTracking()
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.IdEncargado == idEncargado);
        }

        public async Task<Entities.EncargadoEstudiante?> GetVinculo(int idEncargado, int idEstudiante)
        {
            return await _context.EncargadosEstudiantes
                .AsNoTracking()
                .FirstOrDefaultAsync(v =>
                    v.IdEncargado == idEncargado && v.IdEstudiante == idEstudiante);
        }

        public async Task<int> ContarVinculos(int idEstudiante)
        {
            return await _context.EncargadosEstudiantes
                .CountAsync(v => v.IdEstudiante == idEstudiante);
        }

        public async Task<bool> Vincular(int idEncargado, int idEstudiante, string parentesco)
        {
            bool esPrimero = !await _context.EncargadosEstudiantes
                .AnyAsync(v => v.IdEstudiante == idEstudiante);

            await _context.EncargadosEstudiantes.AddAsync(new Entities.EncargadoEstudiante
            {
                IdEncargado = idEncargado,
                IdEstudiante = idEstudiante,
                Parentesco = parentesco,
                EsPrincipal = esPrimero
            });

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CrearYVincular(
            Entities.Usuario usuario, string? telefono, string parentesco, int idEstudiante)
        {
            bool esPrimero = !await _context.EncargadosEstudiantes
                .AnyAsync(v => v.IdEstudiante == idEstudiante);

            var encargado = new Entities.Encargado
            {
                Usuario = usuario,
                Telefono = telefono
            };

            await _context.EncargadosEstudiantes.AddAsync(new Entities.EncargadoEstudiante
            {
                Encargado = encargado,
                IdEstudiante = idEstudiante,
                Parentesco = parentesco,
                EsPrincipal = esPrimero
            });

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DefinirPrincipal(int idEstudiante, int idEncargado)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            var vinculos = await _context.EncargadosEstudiantes
                .Where(v => v.IdEstudiante == idEstudiante)
                .ToListAsync();

            var nuevo = vinculos.FirstOrDefault(v => v.IdEncargado == idEncargado);

            if (nuevo == null)
                return false;

            // Primero se quita el principal anterior y luego se asigna el nuevo, en dos guardados,
            // porque el índice único solo admite un principal por estudiante en todo momento.
            foreach (var anterior in vinculos.Where(v => v.EsPrincipal && v.IdEncargado != idEncargado))
                anterior.EsPrincipal = false;

            await _context.SaveChangesAsync();

            nuevo.EsPrincipal = true;

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return true;
        }

        public async Task<bool> Desvincular(int idEncargado, int idEstudiante)
        {
            var vinculo = await _context.EncargadosEstudiantes
                .FirstOrDefaultAsync(v =>
                    v.IdEncargado == idEncargado && v.IdEstudiante == idEstudiante);

            if (vinculo == null)
                return false;

            _context.EncargadosEstudiantes.Remove(vinculo);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
