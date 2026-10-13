using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.Service.Recurso
{
    public static class RecursoReglas
    {
        // Límite de tamaño por archivo (HU 01-01, escenario 6)
        public const long TamanoMaximoBytes = 10 * 1024 * 1024;

        // Word, Excel, PowerPoint y PDF (HU 01-01)
        public static readonly string[] ExtensionesPermitidas =
            { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };

        // Tipos válidos según la modalidad (HU 01-02, escenario 3)
        public static readonly string[] TiposArchivo =
            { "Documento", "Presentación", "Hoja de cálculo", "PDF" };

        public static readonly string[] TiposEnlace =
            { "Video", "Sitio web", "Plataforma educativa" };

        // Tipo de contenido con el que se entrega cada formato permitido
        public static string ObtenerContentType(string? extension) =>
            extension?.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };

    }
}
