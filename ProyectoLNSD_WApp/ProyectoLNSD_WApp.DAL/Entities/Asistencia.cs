namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Registro de asistencia de un estudiante en una lección (horario) de un día.
    public class Asistencia
    {
        public int IdAsistencia { get; set; }
        public int IdEstudiante { get; set; }
        public int IdHorario { get; set; }
        public DateOnly Fecha { get; set; }

        /// "Ausente" es el único estado que resta a la nota de asistencia.
        public string Estado { get; set; } = string.Empty;

        public string? Observacion { get; set; }
    }
}
