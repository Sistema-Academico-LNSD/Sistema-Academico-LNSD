namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class AreaAcademica
    {
        public int IdArea { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;

        public virtual ICollection<Curso> Cursos { get; set; }
            = new List<Curso>();
    }
}