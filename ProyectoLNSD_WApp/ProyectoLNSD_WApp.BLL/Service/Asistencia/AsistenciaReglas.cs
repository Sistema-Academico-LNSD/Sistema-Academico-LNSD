using ProyectoLNSD_WApp.BLL.Service.Horario;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Asistencia
{
    public static class AsistenciaReglas
    {

        public const decimal UmbralSugerido = 80m;

        public const int MaxObservacion = 250;

        public static decimal? CalcularPorcentaje(int presentes, int ausentes, int tardias, int justificadas)
        {
            int total = presentes + ausentes + tardias + justificadas;

            if (total == 0)
                return null;

            return Math.Round((total - ausentes) * 100m / total, 1);
        }

        public static string? NormalizarEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return null;

            string limpio = estado.Trim().Replace("í", "i").Replace("Í", "I");

            return EstadoAsistencia.Todos.FirstOrDefault(e =>
                string.Equals(e, limpio, StringComparison.OrdinalIgnoreCase));
        }

        public static string EtiquetaEstado(string estado) =>
            estado == EstadoAsistencia.Tardia ? "Tardía" : estado;

        public static string? NombreDia(DateOnly fecha)
        {
            int indice = fecha.DayOfWeek switch
            {
                DayOfWeek.Monday => 0,
                DayOfWeek.Tuesday => 1,
                DayOfWeek.Wednesday => 2,
                DayOfWeek.Thursday => 3,
                DayOfWeek.Friday => 4,
                _ => -1
            };

            return indice < 0 || indice >= HorarioReglas.DiasLectivos.Length
                ? null
                : HorarioReglas.DiasLectivos[indice];
        }

        public static string? LimpiarObservacion(string? texto)
        {
            string limpio = (texto ?? string.Empty).Trim();

            return limpio.Length == 0 ? null : limpio;
        }

        public static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.Now);
    }
}
