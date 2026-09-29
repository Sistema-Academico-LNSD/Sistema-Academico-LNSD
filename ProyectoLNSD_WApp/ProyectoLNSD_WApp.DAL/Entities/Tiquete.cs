using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Tiquete
    {
        public int IdTiquete { get; set; }

        public int IdUsuario { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Estado { get; set; } = EstadoTiquete.Disponible;

        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaUtilizacion { get; set; }

        public int? IdUsuarioValidador { get; set; }

        // Dueño del tiquete
        public virtual Usuario? Usuario { get; set; }

        // Encargado que lo marcó como utilizado
        public virtual Usuario? UsuarioValidador { get; set; }
    }

    /// Estados permitidos (deben coincidir con el CHECK de la tabla Tiquete).
    public static class EstadoTiquete
    {
        public const string Disponible = "Disponible";
        public const string Utilizado = "Utilizado";
    }
}
