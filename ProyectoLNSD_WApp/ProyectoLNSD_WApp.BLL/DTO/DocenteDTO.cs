using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class DocenteDTO
    {
        public int IdDocente { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(150, ErrorMessage = "Los apellidos no pueden superar los 150 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación es requerida")]
        [StringLength(30, ErrorMessage = "La identificación no puede superar los 30 caracteres")]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "La identificación solo puede contener letras, números y guiones")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no tiene un formato válido")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(300, ErrorMessage = "La dirección no puede superar los 300 caracteres")]
        public string? Direccion { get; set; }

        [StringLength(1000, ErrorMessage = "Los títulos no pueden superar los 1000 caracteres")]
        public string? Titulos { get; set; }

        [StringLength(150, ErrorMessage = "La especialidad no puede superar los 150 caracteres")]
        public string? Especialidad { get; set; }

        [Range(0, 60, ErrorMessage = "Los años de experiencia deben estar entre 0 y 60")]
        public int? AniosExperiencia { get; set; }
        public int? IdArea { get; set; }
        public bool Estado { get; set; }
        public string? NombreArea { get; set; }
        public bool TieneCuenta { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}