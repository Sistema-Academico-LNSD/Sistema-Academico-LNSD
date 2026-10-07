namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class CursosDocenteDTO
    {
        public int IdDocente { get; set; }
        public string NombreDocente { get; set; } = string.Empty;
        public bool DocenteActivo { get; set; }
        public string? NombrePeriodoActivo { get; set; }
        public List<AsignacionCursoDTO> Actuales { get; set; } = new();
        public List<AsignacionCursoDTO> Historial { get; set; } = new();
    }
}