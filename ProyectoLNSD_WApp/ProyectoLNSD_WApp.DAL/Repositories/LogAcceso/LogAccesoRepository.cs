using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.LogAcceso
{
    public class LogAccesoRepository : ILogAccesoRepository
    {
        private readonly AppDbContext _context;

        public LogAccesoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Registrar(Entities.LogAcceso log)
        {
            if (log == null)
                return false;

            await _context.LogsAcceso.AddAsync(log);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Entities.LogAcceso>> GetHistorial(
            string? correo,
            DateTime? desde,
            DateTime? hasta,
            bool? soloFallidos)
        {
            var query = _context.LogsAcceso
                .Include(l => l.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(correo))
            {
                query = query.Where(l => l.Correo.Contains(correo));
            }

            if (desde.HasValue)
            {
                query = query.Where(l => l.Fecha >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(l => l.Fecha <= hasta.Value);
            }

            if (soloFallidos == true)
            {
                query = query.Where(l => !l.Exitoso);
            }

            return await query
                .OrderByDescending(l => l.Fecha)
                .ToListAsync();
        }
    }
}