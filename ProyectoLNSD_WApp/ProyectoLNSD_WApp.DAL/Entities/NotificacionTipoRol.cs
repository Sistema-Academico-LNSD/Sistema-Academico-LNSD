namespace ProyectoLNSD_WApp.DAL.Entities;

public class NotificacionTipoRol
{
    public int IdTipo { get; set; }
    public string Rol { get; set; } = string.Empty;
    public virtual NotificacionTipo? Tipo { get; set; }
}
