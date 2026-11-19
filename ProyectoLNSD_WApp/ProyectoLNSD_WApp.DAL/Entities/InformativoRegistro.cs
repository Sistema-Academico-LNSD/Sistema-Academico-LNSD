namespace ProyectoLNSD_WApp.DAL.Entities;

/// Registro unificado del módulo informativo: Anuncio, Actividad de calendario o Evento.
public class InformativoRegistro
{
    public int IdInformativo { get; set; }
    public string Tipo { get; set; } = "Anuncio";
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public DateTime? FechaVigencia { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
    public string? Ubicacion { get; set; }
    public string? Categoria { get; set; }
    public string Estado { get; set; } = "Borrador";
    public bool Eliminado { get; set; }
    public int? IdUsuarioCrea { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
