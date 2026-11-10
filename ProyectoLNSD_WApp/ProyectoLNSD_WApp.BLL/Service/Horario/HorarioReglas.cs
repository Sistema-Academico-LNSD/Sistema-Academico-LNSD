using System.Globalization;
using System.Text.RegularExpressions;

namespace ProyectoLNSD_WApp.BLL.Service.Horario
{
    public static class HorarioReglas
    {
        // Días lectivos (HU 01-02, esc. 4). El sistema no tiene una configuración de días
        // lectivos, así que se usan lunes a viernes. Si algún día se agrega esa opción en
        // Configuración, solo hay que cambiar esta lista.
        public static readonly string[] DiasLectivos =
            { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };

        private static readonly string[] FormatosHora = { "HH:mm", "HH:mm:ss" };

        // Devuelve el nombre del día tal como se guarda, o null si no es un día lectivo
        public static string? NormalizarDia(string? dia)
        {
            if (string.IsNullOrWhiteSpace(dia))
                return null;

            return DiasLectivos.FirstOrDefault(d =>
                string.Equals(d, dia.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Para mostrar los días en orden de semana (el nombre no se ordena bien como texto)
        public static int OrdenDia(string dia)
        {
            int posicion = Array.IndexOf(DiasLectivos, dia);

            return posicion < 0 ? DiasLectivos.Length : posicion;
        }

        // Acepta "08:00" (lo que envía el campo de hora del navegador) o "08:00:00"
        public static bool TryLeerHora(string? texto, out TimeOnly hora)
        {
            return TimeOnly.TryParseExact(
                texto?.Trim(),
                FormatosHora,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out hora);
        }

        public static string FormatearHora(TimeOnly hora) => hora.ToString("HH:mm");

        // "Aula  101 " y "aula 101" cuentan como la misma aula
        public static string NormalizarAula(string? aula)
        {
            return Regex.Replace((aula ?? string.Empty).Trim(), @"\s+", " ");
        }
    }
}