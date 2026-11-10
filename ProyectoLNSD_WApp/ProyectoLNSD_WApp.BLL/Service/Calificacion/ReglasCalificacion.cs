using Ent = ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    /// Reglas de cálculo compartidas por la planilla del docente y los resultados del estudiante.
    public static class ReglasCalificacion
    {
        // El año lectivo tiene dos períodos (semestres) y los porcentajes de los rubros son de la nota anual
        public const int PeriodosPorAnio = 2;

        /// Peso de cada actividad (en % de la nota anual):
        ///  - rubro de peso manual (pruebas): el que definió el docente
        ///  - los demás: la parte que aporta el semestre entre las actividades del semestre, en partes iguales
        public static Dictionary<int, decimal> CalcularPesos(
            IEnumerable<Ent.RubroEvaluacion> rubros, IEnumerable<Ent.ActividadEvaluacion> actividades)
        {
            var pesos = new Dictionary<int, decimal>();
            var lista = actividades.ToList();

            foreach (var rubro in rubros)
            {
                var delRubro = lista.Where(a => a.IdRubro == rubro.IdRubro).ToList();
                int partes = rubro.CantidadActividades ?? delRubro.Count;
                decimal cuotaPeriodo = rubro.Porcentaje / PeriodosPorAnio;

                foreach (var actividad in delRubro)
                    pesos[actividad.IdActividad] = rubro.PesoManual
                        ? actividad.Porcentaje ?? 0m
                        : (partes > 0 ? cuotaPeriodo / partes : 0m);
            }

            return pesos;
        }
    }
}
