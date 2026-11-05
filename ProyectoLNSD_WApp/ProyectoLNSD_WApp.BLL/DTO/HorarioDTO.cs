using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class HorarioDTO
    {
        public int IdHorario { get; set; }

        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        public int IdGrado { get; set; }

        public string NombreGrado { get; set; } = string.Empty;

        public int IdSeccion { get; set; }

        public string NombreSeccion { get; set; } = string.Empty;

        public int IdGrupo { get; set; }

        public string NombreGrupo { get; set; } = string.Empty;

        public int IdCurso { get; set; }

        public string CodigoCurso { get; set; } = string.Empty;

        public string NombreCurso { get; set; } = string.Empty;

        public int IdDocente { get; set; }

        public string NombreDocente { get; set; } = string.Empty;

        public string Dia { get; set; } = string.Empty;

        // "08:00"
        public string HoraInicio { get; set; } = string.Empty;

        public string HoraFin { get; set; } = string.Empty;

        public string Aula { get; set; } = string.Empty;

        public bool Estado { get; set; }
    }
}
