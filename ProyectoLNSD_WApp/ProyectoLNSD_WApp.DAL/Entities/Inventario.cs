using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Inventario
    {
        public int IdInventario { get; set; }

        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public int Cantidad { get; set; }

        public string UnidadMedida { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public DateOnly? FechaVencimiento { get; set; }

        public int CantidadMinima { get; set; }

        public bool Activo { get; set; }

        public virtual CategoriaInventario? Categoria { get; set; }

        public virtual ICollection<InventarioMovimiento> Movimientos { get; set; }
            = new List<InventarioMovimiento>();
    }
}