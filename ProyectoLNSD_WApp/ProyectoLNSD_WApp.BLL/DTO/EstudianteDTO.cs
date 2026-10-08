using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// <summary>
    /// Expediente del estudiante. Nombre, apellidos y correo viven en Usuario;
    /// el resto es información complementaria del estudiante.
    /// </summary>
    public class EstudianteDTO
    {
        public int IdEstudiante { get; set; }
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación es requerida")]
        [StringLength(30, ErrorMessage = "La identificación no puede superar los 30 caracteres")]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "La identificación solo puede contener letras, números y guiones")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateOnly? FechaNacimiento { get; set; }

        [Phone(ErrorMessage = "El teléfono no tiene un formato válido")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(300, ErrorMessage = "La dirección no puede superar los 300 caracteres")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "El correo de emergencia no tiene un formato válido")]
        [StringLength(150, ErrorMessage = "El correo de emergencia no puede superar los 150 caracteres")]
        public string? CorreoEmergencia { get; set; }

        // ---- Información académica (MESF-01-02) ----
        [Required(ErrorMessage = "El grado de referencia es requerido")]
        public int? IdGrado { get; set; }

        [Required(ErrorMessage = "El estado académico es requerido")]
        public string EstadoAcademico { get; set; } = "Regular";

        [Required(ErrorMessage = "La fecha de ingreso es requerida")]
        public DateOnly? FechaIngreso { get; set; }

        // ---- Información médica y de atención (MESF-01-05) ----
        [StringLength(500, ErrorMessage = "Las alergias no pueden superar los 500 caracteres")]
        public string? Alergias { get; set; }

        [StringLength(1000, ErrorMessage = "Las observaciones médicas no pueden superar los 1000 caracteres")]
        public string? ObservacionesMedicas { get; set; }

        [StringLength(1000, ErrorMessage = "Las adecuaciones no pueden superar los 1000 caracteres")]
        public string? AdecuacionesEducativas { get; set; }

        // ---- Solo lectura ----
        public string Carnet { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string? NombreGrado { get; set; }
    }

    /// <summary>Registro de un estudiante nuevo: incluye la contraseña inicial de su cuenta.</summary>
    public class EstudianteCrearDTO : EstudianteDTO
    {
        [Required(ErrorMessage = "La contraseña inicial es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
