using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class FiltrosMaterialDTO
    {
        public int IdGrado { get; set; }

        public string NombreGrado { get; set; } = string.Empty;

        public List<CursoRecursoDTO> Cursos { get; set; } = new();
    }
}
