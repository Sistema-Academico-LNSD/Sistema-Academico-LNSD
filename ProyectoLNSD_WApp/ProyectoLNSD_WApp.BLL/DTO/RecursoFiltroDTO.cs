using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RecursoFiltroDTO
    {
        public int? IdCurso { get; set; }

        public int? IdGrado { get; set; }

        // Lo usarán el administrador y el director; el docente siempre consulta solo los suyos
        public int? IdDocente { get; set; }

        public string? Titulo { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        // "fecha_desc" (por defecto), "fecha_asc", "titulo" o "curso"
        public string? Orden { get; set; }
    }
}
