using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Horario
{
    public class HorarioFiltro
    {
        public int? IdPeriodo { get; set; }

        public int? IdGrado { get; set; }

        public int? IdSeccion { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdCurso { get; set; }

        public int? IdDocente { get; set; }

        public string? Dia { get; set; }

        // Texto parcial del aula
        public string? Aula { get; set; }

        // true = solo activos, false = solo inactivos, null = todos
        public bool? Estado { get; set; }

        // Para el horario de un estudiante: solo los cursos en los que está matriculado
        public List<int>? IdsCursos { get; set; }
    }
}

