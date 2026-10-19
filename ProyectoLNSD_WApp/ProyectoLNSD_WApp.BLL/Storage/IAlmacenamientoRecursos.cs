using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.Storage
{
    public interface IAlmacenamientoRecursos
    {
        // Guarda el contenido con un nombre generado por el servidor y devuelve
        // la ruta interna, el hash SHA-256 y el tamaño real guardado.
        Task<ArchivoGuardado> GuardarAsync(Stream contenido, string extension);

        // Devuelve null si el archivo no existe o la ruta no es válida
        Stream? Abrir(string ruta);

        void Eliminar(string ruta);
    }

    public record ArchivoGuardado(string Ruta, string Hash, long Tamano);
}
