namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class Grupo
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int Anio { get; set; }

        public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}
