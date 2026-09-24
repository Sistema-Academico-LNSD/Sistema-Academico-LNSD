using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.RolPermiso
{
    public class RolPermisoRepository : IRolPermisoRepository
    {
        private readonly AppDbContext _context;

        public RolPermisoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.RolPermiso>> GetPermisosPorRol(int idRol)
        {
            return await _context.RolesPermisos
                .Include(p => p.Modulo)
                .Where(p => p.IdRol == idRol)
                .ToListAsync();
        }

        public async Task<bool> GuardarPermisos(int idRol, List<Entities.RolPermiso> permisos)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existentes = await _context.RolesPermisos
                    .Where(p => p.IdRol == idRol)
                    .ToListAsync();

                if (existentes.Count > 0)
                {
                    _context.RolesPermisos.RemoveRange(existentes);
                    await _context.SaveChangesAsync();
                }

                if (permisos.Count > 0)
                {
                    await _context.RolesPermisos.AddRangeAsync(permisos);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();

                return false;
            }
        }
    }
}