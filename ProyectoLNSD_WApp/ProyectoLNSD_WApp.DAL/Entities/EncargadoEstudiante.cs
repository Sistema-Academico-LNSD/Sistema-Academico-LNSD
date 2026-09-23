namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class EncargadoEstudiante
    {
        public int IdEncargado { get; set; }

        public int IdEstudiante { get; set; }

        public string Parentesco { get; set; } = string.Empty;

        public virtual Encargado? Encargado { get; set; }

        public virtual Estudiante? Estudiante { get; set; }
    }
}
