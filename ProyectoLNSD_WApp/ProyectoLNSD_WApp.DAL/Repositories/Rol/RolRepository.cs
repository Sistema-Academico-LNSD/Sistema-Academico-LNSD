using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Rol
{
    public class RolRepository : IRolRepository
    {
        private readonly AppDbContext _context;

        public RolRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Rol>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Entities.Rol?> GetRolById(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<bool> CreateRol(Entities.Rol rol)
        {
            if (rol == null)
                return false;

            await _context.Roles.AddAsync(rol);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateRol(Entities.Rol rol)
        {
            if (rol == null)
                return false;

            var existing = await _context.Roles.FindAsync(rol.IdRol);

            if (existing == null)
                return false;

            existing.Nombre = rol.Nombre;

            existing.Descripcion = rol.Descripcion;

            _context.Roles.Update(existing);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRol(int id)
        {
            var entity = await _context.Roles.FindAsync(id);

            if (entity == null)
                return false;

            _context.Roles.Remove(entity);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}