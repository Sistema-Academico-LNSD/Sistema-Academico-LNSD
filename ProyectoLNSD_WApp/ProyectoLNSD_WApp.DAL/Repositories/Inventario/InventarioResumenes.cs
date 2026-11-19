using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Inventario
{
    public class ResumenMensualMovimiento
    {
        public int Anio { get; set; }

        public int Mes { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public int Movimientos { get; set; }
    }

    public class ResumenArticuloMovimiento
    {
        public int IdInventario { get; set; }

        public string Articulo { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string UnidadMedida { get; set; } = string.Empty;

        public int Movimientos { get; set; }

        public long Unidades { get; set; }
    }
}
