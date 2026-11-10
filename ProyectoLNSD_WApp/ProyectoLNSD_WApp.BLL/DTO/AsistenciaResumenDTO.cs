namespace ProyectoLNSD_WApp.BLL.DTO
{
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

        public decimal Porcentaje { get; set; }

        public bool BajoUmbral { get; set; }
    }
}
