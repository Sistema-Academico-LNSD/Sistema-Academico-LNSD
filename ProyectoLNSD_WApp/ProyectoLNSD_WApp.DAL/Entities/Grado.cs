using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Grado
    {
        public int IdGrado { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;

        public virtual ICollection<Seccion> Secciones { get; set; } = new List<Seccion>();
    }
}