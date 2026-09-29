using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Modulo
{
    public class ModuloRepository : IModuloRepository
    {
        private readonly AppDbContext _context;

        public ModuloRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Modulo>> GetModulos()
        {
            return await _context.Modulos
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }
    }
}