using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO.Curso
{
    public class CursoDTO
    {
        public int IdCurso { get; set; }

        [Required(ErrorMessage = "El código del curso es requerido")]
        [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del curso es requerido")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del curso es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un área académica")]
        public int IdArea { get; set; }

        public bool Estado { get; set; } = true;

        public List<int> IdGrados { get; set; } = new();
    }
}