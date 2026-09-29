using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    // Una fila de Contenido_Sitio (Mision, Vision, Historia; luego Banner y Bloque).
    public class ContenidoSitioDTO
    {
        public int IdContenido { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es requerido")]
        public string Tipo { get; set; } = string.Empty;

        // Para Misión/Visión/Historia lo asigna el servidor; se deja nullable para que el binding no lo exija.
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres")]
        public string? Titulo { get; set; }

        [StringLength(5000, ErrorMessage = "El texto no puede superar los 5000 caracteres")]
        public string? Descripcion { get; set; }

        public string? RutaImagen { get; set; }
        public int Orden { get; set; }

        // Solo lectura desde la vista: lo define el botón (Guardar borrador / Publicar).
        public string? Estado { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}

