using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Inventario
{
    public class InventarioFiltro
    {
        public string? Texto { get; set; }

        public int? IdCategoria { get; set; }

        public bool? Activo { get; set; }
        public bool SoloBajoMinimo { get; set; }
    }
}
