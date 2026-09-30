namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class Curso
    {
        public int IdCurso { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int IdArea { get; set; }

        public bool Estado { get; set; } = true;

        public virtual AreaAcademica? Area { get; set; }

        public virtual ICollection<CursoGrado> CursosGrados { get; set; }
            = new List<CursoGrado>();
    }
}