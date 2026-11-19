using ProyectoLNSD_WApp.DAL.Entities;
using ProyectoLNSD_WApp.DAL.Repositories.Beca;
using PeriodoEntity = ProyectoLNSD_WApp.DAL.Entities.PeriodoLectivo;

namespace ProyectoLNSD_WApp.BLL.Service.Beca
{
    public static class BecaReglas
    {
        public const int MaxObservacion = 500;

        public const int MaxMotivo = 500;

        public const decimal MontoMaximo = 9_999_999_999.99m;

        public const string MatriculaActiva = "Activa";

        public const string VigenciaVigente = "Vigente";
        public const string VigenciaPorIniciar = "Por iniciar";
        public const string VigenciaVencida = "Vencida";

        public static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.Now);

        public static bool PeriodoFinalizado(PeriodoEntity periodo, DateOnly hoy) => periodo.FechaFin < hoy;

        public static string EstadoVisible(string estado, DateOnly fechaFin, DateOnly hoy) =>
            estado == EstadoBeca.Activa && fechaFin < hoy ? EstadoVisibleBeca.Vencida : estado;

        public static string Vigencia(DateOnly inicio, DateOnly fin, DateOnly hoy) =>
            hoy < inicio ? VigenciaPorIniciar
            : hoy > fin ? VigenciaVencida
            : VigenciaVigente;

        public static string? ValidarVigencia(DateOnly? inicio, DateOnly? fin, PeriodoEntity periodo)
        {
            if (inicio == null || fin == null)
                return "Indique la fecha de inicio y la de finalización de la beca";

            if (fin < inicio)
                return "La fecha de finalización no puede ser anterior a la de inicio";

            if (inicio < periodo.FechaInicio || fin > periodo.FechaFin)
                return $"La vigencia debe estar dentro del período {periodo.Nombre} " +
                       $"({periodo.FechaInicio:dd/MM/yyyy} al {periodo.FechaFin:dd/MM/yyyy}); " +
                       "una beca dura como máximo un año lectivo";

            return null;
        }

        public static string? ValidarMonto(decimal? monto)
        {
            if (monto == null || monto <= 0)
                return "Indique un monto de beca mayor que cero";

            if (monto > MontoMaximo)
                return "El monto de la beca no es válido";

            if (decimal.Round(monto.Value, 2) != monto.Value)
                return "El monto admite como máximo dos decimales";

            return null;
        }

        public static string? Limpiar(string? texto)
        {
            string limpio = (texto ?? string.Empty).Trim();

            return limpio.Length == 0 ? null : limpio;
        }
    }
}
