using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class InstitucionDTO
    {
        public int IdInstitucion { get; set; }

        [Required(ErrorMessage = "El nombre de la institución es requerido")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono principal es requerido")]
        [StringLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres")]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El teléfono secundario no puede superar los 50 caracteres")]
        public string? TelefonoSecundario { get; set; }

        [Required(ErrorMessage = "El correo institucional es requerido")]
        [EmailAddress(ErrorMessage = "El correo institucional no tiene un formato válido")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        // Solo lectura desde la vista: la ruta la asigna el servidor al subir el logo.
        public string? RutaLogo { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}
