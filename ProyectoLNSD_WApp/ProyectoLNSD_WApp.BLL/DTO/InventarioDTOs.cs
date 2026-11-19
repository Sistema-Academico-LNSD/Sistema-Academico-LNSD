using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class CategoriaInventarioDTO
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public int TotalArticulos { get; set; }
    }

    public class CategoriaInventarioGuardarDTO
    {
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Indique el nombre del tipo de inventario")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class ArticuloInventarioDTO
    {
        public int IdInventario { get; set; }

        public int IdCategoria { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public bool CategoriaActiva { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public int Cantidad { get; set; }

        public string UnidadMedida { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public DateOnly? FechaVencimiento { get; set; }

        public int CantidadMinima { get; set; }

        public bool Activo { get; set; }

        /// <summary>Hay mínimo definido y la existencia está por debajo.</summary>
        public bool BajoMinimo { get; set; }

        public bool Vencido { get; set; }
    }

    public class ArticuloInventarioGuardarDTO
    {
        public int IdInventario { get; set; }

        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Indique el nombre del artículo")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres")]
        public string? Descripcion { get; set; }

        /// <summary>Solo se usa al registrar; después la existencia cambia con entradas y salidas.</summary>
        public int CantidadInicial { get; set; }

        [Required(ErrorMessage = "Indique la unidad de medida")]
        [StringLength(30, ErrorMessage = "La unidad de medida no puede superar los 30 caracteres")]
        public string UnidadMedida { get; set; } = string.Empty;

        public DateOnly? FechaVencimiento { get; set; }

        public int CantidadMinima { get; set; }
    }

    public class InventarioFiltroDTO
    {
        public string? Texto { get; set; }

        public int? IdCategoria { get; set; }

        /// <summary>"Activo", "Inactivo" o vacío (todos).</summary>
        public string? Estado { get; set; }

        public bool SoloBajoMinimo { get; set; }
    }

    public class MovimientoInventarioGuardarDTO
    {
        public int IdInventario { get; set; }

        /// <summary>"Entrada" o "Salida".</summary>
        [Required(ErrorMessage = "Indique el tipo de movimiento")]
        [StringLength(10)]
        public string Tipo { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        [StringLength(300, ErrorMessage = "El motivo no puede superar los 300 caracteres")]
        public string? Motivo { get; set; }

        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string? NombreResponsable { get; set; }

        [StringLength(30, ErrorMessage = "La cédula no puede superar los 30 caracteres")]
        public string? CedulaResponsable { get; set; }
    }

    public class MovimientoInventarioDTO
    {
        public int IdMovimiento { get; set; }

        public int IdInventario { get; set; }

        public string Articulo { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public int ExistenciaResultante { get; set; }

        public DateTime Fecha { get; set; }

        public string? Motivo { get; set; }

        public string? NombreResponsable { get; set; }

        public string? CedulaResponsable { get; set; }

        public string? UsuarioRegistra { get; set; }
    }

    public class HistorialArticuloDTO
    {
        public ArticuloInventarioDTO Articulo { get; set; } = new();

        public int TotalEntradas { get; set; }

        public int TotalSalidas { get; set; }

        public List<MovimientoInventarioDTO> Movimientos { get; set; } = new();
    }

    // ---- Reportes y estadísticas (MIVF-01-16, 01-17 y 01-18) ----

    public class ReporteExistenciasFiltroDTO
    {
        public int? IdCategoria { get; set; }

        public bool IncluirInactivos { get; set; }
    }

    public class ReporteExistenciasDTO
    {
        public string Categoria { get; set; } = "Todos los tipos";

        public DateTime GeneradoEl { get; set; }

        public int TotalArticulos { get; set; }

        public int BajoMinimo { get; set; }

        public int Vencidos { get; set; }

        public List<ArticuloInventarioDTO> Articulos { get; set; } = new();
    }

    public class ReporteMovimientosFiltroDTO
    {
        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        public int? IdCategoria { get; set; }

        /// <summary>"Entrada", "Salida" o vacío (ambos).</summary>
        public string? Tipo { get; set; }
    }

    public class MovimientoReporteDTO : MovimientoInventarioDTO
    {
        public string Categoria { get; set; } = string.Empty;

        public string UnidadMedida { get; set; } = string.Empty;
    }

    public class ReporteMovimientosDTO
    {
        public DateOnly Desde { get; set; }

        public DateOnly Hasta { get; set; }

        public string Categoria { get; set; } = "Todos los tipos";

        public string Tipo { get; set; } = "Entradas y salidas";

        public int TotalEntradas { get; set; }

        public int TotalSalidas { get; set; }

        /// <summary>true si había más movimientos de los que se muestran.</summary>
        public bool Truncado { get; set; }

        public List<MovimientoReporteDTO> Movimientos { get; set; } = new();
    }

    public class PuntoMensualInventarioDTO
    {
        public string Etiqueta { get; set; } = string.Empty;

        public int Entradas { get; set; }

        public int Salidas { get; set; }
    }

    public class ArticuloMovimientoResumenDTO
    {
        public int IdInventario { get; set; }

        public string Articulo { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string UnidadMedida { get; set; } = string.Empty;

        public int Movimientos { get; set; }

        public long Unidades { get; set; }
    }

    public class EstadisticasInventarioDTO
    {
        public DateOnly Desde { get; set; }

        public DateOnly Hasta { get; set; }

        public int TotalEntradas { get; set; }

        public int TotalSalidas { get; set; }

        public List<PuntoMensualInventarioDTO> PorMes { get; set; } = new();

        public List<ArticuloMovimientoResumenDTO> TopSalidas { get; set; } = new();

        public List<ArticuloMovimientoResumenDTO> TopEntradas { get; set; } = new();
    }

}