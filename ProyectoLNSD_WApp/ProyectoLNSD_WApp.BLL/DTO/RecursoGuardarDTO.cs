using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RecursoGuardarDTO
    {
        public int IdRecurso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un curso")]
        public int IdCurso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un grado")]
        public int IdGrado { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Seleccione el tipo de recurso")]
        [StringLength(30, ErrorMessage = "El tipo de recurso no es válido")]
        public string TipoRecurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique si el recurso es un archivo o un enlace")]
        [StringLength(10, ErrorMessage = "La modalidad no es válida")]
        public string Modalidad { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "El enlace no puede superar los 500 caracteres")]
        public string? UrlEnlace { get; set; }

        // Si no se indica, se usa la fecha de hoy
        public DateTime? FechaPublicacion { get; set; }
    }
}