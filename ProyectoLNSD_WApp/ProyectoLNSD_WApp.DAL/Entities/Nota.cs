namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Puntos obtenidos por un estudiante (matrícula) en una actividad.
    public class Nota
    {
        public int IdNota { get; set; }
        public int IdMatricula { get; set; }
        public int IdActividad { get; set; }
        public decimal PuntosObtenidos { get; set; }
        public string? Observacion { get; set; }
        public int IdUsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public virtual Matricula? Matricula { get; set; }
        public virtual ActividadEvaluacion? Actividad { get; set; }
    }
}
