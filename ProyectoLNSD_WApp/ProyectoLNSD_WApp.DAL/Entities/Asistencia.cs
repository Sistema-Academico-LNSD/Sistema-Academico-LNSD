namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Asistencia de un estudiante (su matrícula) a una clase del horario en una fecha (MCAF-01).
    /// La clase ya define período, curso, docente, sección (grado) y grupo.
    public class Asistencia
    {
        public int IdAsistencia { get; set; }

        public int IdHorario { get; set; }

        public int IdMatricula { get; set; }

        public DateOnly Fecha { get; set; }

        /// Uno de EstadoAsistencia: Presente, Ausente, Tardia, Justificada
        public string Estado { get; set; } = EstadoAsistencia.Presente;

        public string? Observacion { get; set; }

        public int IdUsuarioRegistra { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? IdUsuarioModifica { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public virtual Horario? Horario { get; set; }

        public virtual Matricula? Matricula { get; set; }
    }

    /// Valores permitidos por el CHECK de la tabla (CK_Asistencia_Estado)
    public static class EstadoAsistencia
    {
        public const string Presente = "Presente";
        public const string Ausente = "Ausente";
        public const string Tardia = "Tardia";
        public const string Justificada = "Justificada";

        public static readonly string[] Todos = { Presente, Ausente, Tardia, Justificada };
    }
}
