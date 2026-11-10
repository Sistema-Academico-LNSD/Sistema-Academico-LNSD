namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Listas para los filtros de la consulta. Salen de los horarios del período,
    /// así solo aparecen secciones, grupos y cursos que tienen clases.
    public class OpcionesAsistenciaDTO
    {
        public int? IdPeriodo { get; set; }

        public List<OpcionFiltroDTO> Periodos { get; set; } = new();

        public List<OpcionFiltroDTO> Grados { get; set; } = new();

        public List<SeccionHorarioDTO> Secciones { get; set; } = new();

        public List<OpcionFiltroDTO> Grupos { get; set; } = new();

        public List<OpcionFiltroDTO> Cursos { get; set; } = new();

        public decimal UmbralSugerido { get; set; }
    }

    /// Estado del cierre de asistencia de un período (MCAF-01-13)
    public class CierreAsistenciaDTO
    {
        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        public bool Cerrado { get; set; }

        public DateTime? FechaCierre { get; set; }

        public string? CerradoPor { get; set; }

        public bool PuedeCerrar { get; set; }

        /// Por qué todavía no se puede cerrar (si aplica)
        public string? Motivo { get; set; }
    }
}
