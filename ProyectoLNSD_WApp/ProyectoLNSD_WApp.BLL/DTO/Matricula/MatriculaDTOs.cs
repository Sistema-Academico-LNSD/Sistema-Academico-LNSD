using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO.Matricula;

public class MatriculaCrearDTO
{
    [Range(1, int.MaxValue)] public int IdEstudiante { get; set; }
    [Range(1, int.MaxValue)] public int IdPeriodo { get; set; }
    [Range(1, int.MaxValue)] public int IdGrupo { get; set; }
    [Range(1, int.MaxValue)] public int IdSeccion { get; set; }
    public DateOnly? FechaMatricula { get; set; }
    [MaxLength(500)] public string? Observaciones { get; set; }
    public List<int> IdCursos { get; set; } = new();
}

public class MatriculaActualizarDTO
{
    [Range(1, int.MaxValue)] public int IdPeriodo { get; set; }
    [Range(1, int.MaxValue)] public int IdGrupo { get; set; }
    [Range(1, int.MaxValue)] public int IdSeccion { get; set; }
    public DateOnly? FechaMatricula { get; set; }
    [MaxLength(500)] public string? Observaciones { get; set; }
    public List<int> IdCursos { get; set; } = new();
}

public class MatriculaEstadoDTO
{
    [Required, RegularExpression("^(Activa|Inactiva|Anulada|Pendiente)$", ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = "Activa";
    [MaxLength(500)] public string? Motivo { get; set; }
}

public class MatriculaTrasladoDTO
{
    [Range(1, int.MaxValue)] public int IdGrupo { get; set; }
    [Range(1, int.MaxValue)] public int IdSeccion { get; set; }
    [Required, MaxLength(500)] public string Motivo { get; set; } = string.Empty;
}

public class MatriculaObservacionCrearDTO
{
    [Required, MinLength(3), MaxLength(1000)] public string Observacion { get; set; } = string.Empty;
}

public class MatriculaFiltroDTO
{
    public int? IdEstudiante { get; set; }
    public string? Identificacion { get; set; }
    public int? IdGrado { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdSeccion { get; set; }
    public int? IdPeriodo { get; set; }
    public string? Estado { get; set; }
    public string? Texto { get; set; }
    [Range(1, 1000)] public int Limite { get; set; } = 100;
}

public class MatriculaCursoDTO
{
    public int IdCurso { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class MatriculaDetalleDTO
{
    public int IdMatricula { get; set; }
    public int IdEstudiante { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Carnet { get; set; } = string.Empty;
    public int IdPeriodo { get; set; }
    public string NombrePeriodo { get; set; } = string.Empty;
    public int IdGrupo { get; set; }
    public string NombreGrupo { get; set; } = string.Empty;
    public int IdSeccion { get; set; }
    public string NombreSeccion { get; set; } = string.Empty;
    public int? IdGrado { get; set; }
    public string NombreGrado { get; set; } = string.Empty;
    public string EncargadoPrincipal { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateOnly FechaMatricula { get; set; }
    public string? Observaciones { get; set; }
    public List<MatriculaCursoDTO> Cursos { get; set; } = new();
}

public class MatriculaHistorialDTO
{
    public int IdHistorial { get; set; }
    public int IdMatricula { get; set; }
    public int? IdUsuario { get; set; }
    public string TipoCambio { get; set; } = string.Empty;
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public int? GrupoAnterior { get; set; }
    public int? GrupoNuevo { get; set; }
    public int? SeccionAnterior { get; set; }
    public int? SeccionNueva { get; set; }
    public string? Detalle { get; set; }
    public DateTime FechaCambio { get; set; }
}

public class MatriculaObservacionDTO
{
    public int IdObservacion { get; set; }
    public int IdMatricula { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public DateOnly FechaRegistro { get; set; }
}

public class MatriculaReporteDTO
{
    public int Total { get; set; }
    public int Activas { get; set; }
    public int Inactivas { get; set; }
    public int Anuladas { get; set; }
    public int Pendientes { get; set; }
    public List<MatriculaDetalleDTO> Matriculas { get; set; } = new();
}
