namespace ProyectoLNSD_WApp.DAL.Entities

{
    public partial class Encargado
    {
        public int IdEncargado { get; set; }

        public int IdUsuario { get; set; }

        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<EncargadoEstudiante> EncargadosEstudiantes { get; set; }
            = new List<EncargadoEstudiante>();
    }
}