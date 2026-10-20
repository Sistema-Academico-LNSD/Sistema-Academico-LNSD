namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class MatriculaCurso
    {
        public int IdMatricula { get; set; }
        public int IdCurso { get; set; }

        public virtual Matricula? Matricula { get; set; }
        public virtual Curso? Curso { get; set; }
    }
}
