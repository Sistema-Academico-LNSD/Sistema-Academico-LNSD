using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class InventarioMovimiento
    {
        public int IdMovimiento { get; set; }

        public int IdInventario { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public int ExistenciaResultante { get; set; }

        public DateTime Fecha { get; set; }

        public string? Motivo { get; set; }

        public string? NombreResponsable { get; set; }

        public string? CedulaResponsable { get; set; }

        public int IdUsuarioRegistra { get; set; }

        public virtual Inventario? Inventario { get; set; }

        public virtual Usuario? UsuarioRegistra { get; set; }
    }

    public static class TipoMovimientoInventario
    {
        public const string Entrada = "Entrada";
        public const string Salida = "Salida";
    }
}