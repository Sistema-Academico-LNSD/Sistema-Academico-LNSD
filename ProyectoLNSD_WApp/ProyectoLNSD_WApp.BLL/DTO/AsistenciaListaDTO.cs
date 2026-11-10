namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class AsistenciaListaDTO
    {
        public HorarioDTO Clase { get; set; } = new();

        public DateOnly Fecha { get; set; }

        public bool YaRegistrada { get; set; }

        public bool PuedeGuardar { get; set; }

        public string? Aviso { get; set; }

        public List<AsistenciaFilaDTO> Estudiantes { get; set; } = new();
    }

    public class AsistenciaFilaDTO
    {
        public int IdMatricula { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string? Estado { get; set; }

        public string? Observacion { get; set; }
    }
}
