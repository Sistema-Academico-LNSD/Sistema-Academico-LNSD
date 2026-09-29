using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class AccesoRapidoDTO
    {
        public int IdAcceso { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres")]
        public string? Descripcion { get; set; }

        // Nombre de un ícono de Bootstrap Icons, por ejemplo: bi-people
        [StringLength(50, ErrorMessage = "El ícono no puede superar los 50 caracteres")]
        public string? Icono { get; set; }

        [Required(ErrorMessage = "El enlace es requerido")]
        [StringLength(250, ErrorMessage = "El enlace no puede superar los 250 caracteres")]
        public string Enlace { get; set; } = string.Empty;

        public int Orden { get; set; }

        public bool Activo { get; set; } = true;

        // Roles que ven este acceso en su página de inicio (entrada del formulario)
        public List<int> IdsRoles { get; set; } = new();

        // Solo lectura: nombres de esos roles, para mostrarlos en la lista
        public List<string> NombresRoles { get; set; } = new();
    }

    // Opción de la lista de roles del formulario
    public class RolOpcionDTO
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
