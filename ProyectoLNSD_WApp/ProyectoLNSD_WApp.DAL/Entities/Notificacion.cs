namespace ProyectoLNSD_WApp.DAL.Entities;

public class Notificacion
{
    public long IdNotificacion { get; set; }
    public int IdUsuario { get; set; }
    public int IdTipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public string ClaveEvento { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Leida { get; set; }
    public DateTime? FechaLectura { get; set; }
    public virtual Usuario? Usuario { get; set; }
    public virtual NotificacionTipo? Tipo { get; set; }
}
