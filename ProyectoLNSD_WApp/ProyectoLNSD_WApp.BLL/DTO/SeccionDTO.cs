using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class SeccionDTO
    {
        public int IdSeccion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un grado")]
        public int IdGrado { get; set; }

        [Required(ErrorMessage = "El nombre de la sección es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 200, ErrorMessage = "La capacidad máxima debe ser mayor a 0")]
        public int CapacidadMaxima { get; set; }

        public bool Estado { get; set; } = true;

        public string? CodigoGrado { get; set; }
        public string? NombreGrado { get; set; }
        public string? Nivel { get; set; }
        public int CuposDisponibles { get; set; }
        public bool EstadoGrado { get; set; }
    }
}