namespace ProyectoLNSD_WApp.DAL.Entities
{
    // Roles que pueden ver cada acceso rápido (relación muchos a muchos)
    public partial class AccesoRapidoRol
    {
        public int IdAcceso { get; set; }
        public int IdRol { get; set; }

        public virtual AccesoRapido? Acceso { get; set; }
        public virtual Rol? Rol { get; set; }
    }
}
