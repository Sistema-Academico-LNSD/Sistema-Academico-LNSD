namespace ProyectoLNSD_WApp.BLL.DTO;

public class InformativoGuardarDTO
{
    public int IdInformativo { get; set; }
    public string Tipo { get; set; } = "Anuncio";
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public DateTime? FechaVigencia { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? HoraInicio { get; set; }
    public string? HoraFin { get; set; }
    public string? Ubicacion { get; set; }
    public string? Categoria { get; set; }
    public string Estado { get; set; } = "Publicado";
    public string Recurrencia { get; set; } = "Ninguna";
    public DateTime? RepetirHasta { get; set; }
    public bool ConfirmarDuplicado { get; set; }
}
