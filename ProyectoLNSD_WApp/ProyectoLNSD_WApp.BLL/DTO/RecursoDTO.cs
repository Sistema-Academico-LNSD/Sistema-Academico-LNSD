using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RecursoDTO
    {
        public int IdRecurso { get; set; }

        public int IdDocente { get; set; }

        public string NombreDocente { get; set; } = string.Empty;

        public int IdCurso { get; set; }

        public string CodigoCurso { get; set; } = string.Empty;

        public string NombreCurso { get; set; } = string.Empty;

        public int IdGrado { get; set; }

        public string NombreGrado { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string TipoRecurso { get; set; } = string.Empty;

        public string Modalidad { get; set; } = string.Empty;

        public string? NombreArchivo { get; set; }

        public string? Extension { get; set; }

        public long? TamanoBytes { get; set; }

        public string? UrlEnlace { get; set; }

        public DateTime FechaPublicacion { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }
}
