namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class MatriculaObservacion
    {
        public int IdObservacion { get; set; }
        public int IdMatricula { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public DateOnly FechaRegistro { get; set; }

        public virtual Matricula? Matricula { get; set; }
    }
}
