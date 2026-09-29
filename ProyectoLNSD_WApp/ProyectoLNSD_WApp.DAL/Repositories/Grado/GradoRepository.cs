using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Grado
{
    public class GradoRepository : IGradoRepository
    {
        private readonly AppDbContext _context;

        public GradoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Grado>> GetGrados()
        {
            return await _context.Grados
                .Include(g => g.Secciones)
                .OrderBy(g => g.Nivel)
                .ThenBy(g => g.Nombre)
                .ToListAsync();
        }

        public async Task<List<Entities.Grado>> GetGradosActivos()
        {
            return await _context.Grados
                .Where(g => g.Estado)
                .OrderBy(g => g.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.Grado?> GetGradoById(int id)
        {
            return await _context.Grados.FindAsync(id);
        }

        public async Task<Entities.Grado?> GetGradoConSecciones(int id)
        {
            return await _context.Grados
                .Include(g => g.Secciones)
                .FirstOrDefaultAsync(g => g.IdGrado == id);
        }

        public async Task<bool> CreateGrado(Entities.Grado grado)
        {
            if (grado == null)
                return false;

            await _context.Grados.AddAsync(grado);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateGrado(Entities.Grado grado)
        {
            var existing = await _context.Grados.FindAsync(grado.IdGrado);
            if (existing == null)
                return false;

            existing.Codigo = grado.Codigo;
            existing.Nombre = grado.Nombre;
            existing.Nivel = grado.Nivel;
            existing.Estado = grado.Estado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CambiarEstado(int id, bool estado)
        {
            var existing = await _context.Grados.FindAsync(id);
            if (existing == null)
                return false;

            existing.Estado = estado;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExisteNombre(string nombre, int? excluirId = null)
        {
            string n = nombre.Trim().ToLower();
            return await _context.Grados.AnyAsync(g =>
                g.Nombre.ToLower() == n &&
                (!excluirId.HasValue || g.IdGrado != excluirId.Value));
        }

        public async Task<bool> ExisteCodigo(string codigo, int? excluirId = null)
        {
            string c = codigo.Trim().ToLower();
            return await _context.Grados.AnyAsync(g =>
                g.Codigo.ToLower() == c &&
                (!excluirId.HasValue || g.IdGrado != excluirId.Value));
        }
    }
}
