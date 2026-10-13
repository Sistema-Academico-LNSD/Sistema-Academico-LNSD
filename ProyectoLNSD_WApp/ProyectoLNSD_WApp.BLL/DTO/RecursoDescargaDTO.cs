using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RecursoDescargaDTO
    {
        public Stream Contenido { get; set; } = Stream.Null;

        public string NombreArchivo { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/octet-stream";
    }
}
