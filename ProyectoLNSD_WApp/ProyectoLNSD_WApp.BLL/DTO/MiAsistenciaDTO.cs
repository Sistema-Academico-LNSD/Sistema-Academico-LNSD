namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Asistencia propia (estudiante) o de los estudiantes asociados (encargado), MCAF-01-07 y 01-08
    public class MiAsistenciaDTO
    {
        public int? IdPeriodo { get; set; }

        /// Períodos en los que la persona tiene matrícula
        public List<OpcionFiltroDTO> Periodos { get; set; } = new();

        public List<AsistenciaMatriculaDTO> Estudiantes { get; set; } = new();
    }

    public class AsistenciaMatriculaDTO
    {
        public int IdMatricula { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string NombrePeriodo { get; set; } = string.Empty;

        public string NombreGrado { get; set; } = string.Empty;

        public string NombreSeccion { get; set; } = string.Empty;

        public string NombreGrupo { get; set; } = string.Empty;

        /// Un resumen por curso
        public List<AsistenciaResumenDTO> Cursos { get; set; } = new();

        /// Ausencias, tardías y justificaciones (las presencias no se listan)
        public List<AsistenciaRegistroDTO> Incidencias { get; set; } = new();
    }
}
