namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Datos institucionales que se imprimen en el encabezado de cada formulario (MPPAF-01-12 y 01-15).
    public class InstitucionDocumentoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? RutaLogo { get; set; }
        public string? CodigoPresupuestario { get; set; }
        public string? CircuitoEscolar { get; set; }
        public string? Dre { get; set; }
        public string? NombreDirector { get; set; }
    }

    public class FormularioDocumentoDTO
    {
        public int IdFormulario { get; set; }
        public int NumeroFormulario { get; set; }
        public int Secuencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public PlantillaPautaDTO Plantilla { get; set; } = new();
        public string Datos { get; set; } = "{}";
    }

    /// Todo lo necesario para imprimir uno o varios formularios de un expediente.
    public class DocumentoPautaDTO
    {
        public InstitucionDocumentoDTO Institucion { get; set; } = new();
        public string CodigoExpediente { get; set; } = string.Empty;
        public int NumeroPauta { get; set; }
        public string NombrePauta { get; set; } = string.Empty;
        public string EstadoExpediente { get; set; } = string.Empty;
        public DateOnly FechaActivacion { get; set; }
        public string Responsable { get; set; } = string.Empty;
        public string GeneradoPor { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public bool EsExpedienteCompleto { get; set; }
        public List<FormularioDocumentoDTO> Formularios { get; set; } = new();
    }
}
