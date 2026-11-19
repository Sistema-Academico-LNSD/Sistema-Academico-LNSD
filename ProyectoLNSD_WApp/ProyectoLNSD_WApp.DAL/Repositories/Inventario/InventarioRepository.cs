using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Inventario
{
    public class InventarioRepository : IInventarioRepository
    {
        private readonly AppDbContext _context;

        public InventarioRepository(AppDbContext context)
        {
            _context = context;
        }

        // ---- Tipos / categorías ----

        public async Task<List<CategoriaInventario>> GetCategorias(bool soloActivas)
        {
            return await _context.CategoriasInventario
                .AsNoTracking()
                .Where(c => !soloActivas || c.Activo)
                .OrderByDescending(c => c.Activo)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<CategoriaInventario?> GetCategoria(int idCategoria)
        {
            return await _context.CategoriasInventario
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria);
        }

        public async Task<bool> ExisteNombreCategoria(string nombre, int? excluirIdCategoria = null)
        {
            return await _context.CategoriasInventario
                .AnyAsync(c => c.Nombre == nombre &&
                               (!excluirIdCategoria.HasValue || c.IdCategoria != excluirIdCategoria.Value));
        }

        public async Task CrearCategoria(CategoriaInventario categoria)
        {
            _context.CategoriasInventario.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarCategoria(CategoriaInventario categoria)
        {
            _context.CategoriasInventario.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, int>> ContarArticulosPorCategoria()
        {
            return await _context.Inventarios
                .AsNoTracking()
                .GroupBy(i => i.IdCategoria)
                .Select(g => new { g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Total);
        }

        // ---- Artículos ----

        public async Task<List<Entities.Inventario>> BuscarArticulos(InventarioFiltro filtro, int limite)
        {
            var consulta = _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Texto))
                consulta = consulta.Where(i => i.Nombre.Contains(filtro.Texto));

            if (filtro.IdCategoria.HasValue)
                consulta = consulta.Where(i => i.IdCategoria == filtro.IdCategoria.Value);

            if (filtro.Activo.HasValue)
                consulta = consulta.Where(i => i.Activo == filtro.Activo.Value);

            if (filtro.SoloBajoMinimo)
                consulta = consulta.Where(i => i.Activo && i.CantidadMinima > 0 && i.Cantidad < i.CantidadMinima);

            return await consulta
                .OrderByDescending(i => i.Activo)
                .ThenBy(i => i.Nombre)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<Entities.Inventario?> GetArticulo(int idInventario)
        {
            return await _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(i => i.IdInventario == idInventario);
        }

        public async Task<bool> ExisteNombreArticulo(int idCategoria, string nombre, int? excluirIdInventario = null)
        {
            return await _context.Inventarios
                .AnyAsync(i => i.IdCategoria == idCategoria && i.Nombre == nombre &&
                               (!excluirIdInventario.HasValue || i.IdInventario != excluirIdInventario.Value));
        }

        public async Task CrearArticulo(Entities.Inventario articulo, int idUsuario)
        {
            if (articulo.Cantidad > 0)
            {
                articulo.Movimientos.Add(new InventarioMovimiento
                {
                    Tipo = TipoMovimientoInventario.Entrada,
                    Cantidad = articulo.Cantidad,
                    ExistenciaResultante = articulo.Cantidad,
                    Fecha = articulo.FechaRegistro,
                    Motivo = "Existencia inicial",
                    IdUsuarioRegistra = idUsuario
                });
            }

            _context.Inventarios.Add(articulo);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarArticulo(Entities.Inventario articulo)
        {
            // Solo el artículo: no se arrastra la categoría que viene incluida
            _context.Entry(articulo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // ---- Movimientos ----

        public async Task<InventarioMovimiento?> RegistrarMovimiento(InventarioMovimiento movimiento)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            int delta = movimiento.Tipo == TipoMovimientoInventario.Entrada
                ? movimiento.Cantidad
                : -movimiento.Cantidad;

            int idInventario = movimiento.IdInventario;

            // La condición va dentro del UPDATE: no deja la existencia en negativo ni usa artículos inactivos
            int filas = await _context.Inventarios
                .Where(i => i.IdInventario == idInventario && i.Activo && i.Cantidad + delta >= 0)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Cantidad, i => i.Cantidad + delta));

            if (filas == 0)
                return null;   // al salir sin commit, la transacción se revierte

            movimiento.ExistenciaResultante = await _context.Inventarios
                .Where(i => i.IdInventario == idInventario)
                .Select(i => i.Cantidad)
                .FirstAsync();

            _context.InventarioMovimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();

            return movimiento;
        }

        public async Task<List<InventarioMovimiento>> GetMovimientos(int idInventario, int limite)
        {
            return await _context.InventarioMovimientos
                .AsNoTracking()
                .Include(m => m.UsuarioRegistra)
                .Where(m => m.IdInventario == idInventario)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdMovimiento)
                .Take(limite)
                .ToListAsync();
        }

        // ---- Reportes y estadísticas ----

        private IQueryable<InventarioMovimiento> MovimientosEnRango(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria, string? tipo)
        {
            var consulta = _context.InventarioMovimientos
                .AsNoTracking()
                .Where(m => m.Fecha >= desde && m.Fecha < hastaExclusivo);

            if (idCategoria.HasValue)
                consulta = consulta.Where(m => m.Inventario!.IdCategoria == idCategoria.Value);

            if (!string.IsNullOrEmpty(tipo))
                consulta = consulta.Where(m => m.Tipo == tipo);

            return consulta;
        }

        public async Task<List<InventarioMovimiento>> GetMovimientosReporte(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria, string? tipo, int limite)
        {
            return await MovimientosEnRango(desde, hastaExclusivo, idCategoria, tipo)
                .Include(m => m.Inventario).ThenInclude(i => i!.Categoria)
                .Include(m => m.UsuarioRegistra)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdMovimiento)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<List<ResumenMensualMovimiento>> GetResumenMensual(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria)
        {
            return await MovimientosEnRango(desde, hastaExclusivo, idCategoria, null)
                .GroupBy(m => new { m.Fecha.Year, m.Fecha.Month, m.Tipo })
                .Select(g => new ResumenMensualMovimiento
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Tipo = g.Key.Tipo,
                    Movimientos = g.Count()
                })
                .ToListAsync();
        }

        public async Task<List<ResumenArticuloMovimiento>> GetTopArticulos(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria, string tipo, int top)
        {
            return await MovimientosEnRango(desde, hastaExclusivo, idCategoria, tipo)
                .GroupBy(m => new
                {
                    m.IdInventario,
                    m.Inventario!.Nombre,
                    m.Inventario.UnidadMedida,
                    Categoria = m.Inventario.Categoria!.Nombre
                })
                .Select(g => new ResumenArticuloMovimiento
                {
                    IdInventario = g.Key.IdInventario,
                    Articulo = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    UnidadMedida = g.Key.UnidadMedida,
                    Movimientos = g.Count(),
                    Unidades = g.Sum(x => (long)x.Cantidad)
                })
                .OrderByDescending(x => x.Unidades)
                .ThenBy(x => x.Articulo)
                .Take(top)
                .ToListAsync();
        }
    }
}