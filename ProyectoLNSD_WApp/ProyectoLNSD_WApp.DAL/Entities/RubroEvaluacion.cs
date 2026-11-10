namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Rubro fijo de evaluación (MEP): trabajo cotidiano, pruebas y tareas.
    public class RubroEvaluacion
    {
        public int IdRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public int Orden { get; set; }

        /// Rango permitido para el valor (puntos) de cada actividad del rubro; null = sin límite.
        public decimal? PuntosMinimo { get; set; }
        public decimal? PuntosMaximo { get; set; }

        /// Cantidad fija de actividades del rubro (pruebas: 4); null = la define el docente.
        public int? CantidadActividades { get; set; }

        public bool Estado { get; set; } = true;
    }
}
