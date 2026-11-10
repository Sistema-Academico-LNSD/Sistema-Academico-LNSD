namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Beca socioeconómica de un estudiante en un período lectivo (MBEF-01).
    /// Una sola por estudiante y período; su vigencia cabe dentro del período.
    public class Beca
    {
        public int IdBeca { get; set; }

        public int IdEstudiante { get; set; }

        public int IdPeriodo { get; set; }

        public decimal Monto { get; set; }

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        /// Uno de EstadoBeca: Pendiente, Activa, Inactiva
        public string Estado { get; set; } = EstadoBeca.Pendiente;

        public string? Observacion { get; set; }

        public string? MotivoDesactivacion { get; set; }

        public DateTime? FechaActivacion { get; set; }

        public DateTime? FechaDesactivacion { get; set; }

        public int IdUsuarioRegistra { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? IdUsuarioModifica { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public virtual Estudiante? Estudiante { get; set; }

        public virtual PeriodoLectivo? Periodo { get; set; }

        public virtual Usuario? UsuarioRegistra { get; set; }

        public virtual Usuario? UsuarioModifica { get; set; }

        public virtual ICollection<BecaRequisitoCumplido> RequisitosCumplidos { get; set; }
            = new List<BecaRequisitoCumplido>();
    }

    /// Valores permitidos por el CHECK de la tabla (CK_Beca_Estado)
    public static class EstadoBeca
    {
        public const string Pendiente = "Pendiente";
        public const string Activa = "Activa";
        public const string Inactiva = "Inactiva";
    }
}
