using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Inventario
{
    public interface IInventarioRepository
    {
        // ---- Tipos / categorías ----
        Task<List<CategoriaInventario>> GetCategorias(bool soloActivas);

        Task<CategoriaInventario?> GetCategoria(int idCategoria);

        Task<bool> ExisteNombreCategoria(string nombre, int? excluirIdCategoria = null);

        Task CrearCategoria(CategoriaInventario categoria);

        Task ActualizarCategoria(CategoriaInventario categoria);

        /// <summary>Cantidad de artículos por categoría (idCategoria → total).</summary>
        Task<Dictionary<int, int>> ContarArticulosPorCategoria();

        // ---- Artículos ----
        Task<List<Entities.Inventario>> BuscarArticulos(InventarioFiltro filtro, int limite);

        Task<Entities.Inventario?> GetArticulo(int idInventario);

        Task<bool> ExisteNombreArticulo(int idCategoria, string nombre, int? excluirIdInventario = null);

        /// <summary>Crea el artículo y, si trae cantidad, su movimiento de entrada inicial (una sola transacción).</summary>
        Task CrearArticulo(Entities.Inventario articulo, int idUsuario);

        Task ActualizarArticulo(Entities.Inventario articulo);

        Task<InventarioMovimiento?> RegistrarMovimiento(InventarioMovimiento movimiento);

        Task<List<InventarioMovimiento>> GetMovimientos(int idInventario, int limite);

        // ---- Reportes y estadísticas ----

        Task<List<InventarioMovimiento>> GetMovimientosReporte(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria, string? tipo, int limite);

        Task<List<ResumenMensualMovimiento>> GetResumenMensual(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria);

        Task<List<ResumenArticuloMovimiento>> GetTopArticulos(
            DateTime desde, DateTime hastaExclusivo, int? idCategoria, string tipo, int top);
    }
}