namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class CursoGrado
    {
        public int IdCurso { get; set; }

        public int IdGrado { get; set; }

        public virtual Curso? Curso { get; set; }

        public virtual Grado? Grado { get; set; }
    }
}