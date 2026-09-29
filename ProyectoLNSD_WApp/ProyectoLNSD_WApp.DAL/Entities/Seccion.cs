using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Seccion
    {
        public int IdSeccion { get; set; }
        public int IdGrado { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CapacidadMaxima { get; set; }
        public bool Estado { get; set; } = true;

        public virtual Grado? Grado { get; set; }
    }
}
