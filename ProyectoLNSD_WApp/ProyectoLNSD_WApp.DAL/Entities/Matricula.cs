namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class Matricula
    {
        public int IdMatricula { get; set; }
        public int IdEstudiante { get; set; }
        public int IdPeriodo { get; set; }
        public int IdGrupo { get; set; }
        public int IdSeccion { get; set; }
        public DateOnly FechaMatricula { get; set; }
        public string Estado { get; set; } = "Activa";
        public string? Observaciones { get; set; }

        public virtual Estudiante? Estudiante { get; set; }
        public virtual PeriodoLectivo? PeriodoLectivo { get; set; }
        public virtual Grupo? Grupo { get; set; }
        public virtual Seccion? Seccion { get; set; }
        public virtual ICollection<MatriculaCurso> MatriculasCursos { get; set; } = new List<MatriculaCurso>();
        public virtual ICollection<MatriculaObservacion> ObservacionesRegistradas { get; set; } = new List<MatriculaObservacion>();
        public virtual ICollection<MatriculaHistorial> Historial { get; set; } = new List<MatriculaHistorial>();
    }
}
