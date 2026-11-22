namespace ProyectoLNSD_WApp.DAL.Entities;

public class NotificacionTipo
{
    public int IdTipo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public virtual ICollection<NotificacionTipoRol> RolesDestinatarios { get; set; } = new List<NotificacionTipoRol>();
}
