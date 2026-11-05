using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class HorarioFiltroDTO
    {
        public int? IdPeriodo { get; set; }

        public int? IdGrado { get; set; }

        public int? IdSeccion { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdCurso { get; set; }

        public int? IdDocente { get; set; }

        public string? Dia { get; set; }

        public string? Aula { get; set; }

        // true = activos, false = inactivos, sin valor = todos
        public bool? Estado { get; set; }
    }
}
