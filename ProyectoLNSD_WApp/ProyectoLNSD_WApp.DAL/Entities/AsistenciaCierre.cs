namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Cierre del registro de asistencia de un período (MCAF-01-13).
    /// Si existe la fila, el período está cerrado y no se puede registrar ni modificar.
    public class AsistenciaCierre
    {
        public int IdPeriodo { get; set; }

        public DateTime FechaCierre { get; set; }

        public int IdUsuario { get; set; }

        public virtual PeriodoLectivo? Periodo { get; set; }

        public virtual Usuario? Usuario { get; set; }
    }
}
