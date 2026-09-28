using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo
{
    public class PeriodoLectivoRepository : IPeriodoLectivoRepository
    {
        private readonly AppDbContext _context;

        public PeriodoLectivoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.PeriodoLectivo>> GetPeriodos()
        {
            return await _context.PeriodosLectivos
                .AsNoTracking()
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<Entities.PeriodoLectivo?> GetPeriodo(int idPeriodo)
        {
            return await _context.PeriodosLectivos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPeriodo == idPeriodo);
        }

        public async Task<Entities.PeriodoLectivo?> GetPeriodoActivo()
        {
            return await _context.PeriodosLectivos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Activo);
        }

        public async Task<bool> ExisteNombre(string nombre, int? excluirIdPeriodo = null)
        {
            return await _context.PeriodosLectivos.AnyAsync(p =>
                p.Nombre == nombre &&
                (excluirIdPeriodo == null || p.IdPeriodo != excluirIdPeriodo));
        }

        public async Task<bool> CreatePeriodo(Entities.PeriodoLectivo periodo, bool desactivarAnteriores)
        {
            if (periodo == null)
                return false;

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            if (desactivarAnteriores)
                await DesactivarOtros(0);

            await _context.PeriodosLectivos.AddAsync(periodo);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();
            return true;
        }

        public async Task<bool> UpdatePeriodo(Entities.PeriodoLectivo periodo, bool desactivarAnteriores)
        {
            if (periodo == null)
                return false;

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            var existing = await _context.PeriodosLectivos.FindAsync(periodo.IdPeriodo);
            if (existing == null)
                return false;

            // Primero se libera el "activo" anterior: el índice único de la BD solo permite uno.
            if (desactivarAnteriores)
                await DesactivarOtros(periodo.IdPeriodo);

            existing.Nombre = periodo.Nombre;
            existing.FechaInicio = periodo.FechaInicio;
            existing.FechaFin = periodo.FechaFin;
            existing.Activo = periodo.Activo;

            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();
            return true;
        }

        private async Task DesactivarOtros(int idPeriodoAExcluir)
        {
            await _context.PeriodosLectivos
                .Where(p => p.Activo && p.IdPeriodo != idPeriodoAExcluir)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Activo, false));
        }
    }
}
