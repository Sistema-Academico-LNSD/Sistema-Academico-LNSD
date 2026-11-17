using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class CategoriaInventario
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public virtual ICollection<Inventario> Articulos { get; set; }
            = new List<Inventario>();
    }
}