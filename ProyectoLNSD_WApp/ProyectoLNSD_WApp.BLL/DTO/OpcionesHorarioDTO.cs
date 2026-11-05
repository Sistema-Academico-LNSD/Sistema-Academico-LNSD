using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class OpcionesHorarioDTO
    {
        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        public List<SeccionHorarioDTO> Secciones { get; set; } = new();

        public List<string> Dias { get; set; } = new();
    }

    public class SeccionHorarioDTO
    {
        public int IdSeccion { get; set; }

        public int IdGrado { get; set; }

        public string NombreGrado { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
    }
}
