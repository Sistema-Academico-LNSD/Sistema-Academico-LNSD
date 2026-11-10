using ProyectoLNSD_WApp.BLL.Service.Horario;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Asistencia
{
    /// Reglas del módulo en un solo lugar. Si la institución cambia el criterio
    /// del porcentaje o el umbral, solo se toca este archivo.
    public static class AsistenciaReglas
    {
        /// Porcentaje por debajo del cual un estudiante se marca como "bajo" en los reportes.
        /// PENDIENTE DE CONFIRMAR con la coordinación académica (norma MEP vigente).
        public const decimal UmbralSugerido = 80m;

        public const int MaxObservacion = 250;

        /// MCAF-01-10. Criterio actual (PENDIENTE DE CONFIRMAR):
        /// - Presente y Justificada cuentan como asistencia.
        /// - Tardía cuenta como asistencia (se reporta aparte).
        /// - Solo la ausencia sin justificar baja el porcentaje.
        /// Porcentaje = (total - ausentes) / total * 100, con un decimal. Sin registros = null.
        public static decimal? CalcularPorcentaje(int presentes, int ausentes, int tardias, int justificadas)
        {
            int total = presentes + ausentes + tardias + justificadas;

            if (total == 0)
                return null;

            return Math.Round((total - ausentes) * 100m / total, 1);
        }

        /// Acepta "tardía", "TARDIA", etc. y devuelve el valor tal como se guarda, o null si no es válido
        public static string? NormalizarEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return null;

            string limpio = estado.Trim().Replace("í", "i").Replace("Í", "I");

            return EstadoAsistencia.Todos.FirstOrDefault(e =>
                string.Equals(e, limpio, StringComparison.OrdinalIgnoreCase));
        }

        /// Texto para mostrar ("Tardia" se guarda sin tilde por el CHECK de la tabla)
        public static string EtiquetaEstado(string estado) =>
            estado == EstadoAsistencia.Tardia ? "Tardía" : estado;

        /// Nombre del día como lo guarda Horario ("Lunes" ... "Viernes"); null si es fin de semana
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
