using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class GradoDTO
    {
        public int IdGrado { get; set; }

        [Required(ErrorMessage = "El código del grado es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede superar los 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del grado es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel académico es requerido")]
        [StringLength(50)]
        public string Nivel { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;
    }
}
