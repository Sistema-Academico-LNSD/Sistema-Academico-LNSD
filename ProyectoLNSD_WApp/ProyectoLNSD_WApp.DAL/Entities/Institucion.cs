using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities;

public partial class Institucion
{
    public int IdInstitucion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? TelefonoSecundario { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? RutaLogo { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
