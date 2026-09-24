namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class RolPermiso
    {
        public int IdRolPermiso { get; set; }
        public int IdRol { get; set; }
        public int IdModulo { get; set; }
        public bool PuedeVer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public virtual Rol? Rol { get; set; }
        public virtual Modulo? Modulo { get; set; }
    }
}