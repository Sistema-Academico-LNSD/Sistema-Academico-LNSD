using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Tiquete
{
    public class TiqueteRepository : ITiqueteRepository
    {
        private readonly AppDbContext _context;

        public TiqueteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Entities.Tiquete?> GetTiqueteDisponibleByUsuario(int idUsuario)
        {
            return await _context.Tiquetes
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.IdUsuario == idUsuario &&
                    t.Estado == Entities.EstadoTiquete.Disponible);
        }

        public async Task<List<Entities.Tiquete>> GetTiquetesByUsuario(int idUsuario)
        {
            return await _context.Tiquetes
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario)
                .OrderByDescending(t => t.FechaGeneracion)
                .ToListAsync();
        }

        public async Task<bool> ExisteCodigo(string codigo)
        {
            return await _context.Tiquetes.AnyAsync(t => t.Codigo == codigo);
        }

        public async Task<bool> CreateTiquete(Entities.Tiquete tiquete)
        {
            if (tiquete == null)
                return false;

            try
            {
                await _context.Tiquetes.AddAsync(tiquete);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                _context.Entry(tiquete).State = EntityState.Detached;

                return false;
            }
        }

        public async Task<Entities.Tiquete?> GetTiqueteByCodigo(string codigo)
        {
            return await _context.Tiquetes
                .AsNoTracking()
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Codigo == codigo);
        }

        public async Task<bool> MarcarUtilizado(int idTiquete, int idUsuarioValidador, DateTime fechaUtilizacion)
        {
            // Un solo UPDATE condicionado a que siga Disponible: si dos encargados lo
            // intentan a la vez, solo uno afecta la fila y el otro recibe false.
            int filas = await _context.Tiquetes
                .Where(t => t.IdTiquete == idTiquete &&
                            t.Estado == Entities.EstadoTiquete.Disponible)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Estado, Entities.EstadoTiquete.Utilizado)
                    .SetProperty(t => t.FechaUtilizacion, (DateTime?)fechaUtilizacion)
                    .SetProperty(t => t.IdUsuarioValidador, (int?)idUsuarioValidador));

            return filas > 0;
        }

        public async Task<List<Entities.Tiquete>> GetTiquetes(string? estado)
        {
            IQueryable<Entities.Tiquete> consulta = _context.Tiquetes
                .AsNoTracking()
                .Include(t => t.Usuario);

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(t => t.Estado == estado);

            return await consulta
                .OrderByDescending(t => t.FechaGeneracion)
                .ToListAsync();
        }

        public async Task<int> ContarTiquetes(string? estado = null)
        {
            IQueryable<Entities.Tiquete> consulta = _context.Tiquetes;

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(t => t.Estado == estado);

            return await consulta.CountAsync();
        }
    }
}
