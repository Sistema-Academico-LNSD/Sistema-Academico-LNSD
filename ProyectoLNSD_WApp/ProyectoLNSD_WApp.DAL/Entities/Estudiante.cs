namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Estudiante
    {
        public int IdEstudiante { get; set; }

        public int IdUsuario { get; set; }

        public string Identificacion { get; set; } = string.Empty;

        public virtual Usuario? Usuario { get; set; }

        //public virtual ICollection<Beca> Becas { get; set; }
        //    = new List<Beca>();

        //public virtual ICollection<Matricula> Matriculas { get; set; }
        //    = new List<Matricula>();

        //public virtual ICollection<Nota> Notas { get; set; }
        //    = new List<Nota>();

        //public virtual ICollection<Asistencia> Asistencias { get; set; }
        //    = new List<Asistencia>();

        public virtual ICollection<EstudianteDocente> EstudiantesDocentes { get; set; }
            = new List<EstudianteDocente>();

        public virtual ICollection<EncargadoEstudiante> EncargadosEstudiantes { get; set; }
            = new List<EncargadoEstudiante>();
    }
}