namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Modulo
    {
        public int IdModulo { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public virtual ICollection<RolPermiso> RolesPermisos { get; set; }
            = new List<RolPermiso>();
    }
}