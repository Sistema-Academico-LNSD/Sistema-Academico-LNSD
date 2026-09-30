using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO.Curso
{
    public class AreaAcademicaDTO
    {
        public int IdArea { get; set; }

        [Required(ErrorMessage = "El nombre del área es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;
    }
}