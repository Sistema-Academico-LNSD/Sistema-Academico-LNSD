using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class RecursoAcademico
    {
        public int IdRecurso { get; set; }

        public int IdDocente { get; set; }

        public int IdCurso { get; set; }

        public int IdGrado { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string TipoRecurso { get; set; } = string.Empty;

        public string Modalidad { get; set; } = ModalidadRecurso.Archivo;

        // Solo para modalidad Archivo
        public string? NombreArchivoOriginal { get; set; }

        public string? RutaArchivo { get; set; }

        public string? Extension { get; set; }

        public long? TamanoBytes { get; set; }

        public string? HashArchivo { get; set; }

        // Solo para modalidad Enlace
        public string? UrlEnlace { get; set; }

        public DateTime FechaPublicacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public virtual Docente? Docente { get; set; }

        public virtual Curso? Curso { get; set; }

        public virtual Grado? Grado { get; set; }
    }

    /// Modalidades permitidas (deben coincidir con el CHECK de la tabla Recursos_Academicos).
    public static class ModalidadRecurso
    {
        public const string Archivo = "Archivo";
        public const string Enlace = "Enlace";
    }
}
