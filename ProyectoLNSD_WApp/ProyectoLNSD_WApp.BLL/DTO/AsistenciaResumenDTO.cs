namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Asistencia de un estudiante en un curso: conteos y porcentaje (MCAF-01-10 y 01-12)
    public class AsistenciaResumenDTO
    {
        public int IdMatricula { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string NombreGrado { get; set; } = string.Empty;

        public string NombreSeccion { get; set; } = string.Empty;

        public string NombreGrupo { get; set; } = string.Empty;

        public int IdCurso { get; set; }

        public string NombreCurso { get; set; } = string.Empty;

        public int Presentes { get; set; }

        public int Ausentes { get; set; }

        public int Tardias { get; set; }

        public int Justificadas { get; set; }

        public int Total { get; set; }

        /// 0 a 100, con un decimal
        public decimal Porcentaje { get; set; }

        /// true si el porcentaje está por debajo del umbral de alerta
        public bool BajoUmbral { get; set; }
    }
}
