using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Inventario
{
    public interface IInventarioService
    {
        // Tipos
        Task<RespuestaDTO<List<CategoriaInventarioDTO>>> GetCategorias(bool incluirInactivas);

        Task<RespuestaDTO<CategoriaInventarioDTO>> GuardarCategoria(CategoriaInventarioGuardarDTO dto);

        Task<RespuestaDTO<CategoriaInventarioDTO>> CambiarEstadoCategoria(int idCategoria, bool activo);

        // Artículos
        Task<RespuestaDTO<List<ArticuloInventarioDTO>>> BuscarArticulos(InventarioFiltroDTO filtro);

        Task<RespuestaDTO<ArticuloInventarioDTO>> GuardarArticulo(int idUsuario, ArticuloInventarioGuardarDTO dto);

        Task<RespuestaDTO<ArticuloInventarioDTO>> CambiarEstadoArticulo(int idInventario, bool activo);
        
        // Movimientos
        Task<RespuestaDTO<MovimientoInventarioDTO>> RegistrarMovimiento(int idUsuario, MovimientoInventarioGuardarDTO dto);

        Task<RespuestaDTO<HistorialArticuloDTO>> GetHistorial(int idInventario);

        // Reportes y estadísticas
        Task<RespuestaDTO<ReporteExistenciasDTO>> GetReporteExistencias(ReporteExistenciasFiltroDTO filtro);

        Task<RespuestaDTO<byte[]>> ExportarExistencias(ReporteExistenciasFiltroDTO filtro);

        Task<RespuestaDTO<ReporteMovimientosDTO>> GetReporteMovimientos(ReporteMovimientosFiltroDTO filtro);

        Task<RespuestaDTO<byte[]>> ExportarMovimientos(ReporteMovimientosFiltroDTO filtro);

        Task<RespuestaDTO<EstadisticasInventarioDTO>> GetEstadisticas(ReporteMovimientosFiltroDTO filtro);
    }
}