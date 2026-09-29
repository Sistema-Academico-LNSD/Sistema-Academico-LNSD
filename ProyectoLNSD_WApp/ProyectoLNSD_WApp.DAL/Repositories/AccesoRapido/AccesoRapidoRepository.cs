using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.AccesoRapido
{
    public class AccesoRapidoRepository : IAccesoRapidoRepository
    {
        private readonly AppDbContext _context;

        public AccesoRapidoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.AccesoRapido>> GetAccesos()
        {
            return await _context.AccesosRapidos
                .AsNoTracking()
                .OrderBy(a => a.Orden)
                .ThenBy(a => a.IdAcceso)
                .ToListAsync();
        }

        public async Task<Entities.AccesoRapido?> GetAcceso(int idAcceso)
        {
            return await _context.AccesosRapidos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAcceso == idAcceso);
        }

        public async Task<List<Entities.AccesoRapidoRol>> GetAsignaciones()
        {
            return await _context.AccesosRapidosRoles
                .AsNoTracking()
                .Include(x => x.Rol)
                .ToListAsync();
        }

        public async Task<List<Entities.AccesoRapido>> GetActivosPorRol(IEnumerable<string> nombresRol)
        {
            var roles = nombresRol.ToList();

            return await _context.AccesosRapidosRoles
                .AsNoTracking()
                .Where(x => roles.Contains(x.Rol!.Nombre) && x.Acceso!.Activo)
                .Select(x => x.Acceso!)
                .Distinct()
                .OrderBy(a => a.Orden)
                .ThenBy(a => a.IdAcceso)
                .ToListAsync();
        }

        public async Task<bool> CreateAcceso(Entities.AccesoRapido acceso, IEnumerable<int> idsRoles)
        {
            if (acceso == null)
                return false;

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            await _context.AccesosRapidos.AddAsync(acceso);
            await _context.SaveChangesAsync();   // genera IdAcceso

            foreach (var idRol in idsRoles)
                _context.AccesosRapidosRoles.Add(new Entities.AccesoRapidoRol { IdAcceso = acceso.IdAcceso, IdRol = idRol });

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateAcceso(Entities.AccesoRapido acceso, IEnumerable<int> idsRoles)
        {
            if (acceso == null)
                return false;

            var existing = await _context.AccesosRapidos.FindAsync(acceso.IdAcceso);
            if (existing == null)
                return false;

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            existing.Nombre = acceso.Nombre;
            existing.Descripcion = acceso.Descripcion;
            existing.Icono = acceso.Icono;
            existing.Enlace = acceso.Enlace;
            existing.Orden = acceso.Orden;
            existing.Activo = acceso.Activo;

            // Roles: solo se quitan los que ya no están y solo se agregan los nuevos
            var nuevos = idsRoles.ToHashSet();
            var actuales = await _context.AccesosRapidosRoles
                .Where(x => x.IdAcceso == acceso.IdAcceso)
                .ToListAsync();

            _context.AccesosRapidosRoles.RemoveRange(actuales.Where(x => !nuevos.Contains(x.IdRol)));

            var yaAsignados = actuales.Select(x => x.IdRol).ToHashSet();
            foreach (var idRol in nuevos.Where(id => !yaAsignados.Contains(id)))
                _context.AccesosRapidosRoles.Add(new Entities.AccesoRapidoRol { IdAcceso = acceso.IdAcceso, IdRol = idRol });

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteAcceso(int idAcceso)
        {
            var existing = await _context.AccesosRapidos.FindAsync(idAcceso);
            if (existing == null)
                return false;

            // Las filas de Acceso_Rapido_Rol se borran solas (ON DELETE CASCADE)
            _context.AccesosRapidos.Remove(existing);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
