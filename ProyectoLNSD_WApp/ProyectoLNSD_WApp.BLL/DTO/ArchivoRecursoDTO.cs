using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class ArchivoRecursoDTO
    {
        public string NombreOriginal { get; set; } = string.Empty;

        public long Tamano { get; set; }

        public Stream Contenido { get; set; } = Stream.Null;
    }
}
