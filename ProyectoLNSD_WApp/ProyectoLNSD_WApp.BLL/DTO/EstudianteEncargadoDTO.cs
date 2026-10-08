namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// <summary>Información básica del estudiante que ve su encargado (MESF-01-12).</summary>
    public class EstudianteEncargadoDTO
    {
        public int IdEstudiante { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public DateOnly? FechaNacimiento { get; set; }
        public DateOnly FechaIngreso { get; set; }
        public string? NombreGrado { get; set; }
        public string EstadoAcademico { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string Parentesco { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
    }
}
