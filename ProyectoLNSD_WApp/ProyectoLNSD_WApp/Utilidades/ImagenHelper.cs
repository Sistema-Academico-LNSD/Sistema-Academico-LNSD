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
    }
}
