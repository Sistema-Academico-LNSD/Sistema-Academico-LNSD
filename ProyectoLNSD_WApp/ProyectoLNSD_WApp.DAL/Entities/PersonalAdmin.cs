namespace ProyectoLNSD_WApp.DAL.Entities

{
    public partial class PersonalAdmin
    {
        public int IdPerAdm { get; set; }

        public int IdUsuario { get; set; }

        public string Cargo { get; set; } = string.Empty;

        public string Departamento { get; set; } = string.Empty;

        public virtual Usuario? Usuario { get; set; }

        //public virtual ICollection<Inventario> Inventarios { get; set; }
        //    = new List<Inventario>();
    }
}