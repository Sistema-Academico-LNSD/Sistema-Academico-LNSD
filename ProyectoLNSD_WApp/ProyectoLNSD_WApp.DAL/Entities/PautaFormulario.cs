namespace ProyectoLNSD_WApp.DAL.Entities;

/// Un formulario del MEP (N°1 a N°12) ya llenado. Los valores van en Datos (JSON).
public partial class PautaFormulario
{
    public int IdFormulario { get; set; }
    public int IdExpediente { get; set; }
    public byte NumeroFormulario { get; set; }
    public short Secuencia { get; set; } = 1;
    public string Estado { get; set; } = "Borrador";          // Borrador | EnElaboracion | Finalizado
    public string Datos { get; set; } = "{}";
    public int IdUsuarioActualiza { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }

    public virtual PautaExpediente? Expediente { get; set; }
    public virtual Usuario? UsuarioActualiza { get; set; }
}
