using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class TiqueteDTO
    {
        public int IdTiquete { get; set; }

        public int IdUsuario { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaUtilizacion { get; set; }

        // Datos del dueño: AutoMapper los llena solo cuando el tiquete se consulta con su usuario
        public string? UsuarioNombre { get; set; }

        public string? UsuarioApellido { get; set; }

        public string? UsuarioCorreo { get; set; }
    }
}
