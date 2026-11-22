namespace ProyectoLNSD_WApp.BLL.DTO;

public class NotificacionListaDTO
{
    public long IdNotificacion { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Leida { get; set; }
    public DateTime? FechaLectura { get; set; }
}

public class NotificacionPanelDTO
{
    public int NoLeidas { get; set; }
    public List<NotificacionListaDTO> Notificaciones { get; set; } = new();
}

public class NotificacionTipoDTO
{
    public int IdTipo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class NotificacionTipoGuardarDTO
{
    public int IdTipo { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool Activo { get; set; } = true;
}
