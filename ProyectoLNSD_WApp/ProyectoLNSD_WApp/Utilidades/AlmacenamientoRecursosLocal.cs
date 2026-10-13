using System.Security.Cryptography;
using ProyectoLNSD_WApp.BLL.Storage;

namespace ProyectoLNSD_WApp.Utilidades
{
    // Guarda los recursos FUERA de wwwroot (en App_Data/recursos), así nadie puede
    // descargarlos escribiendo la URL: siempre se pasa por el controller, que valida permisos.
    public class AlmacenamientoRecursosLocal : IAlmacenamientoRecursos
    {
        private readonly string _raiz;

        public AlmacenamientoRecursosLocal(IWebHostEnvironment entorno, IConfiguration configuracion)
        {
            // Opcional: "Recursos:RutaAlmacenamiento" en appsettings para cambiar la carpeta
            string? configurada = configuracion["Recursos:RutaAlmacenamiento"];

            _raiz = Path.GetFullPath(string.IsNullOrWhiteSpace(configurada)
                ? Path.Combine(entorno.ContentRootPath, "App_Data", "recursos")
                : configurada);

            Directory.CreateDirectory(_raiz);
        }

        public async Task<ArchivoGuardado> GuardarAsync(Stream contenido, string extension)
        {
            // Nombre generado por el servidor: nunca se usa el que manda el usuario
            string nombre = $"{Guid.NewGuid():N}{extension}";
            string destino = Path.Combine(_raiz, nombre);

            using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long total = 0;

            await using (var salida = new FileStream(destino, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[81920];
                int leidos;

                // Se copia y se calcula el hash en la misma lectura
                while ((leidos = await contenido.ReadAsync(buffer)) > 0)
                {
                    sha.AppendData(buffer, 0, leidos);
                    await salida.WriteAsync(buffer.AsMemory(0, leidos));
                    total += leidos;
                }
            }

            string hash = Convert.ToHexString(sha.GetHashAndReset()).ToLowerInvariant();

            return new ArchivoGuardado(nombre, hash, total);
        }

        public Stream? Abrir(string ruta)
        {
            string? completa = Resolver(ruta);

            if (completa == null || !File.Exists(completa))
                return null;

            return new FileStream(completa, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Eliminar(string ruta)
        {
            string? completa = Resolver(ruta);

            if (completa == null)
                return;

            try
            {
                if (File.Exists(completa))
                    File.Delete(completa);
            }
            catch (IOException)
            {
                // Si el archivo está en uso no se interrumpe la operación
            }
        }

        // Solo acepta nombres simples dentro de la carpeta de recursos (evita "..\..\")
        private string? Resolver(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta) || ruta != Path.GetFileName(ruta))
                return null;

            string completa = Path.GetFullPath(Path.Combine(_raiz, ruta));

            return completa.StartsWith(_raiz + Path.DirectorySeparatorChar)
                ? completa
                : null;
        }
    }
}
