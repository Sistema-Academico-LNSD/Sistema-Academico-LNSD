namespace ProyectoLNSD_WApp.DAL.Entities;

public partial class PautaBitacora
{
    public int IdBitacora { get; set; }
    public int IdExpediente { get; set; }
    public int? IdFormulario { get; set; }
    public int IdUsuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; }

    public virtual PautaExpediente? Expediente { get; set; }
    public virtual PautaFormulario? Formulario { get; set; }
    public virtual Usuario? Usuario { get; set; }
}
