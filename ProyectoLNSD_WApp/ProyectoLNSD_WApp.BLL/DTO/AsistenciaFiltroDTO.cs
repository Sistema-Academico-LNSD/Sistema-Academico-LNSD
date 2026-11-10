namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Filtros de la consulta y los reportes de asistencia (MCAF-01-09 y 01-12)
    public class AsistenciaFiltroDTO
    {
        /// Sin valor = período activo
        public int? IdPeriodo { get; set; }

        public int? IdGrado { get; set; }

        public int? IdSeccion { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdCurso { get; set; }

        /// Nombre, apellido, carné o identificación del estudiante
        public string? Texto { get; set; }

        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        /// Solo para el detalle: Presente, Ausente, Tardia o Justificada
        public string? Estado { get; set; }

        /// Porcentaje mínimo esperado; los que estén por debajo se marcan
        public decimal? Umbral { get; set; }

        /// true = el resumen muestra solo los que están por debajo del umbral
        public bool SoloBajoUmbral { get; set; }
    }
}
