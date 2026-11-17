using System.Text.RegularExpressions;

namespace ProyectoLNSD_WApp.BLL.Service.Inventario

{
    public static class InventarioReglas
    {
        public const int MaxCantidad = 1_000_000;

        public const string EstadoActivo = "Activo";
        public const string EstadoInactivo = "Inactivo";

        public static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.Now);

        public static string? Limpiar(string? texto)
        {
            string limpio = (texto ?? string.Empty).Trim();
            return limpio.Length == 0 ? null : limpio;
        }

        public const int MaxExistencia = 100_000_000;

        /// Cédula nacional, DIMEX o pasaporte: letras, números y guiones.
        public static bool CedulaValida(string cedula) =>
            Regex.IsMatch(cedula, @"^[0-9A-Za-z-]{5,30}$");

    }
}