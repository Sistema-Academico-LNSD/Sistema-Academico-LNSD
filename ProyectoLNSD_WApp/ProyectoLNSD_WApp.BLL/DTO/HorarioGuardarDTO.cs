using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class HorarioGuardarDTO
    {
        public int IdHorario { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una sección")]
        public int IdSeccion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un grupo")]
        public int IdGrupo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un curso")]
        public int IdCurso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un docente")]
        public int IdDocente { get; set; }

        [Required(ErrorMessage = "Seleccione el día")]
        [StringLength(15, ErrorMessage = "El día no es válido")]
        public string Dia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique la hora de inicio")]
        [StringLength(8, ErrorMessage = "La hora de inicio no es válida")]
        public string HoraInicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique la hora de finalización")]
        [StringLength(8, ErrorMessage = "La hora de finalización no es válida")]
        public string HoraFin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique el aula")]
        [StringLength(50, ErrorMessage = "El aula no puede superar los 50 caracteres")]
        public string Aula { get; set; } = string.Empty;
    }
}