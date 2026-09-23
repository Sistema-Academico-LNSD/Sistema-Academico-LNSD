using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Usuario
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Usuario>> GetUsuarios()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();
        }

        public async Task<Entities.Usuario?> GetUsuarioById(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
        }

        public async Task<Entities.Usuario?> GetUsuarioByCorreo(string correo)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }

        public async Task<List<Entities.Usuario>> BuscarUsuarios(
    string? nombre,
    string? correo,
    int? idRol,
    bool? estado)
        {
            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(u =>
                    u.Nombre.Contains(nombre));
            }

            if (!string.IsNullOrWhiteSpace(correo))
            {
                query = query.Where(u =>
                    u.Correo.Contains(correo));
            }

            if (idRol.HasValue)
            {
                query = query.Where(u =>
                    u.IdRol == idRol.Value);
            }

            if (estado.HasValue)
            {
                query = query.Where(u =>
                    u.Estado == estado.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CreateUsuario(Entities.Usuario usuario)
        {
            if (usuario == null)
                return false;

            await _context.Usuarios.AddAsync(usuario);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUsuario(Entities.Usuario usuario)
        {
            if (usuario == null)
                return false;

            var existing = await _context.Usuarios
                .FindAsync(usuario.IdUsuario);

            if (existing == null)
                return false;

            existing.Nombre = usuario.Nombre;
            existing.Apellido = usuario.Apellido;
            existing.Correo = usuario.Correo;
            existing.IdRol = usuario.IdRol;
            existing.Estado = usuario.Estado;

            _context.Usuarios.Update(existing);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUsuario(int id)
        {
            var entity = await _context.Usuarios.FindAsync(id);

            if (entity == null)
                return false;

            _context.Usuarios.Remove(entity);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CambiarEstado(int id, bool estado)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return false;

            usuario.Estado = estado;

            _context.Usuarios.Update(usuario);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}