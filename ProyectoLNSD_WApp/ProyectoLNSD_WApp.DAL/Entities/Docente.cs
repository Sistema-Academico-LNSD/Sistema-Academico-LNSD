namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Docente
    {
        public int IdDocente { get; set; }

        public int IdUsuario { get; set; }

        public string? Especialidad { get; set; }

        public virtual Usuario? Usuario { get; set; }

        //public virtual ICollection<Horario> Horarios { get; set; }
        //    = new List<Horario>();

        public virtual ICollection<EstudianteDocente> EstudiantesDocentes { get; set; }
            = new List<EstudianteDocente>();
    }
}