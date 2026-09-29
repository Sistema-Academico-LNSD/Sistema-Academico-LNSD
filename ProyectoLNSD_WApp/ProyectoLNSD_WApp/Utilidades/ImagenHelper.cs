namespace ProyectoLNSD_WApp.Utilidades
{
    // Reutilizable para logo (HU 01) y banners (HU 03).
    public static class ImagenHelper
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        public const long TamanoMaximoBytes = 2 * 1024 * 1024; // 2 MB

        // Devuelve (true, rutaPublica) o (false, mensajeDeError)
        public static async Task<(bool Ok, string Resultado)> GuardarAsync(
            IFormFile archivo, IWebHostEnvironment entorno, string carpeta)
        {
            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension) ||
                !archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return (false, "Formato no permitido. Use una imagen JPG, PNG o WEBP.");

            if (archivo.Length > TamanoMaximoBytes)
                return (false, "La imagen supera el tamaño máximo permitido de 2 MB.");

            string directorio = Path.Combine(entorno.WebRootPath, "uploads", carpeta);
            Directory.CreateDirectory(directorio);

            // Nombre generado por el servidor: nunca se usa el nombre que manda el usuario.
            string nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            await using var stream = new FileStream(Path.Combine(directorio, nombreArchivo), FileMode.Create);
            await archivo.CopyToAsync(stream);

            return (true, $"/uploads/{carpeta}/{nombreArchivo}");
        }

        // Borra del disco un archivo subido antes (al reemplazar o eliminar). No lanza error si no existe.
        public static void Eliminar(string? rutaPublica, IWebHostEnvironment entorno)
        {
            if (string.IsNullOrWhiteSpace(rutaPublica) || !rutaPublica.StartsWith("/uploads/"))
                return;

            string raiz = Path.GetFullPath(Path.Combine(entorno.WebRootPath, "uploads"));
            string archivo = Path.GetFullPath(Path.Combine(
                entorno.WebRootPath, rutaPublica.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            // Solo se borra dentro de wwwroot/uploads
            if (!archivo.StartsWith(raiz + Path.DirectorySeparatorChar))
                return;

            try
            {
                if (File.Exists(archivo))
                    File.Delete(archivo);
            }
            catch (IOException)
            {
                // Si el archivo está en uso no se interrumpe la operación
            }
        }
    }
}
