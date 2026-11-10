namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Autorización del administrador o director para corregir UNA nota de un período cerrado.
    public class CorreccionAutorizada
    {
        public int IdAutorizacion { get; set; }
        public int IdNota { get; set; }
        public int IdUsuarioAutoriza { get; set; }
        public string Motivo { get; set; } = string.Empty;

        /// true cuando el docente ya hizo el cambio (la autorización sirve una sola vez).
        public bool Usada { get; set; }

        public DateTime FechaAutorizacion { get; set; }
        public DateTime? FechaUso { get; set; }

        public virtual Nota? Nota { get; set; }
    }
}
