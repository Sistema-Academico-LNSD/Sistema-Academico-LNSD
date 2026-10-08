using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// <summary>Encargado vinculado a un estudiante (una fila de la tabla).</summary>
    public class EncargadoVinculoDTO
    {
        public int IdEncargado { get; set; }
        public int IdEstudiante { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Parentesco { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
    }

    /// <summary>Resultado de la búsqueda de encargados existentes.</summary>
    public class EncargadoBusquedaDTO
    {
        public int IdEncargado { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class VincularEncargadoDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Estudiante inválido")]
        public int IdEstudiante { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un encargado")]
        public int IdEncargado { get; set; }

        [Required(ErrorMessage = "El parentesco es requerido")]
        public string Parentesco { get; set; } = string.Empty;
    }

    public class CrearEncargadoDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Estudiante inválido")]
        public int IdEstudiante { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "El teléfono no tiene un formato válido")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El parentesco es requerido")]
        public string Parentesco { get; set; } = string.Empty;

        /// <summary>Opcional. Si se deja vacía, el encargado define la suya con "Olvidé mi contraseña".</summary>
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? Password { get; set; }
    }
}
