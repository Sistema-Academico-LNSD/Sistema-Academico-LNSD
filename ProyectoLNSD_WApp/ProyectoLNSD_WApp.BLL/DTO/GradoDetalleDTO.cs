using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class GradoDetalleDTO
    {
        public GradoDTO Grado { get; set; } = new();
        public List<SeccionDTO> Secciones { get; set; } = new();
    }
}