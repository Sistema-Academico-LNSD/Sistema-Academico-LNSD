using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities;

public partial class ContenidoSitio
{
    public int IdContenido { get; set; }
    public string Tipo { get; set; } = string.Empty;      // Mision, Vision, Historia, Banner, Bloque
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? RutaImagen { get; set; }
    public int Orden { get; set; }
    public string Estado { get; set; } = "Borrador";      // Borrador, Publicado
    public DateTime? FechaPublicacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public int? IdUsuarioModifica { get; set; }
}
