namespace ProyectoLNSD_WApp.DAL.Repositories.Asistencia
{
    /// Criterios de búsqueda de asistencia. Se acumulan: solo quedan los registros que cumplen todos.
    public class AsistenciaFiltro
    {
        public int? IdPeriodo { get; set; }

        public int? IdGrado { get; set; }

        public int? IdSeccion { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdCurso { get; set; }

        /// Limita a las clases de un docente (lo usa el servicio cuando consulta un docente)
        public int? IdDocente { get; set; }

        public int? IdMatricula { get; set; }

        /// Para estudiante y encargado: solo sus matrículas
        public List<int>? IdsMatriculas { get; set; }

        /// Nombre, apellido, carné o identificación del estudiante
        public string? Texto { get; set; }

        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        public string? Estado { get; set; }

        /// true = sin las presencias (solo ausencias, tardías y justificadas)
        public bool SoloIncidencias { get; set; }
    }

    /// Conteo de estados de un estudiante (matrícula) en un curso. Lo calcula SQL Server.
    public class ConteoAsistencia
    {
        public int IdMatricula { get; set; }

        public int IdCurso { get; set; }

        public int Presentes { get; set; }

        public int Ausentes { get; set; }

        public int Tardias { get; set; }

        public int Justificadas { get; set; }
    }
}
