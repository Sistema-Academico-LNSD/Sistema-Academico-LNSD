namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Cada registro o cambio de una nota (Registro, Edicion, Correccion).
    public class NotaHistorial
    {
        public int IdHistorial { get; set; }
        public int IdNota { get; set; }
        public int IdUsuario { get; set; }
        public string TipoCambio { get; set; } = string.Empty;
        public decimal? ValorAnterior { get; set; }
        public decimal ValorNuevo { get; set; }
        public string? Observacion { get; set; }
        public DateTime FechaCambio { get; set; }

        public virtual Nota? Nota { get; set; }
    }
}
