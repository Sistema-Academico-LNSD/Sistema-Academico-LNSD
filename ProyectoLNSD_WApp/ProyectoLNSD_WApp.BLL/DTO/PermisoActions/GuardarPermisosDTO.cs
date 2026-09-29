using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO.PermisoActions
{
    public class GuardarPermisosDTO
    {
        [Required]
        public int IdRol { get; set; }

        public List<ModuloPermisoDTO> Permisos { get; set; } = new();
    }
}