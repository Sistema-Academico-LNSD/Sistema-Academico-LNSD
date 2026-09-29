using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RolDTO
    {
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede superar los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres")]
        public string? Descripcion { get; set; }
    }
}