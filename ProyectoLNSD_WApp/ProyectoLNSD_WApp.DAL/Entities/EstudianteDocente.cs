namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class EstudianteDocente
    {
        public int IdEstudiante { get; set; }
        public int IdDocente { get; set; }
        public virtual Estudiante? Estudiante { get; set; }
        public virtual Docente? Docente { get; set; }
    }
}