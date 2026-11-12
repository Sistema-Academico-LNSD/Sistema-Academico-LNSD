namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Configuración de calificaciones (una sola fila).
    public class ConfiguracionCalificacion
    {
        public int IdConfig { get; set; }
        public decimal NotaMinima { get; set; } = 70m;
        public bool ConsiderarAsistencia { get; set; } = true;

        /// Lo que vale la asistencia en la nota anual (MEP: 5%).
        public decimal PorcentajeAsistencia { get; set; } = 5m;
    }
}
