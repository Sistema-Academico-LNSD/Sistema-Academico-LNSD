namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class AsignacionCursoDTO
    {
        public int IdDocenteCurso { get; set; }
        public int IdCurso { get; set; }
        public string CodigoCurso { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string? NombreArea { get; set; }
        public int IdPeriodo { get; set; }
        public string NombrePeriodo { get; set; } = string.Empty;
        public bool PeriodoActivo { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }
}