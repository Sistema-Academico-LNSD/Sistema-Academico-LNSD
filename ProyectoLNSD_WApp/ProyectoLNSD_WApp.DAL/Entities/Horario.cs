using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Horario
    {
        public int IdHorario { get; set; }

        public int IdPeriodo { get; set; }

        public int IdSeccion { get; set; }

        public int IdGrupo { get; set; }

        public int IdCurso { get; set; }

        public int IdDocente { get; set; }

        public string Dia { get; set; } = string.Empty;

        public TimeOnly HoraInicio { get; set; }

        public TimeOnly HoraFin { get; set; }

        public string Aula { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;

        public virtual PeriodoLectivo? Periodo { get; set; }

        public virtual Seccion? Seccion { get; set; }

        public virtual Grupo? Grupo { get; set; }

        public virtual Curso? Curso { get; set; }

        public virtual Docente? Docente { get; set; }
    }
}
