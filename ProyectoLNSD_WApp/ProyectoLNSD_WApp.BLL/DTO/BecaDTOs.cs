using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RequisitoBecaDTO
    {
        public int IdRequisito { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Obligatorio { get; set; }

        public string? Verificacion { get; set; }

        public bool Automatico => Verificacion != null;

        public bool Activo { get; set; }
    }

    public class RequisitoBecaGuardarDTO
    {
        public int IdRequisito { get; set; }

        [Required(ErrorMessage = "Indique el nombre del requisito")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string? Descripcion { get; set; }

        public bool Obligatorio { get; set; } = true;

        /// Vacío = manual
        [StringLength(30)]
        public string? Verificacion { get; set; }
    }

    public class RequisitoCumplimientoDTO
    {
        public int IdRequisito { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Obligatorio { get; set; }

        public bool Automatico { get; set; }

        public bool Activo { get; set; }

        public bool Cumplido { get; set; }
    }

    public class BecaListaDTO
    {
        public int IdBeca { get; set; }

        public int IdEstudiante { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Identificacion { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        public string Grado { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string EstadoVisible { get; set; } = string.Empty;

        public string Vigencia { get; set; } = string.Empty;

        public bool CumpleRequisitos { get; set; }

        public List<string> RequisitosFaltantes { get; set; } = new();

        public string? MotivoDesactivacion { get; set; }

        public bool Editable { get; set; }
    }

    public class BecaDetalleDTO
    {
        public int IdBeca { get; set; }

        public int IdEstudiante { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Identificacion { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string Grado { get; set; } = string.Empty;

        public string? EstadoMatricula { get; set; }

        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        public DateOnly PeriodoInicio { get; set; }

        public DateOnly PeriodoFin { get; set; }

        public decimal? Monto { get; set; }

        public DateOnly? FechaInicio { get; set; }

        public DateOnly? FechaFin { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string EstadoVisible { get; set; } = string.Empty;

        public string? Vigencia { get; set; }

        public string? Observacion { get; set; }

        public string? MotivoDesactivacion { get; set; }

        public List<RequisitoCumplimientoDTO> Requisitos { get; set; } = new();

        public bool CumpleRequisitos { get; set; }

        public bool Editable { get; set; }

        public string? MotivoNoEditable { get; set; }

        public string? RegistradaPor { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string? ModificadaPor { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public DateTime? FechaActivacion { get; set; }

        public DateTime? FechaDesactivacion { get; set; }
    }

    public class BecaGuardarDTO
    {
        public int IdBeca { get; set; }

        public int IdEstudiante { get; set; }

        public int IdPeriodo { get; set; }

        public decimal? Monto { get; set; }

        public DateOnly? FechaInicio { get; set; }

        public DateOnly? FechaFin { get; set; }

        [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres")]
        public string? Observacion { get; set; }

        public List<int> IdsRequisitosCumplidos { get; set; } = new();

        public bool Activar { get; set; }
    }

    public class BecaFiltroDTO
    {
        public int? IdPeriodo { get; set; }

        public int? IdGrado { get; set; }

        public string? Texto { get; set; }

        public string? Estado { get; set; }
    }

    public class PeriodoBecaDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public bool Activo { get; set; }

        public bool Finalizado { get; set; }
    }

    public class OpcionesBecaDTO
    {
        public int? IdPeriodoActivo { get; set; }

        public List<PeriodoBecaDTO> Periodos { get; set; } = new();

        public List<OpcionFiltroDTO> Grados { get; set; } = new();

        public List<string> Estados { get; set; } = new();
    }

    public class EstudianteBecaDTO
    {
        public int IdEstudiante { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Identificacion { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string Grado { get; set; } = string.Empty;
    }

    public class HistorialBecaDTO
    {
        public int IdEstudiante { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Identificacion { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public decimal MontoTotalActivas { get; set; }

        public List<BecaListaDTO> Becas { get; set; } = new();
    }

    public class MiBecaDTO
    {
        public string NombrePeriodo { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public string Vigencia { get; set; } = string.Empty;

        public int? DiasRestantes { get; set; }
    }

    public class MisBecasEstudianteDTO
    {
        public string NombreEstudiante { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string Grado { get; set; } = string.Empty;

        public List<MiBecaDTO> Becas { get; set; } = new();
    }
}
