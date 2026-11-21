namespace ProyectoLNSD_WApp.DAL.Repositories.Pauta
{
    /// Filtros para listar expedientes (MPPAF-01-10). IdUsuarioResponsable = null significa "todos".
    public class PautaExpedienteFiltro
    {
        public int? IdUsuarioResponsable { get; set; }
        public string? Estado { get; set; }              // Abierto | Cerrado
        public byte? NumeroPauta { get; set; }
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        public string? TextoEstudiante { get; set; }     // nombre o parte del nombre
        public int? IdEstudiante { get; set; }
    }
}
