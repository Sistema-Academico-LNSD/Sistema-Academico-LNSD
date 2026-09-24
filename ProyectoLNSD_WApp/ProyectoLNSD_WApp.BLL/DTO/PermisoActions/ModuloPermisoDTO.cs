namespace ProyectoLNSD_WApp.BLL.DTO.PermisoActions
{
    public class ModuloPermisoDTO
    {
        public int IdModulo { get; set; }

        public string NombreModulo { get; set; } = string.Empty;

        public bool PuedeVer { get; set; }

        public bool PuedeCrear { get; set; }

        public bool PuedeEditar { get; set; }

        public bool PuedeEliminar { get; set; }
    }
}