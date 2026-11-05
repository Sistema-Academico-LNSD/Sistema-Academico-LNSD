using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    // Horario de una matrícula vigente: un estudiante, su sección y grupo, y sus clases
    public class HorarioMatriculaDTO
    {
        public int IdMatricula { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string NombrePeriodo { get; set; } = string.Empty;

        public string NombreGrado { get; set; } = string.Empty;

        public string NombreSeccion { get; set; } = string.Empty;

        public string NombreGrupo { get; set; } = string.Empty;

        public List<HorarioDTO> Horarios { get; set; } = new();
    }
}
