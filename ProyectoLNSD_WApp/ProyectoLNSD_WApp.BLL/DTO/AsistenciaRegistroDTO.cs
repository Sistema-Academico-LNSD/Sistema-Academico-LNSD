namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class AsistenciaRegistroDTO
    {
        public int IdAsistencia { get; set; }

        public DateOnly Fecha { get; set; }

        public string Dia { get; set; } = string.Empty;

        public string HoraInicio { get; set; } = string.Empty;

        public string HoraFin { get; set; } = string.Empty;

        public int IdCurso { get; set; }

        public string NombreCurso { get; set; } = string.Empty;

        public string NombreGrado { get; set; } = string.Empty;

        public string NombreSeccion { get; set; } = string.Empty;

        public string NombreGrupo { get; set; } = string.Empty;

        public string NombreDocente { get; set; } = string.Empty;

        public int IdMatricula { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string? Observacion { get; set; }

        public bool Modificado { get; set; }
    }
}
