namespace ProyectoLNSD_WApp.DAL.Entities;

/// Expediente de una pauta activada: agrupa los formularios (MPPAF-01-02, 01-10, 01-14).
public partial class PautaExpediente
{
    public int IdExpediente { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public byte NumeroPauta { get; set; }
    public int IdUsuarioResponsable { get; set; }
    public string Estado { get; set; } = "Abierto";          // Abierto | Cerrado
    public DateOnly FechaActivacion { get; set; }
    public int? IdExpedienteOrigen { get; set; }

    // Cierre
    public DateTime? FechaCierre { get; set; }
    public string? NombreCierre { get; set; }
    public string? VistoBuenoDirector { get; set; }
    public bool DevolucionRealizada { get; set; }
    public string? ObservacionCierre { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public virtual Usuario? UsuarioResponsable { get; set; }
    public virtual PautaExpediente? ExpedienteOrigen { get; set; }
    public virtual ICollection<PautaExpedienteEstudiante> Estudiantes { get; set; } = new List<PautaExpedienteEstudiante>();
    public virtual ICollection<PautaFormulario> Formularios { get; set; } = new List<PautaFormulario>();
}
