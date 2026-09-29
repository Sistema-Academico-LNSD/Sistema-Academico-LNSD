using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities;

public partial class AccesoRapido
{
    public int IdAcceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string Enlace { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; }
    public int? IdModulo { get; set; }
}
