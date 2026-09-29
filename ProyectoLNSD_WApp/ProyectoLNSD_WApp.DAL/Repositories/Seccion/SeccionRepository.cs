using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Seccion
{
    public class SeccionRepository : ISeccionRepository
    {
        private readonly AppDbContext _context;

        public SeccionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Seccion>> GetSecciones()
        {
            return await _context.Secciones
                .Include(s => s.Grado)
                .OrderBy(s => s.Grado!.Nombre)
                .ThenBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.Seccion?> GetSeccionById(int id)
        {
            return await _context.Secciones
                .Include(s => s.Grado)
                .FirstOrDefaultAsync(s => s.IdSeccion == id);
        }

        public async Task<bool> CreateSeccion(Entities.Seccion seccion)
        {
            if (seccion == null)
                return false;

            await _context.Secciones.AddAsync(seccion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateSeccion(Entities.Seccion seccion)
        {
            var existing = await _context.Secciones.FindAsync(seccion.IdSeccion);
            if (existing == null)
                return false;

            existing.IdGrado = seccion.IdGrado;
            existing.Nombre = seccion.Nombre;
            existing.CapacidadMaxima = seccion.CapacidadMaxima;
            existing.Estado = seccion.Estado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CambiarEstado(int id, bool estado)
        {
            var existing = await _context.Secciones.FindAsync(id);
            if (existing == null)
                return false;

            existing.Estado = estado;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExisteNombreEnGrado(int idGrado, string nombre, int? excluirId = null)
        {
            string n = nombre.Trim().ToLower();
            return await _context.Secciones.AnyAsync(s =>
                s.IdGrado == idGrado &&
                s.Nombre.ToLower() == n &&
                (!excluirId.HasValue || s.IdSeccion != excluirId.Value));
        }
    }
}
