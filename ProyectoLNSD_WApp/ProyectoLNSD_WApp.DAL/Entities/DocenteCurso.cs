namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class DocenteCurso
    {
        public int IdDocenteCurso { get; set; }
        public int IdDocente { get; set; }
        public int IdCurso { get; set; }
        public int IdPeriodo { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public virtual Docente? Docente { get; set; }
        public virtual Curso? Curso { get; set; }
        public virtual PeriodoLectivo? Periodo { get; set; }
    }
}