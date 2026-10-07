using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Docente
{
    public class DocenteRepository : IDocenteRepository
    {
        private readonly AppDbContext _context;

        public DocenteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Docente>> Buscar(string? texto, int? idArea, bool? estado)
        {
            var query = _context.Docentes
                .AsNoTracking()
                .Include(d => d.Area)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                string t = texto.Trim();

                query = query.Where(d =>
                    d.Nombre.Contains(t) ||
                    d.Apellidos.Contains(t) ||
                    d.Identificacion.Contains(t) ||
                    d.Correo.Contains(t) ||
                    (d.Especialidad != null && d.Especialidad.Contains(t)));
            }

            if (idArea.HasValue)
            {
                query = query.Where(d => d.IdArea == idArea.Value);
            }

            if (estado.HasValue)
            {
                query = query.Where(d => d.Estado == estado.Value);
            }

            return await query
                .OrderBy(d => d.Apellidos)
                .ThenBy(d => d.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.Docente?> GetById(int idDocente)
        {
            return await _context.Docentes
                .AsNoTracking()
                .Include(d => d.Area)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.IdDocente == idDocente);
        }

        public async Task<bool> ExisteIdentificacion(string identificacion, int? excluirIdDocente = null)
        {
            return await _context.Docentes.AnyAsync(d =>
                d.Identificacion == identificacion &&
                (!excluirIdDocente.HasValue || d.IdDocente != excluirIdDocente.Value));
        }

        public async Task<bool> ExisteCorreo(string correo, int? excluirIdDocente = null)
        {
            return await _context.Docentes.AnyAsync(d =>
                d.Correo == correo &&
                (!excluirIdDocente.HasValue || d.IdDocente != excluirIdDocente.Value));
        }

        public async Task<bool> Crear(Entities.Docente docente)
        {
            docente.FechaRegistro = DateTime.UtcNow;

            await _context.Docentes.AddAsync(docente);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Actualizar(Entities.Docente docente)
        {
            var existente = await _context.Docentes.FindAsync(docente.IdDocente);

            if (existente == null)
                return false;

            existente.Nombre = docente.Nombre;
            existente.Apellidos = docente.Apellidos;
            existente.Identificacion = docente.Identificacion;
            existente.Correo = docente.Correo;
            existente.Telefono = docente.Telefono;
            existente.Direccion = docente.Direccion;
            existente.Titulos = docente.Titulos;
            existente.Especialidad = docente.Especialidad;
            existente.AniosExperiencia = docente.AniosExperiencia;
            existente.IdArea = docente.IdArea;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CambiarEstado(int idDocente, bool estado)
        {
            var existente = await _context.Docentes.FindAsync(idDocente);

            if (existente == null)
                return false;

            existente.Estado = estado;

            await _context.SaveChangesAsync();

            return true;
        }


        // ---- Cuenta de usuario (MDOF-01-02) ----

        public async Task<List<Entities.Usuario>> GetUsuariosDisponibles(string nombreRol)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .Where(u =>
                    u.Estado &&
                    u.Rol != null && u.Rol.Nombre == nombreRol &&
                    !_context.Docentes.Any(d => d.IdUsuario == u.IdUsuario))
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<bool> UsuarioEstaAsociado(int idUsuario)
        {
            return await _context.Docentes.AnyAsync(d => d.IdUsuario == idUsuario);
        }

        public async Task<bool> AsignarUsuario(int idDocente, int? idUsuario)
        {
            var existente = await _context.Docentes.FindAsync(idDocente);

            if (existente == null)
                return false;

            existente.IdUsuario = idUsuario;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CrearCuentaYAsociar(int idDocente, Entities.Usuario usuario)
        {
            var existente = await _context.Docentes.FindAsync(idDocente);

            if (existente == null)
                return false;

            existente.Usuario = usuario;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}