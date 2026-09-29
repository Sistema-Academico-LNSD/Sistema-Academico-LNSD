using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO.UsuarioActions
{
    public class UsuarioCrearDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}