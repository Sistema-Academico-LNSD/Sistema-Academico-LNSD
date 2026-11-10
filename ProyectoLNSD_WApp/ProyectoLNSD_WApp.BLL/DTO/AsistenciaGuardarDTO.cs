using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Lo que envía la pantalla al guardar la lista (MCAF-01-01, 03, 04 y 05)
    public class AsistenciaGuardarDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione la clase")]
        public int IdHorario { get; set; }

        [Required(ErrorMessage = "Seleccione la fecha")]
        public DateOnly? Fecha { get; set; }

        [MinLength(1, ErrorMessage = "La lista no tiene estudiantes")]
        public List<AsistenciaRegistroGuardarDTO> Registros { get; set; } = new();
    }

    public class AsistenciaRegistroGuardarDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Estudiante no válido")]
        public int IdMatricula { get; set; }

        [Required(ErrorMessage = "Marque el estado de asistencia de todos los estudiantes")]
        public string Estado { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La observación no puede superar 250 caracteres")]
        public string? Observacion { get; set; }
    }
}
