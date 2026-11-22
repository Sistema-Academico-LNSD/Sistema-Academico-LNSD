namespace ProyectoLNSD_WApp.DAL.Entities;

public partial class PautaExpedienteEstudiante
{
    public int IdPautaEstudiante { get; set; }
    public int IdExpediente { get; set; }
    public int? IdEstudiante { get; set; }                    // null si no está matriculado
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Seccion { get; set; }
    public string Rol { get; set; } = string.Empty;           // Recibe | Ejerce | Observador | Identificada

    public virtual PautaExpediente? Expediente { get; set; }
    public virtual Estudiante? Estudiante { get; set; }
}
