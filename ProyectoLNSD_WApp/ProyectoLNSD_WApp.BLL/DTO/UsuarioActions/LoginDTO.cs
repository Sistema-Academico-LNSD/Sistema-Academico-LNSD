using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO.UsuarioActions
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
}