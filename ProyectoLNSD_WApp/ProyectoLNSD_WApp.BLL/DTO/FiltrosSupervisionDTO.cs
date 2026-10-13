using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class FiltrosSupervisionDTO
    {
        public List<OpcionFiltroDTO> Docentes { get; set; } = new();

        public List<OpcionFiltroDTO> Cursos { get; set; } = new();
    }

    public class OpcionFiltroDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}
