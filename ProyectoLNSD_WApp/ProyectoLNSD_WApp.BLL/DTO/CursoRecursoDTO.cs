using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class CursoRecursoDTO
    {
        public int IdCurso { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public List<GradoRecursoDTO> Grados { get; set; } = new();
    }

    public class GradoRecursoDTO
    {
        public int IdGrado { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}
