using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Calificacion;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using Ent = ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    /// Calcula el promedio del semestre, la nota del año y el resultado (aprueba o reprueba) de un estudiante.
    /// Es la única fuente de estos cálculos: la usan la planilla del docente y las consultas de estudiante,
    /// encargado, administrador y director.
    public class ResultadoCalificacionService : IResultadoCalificacionService
    {
        private readonly ICalificacionRepository _calificacionRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;

        // Datos ya cargados durante la solicitud (el servicio es Scoped)
        private readonly Dictionary<(int, int, int, int), DatosClase> _clases = new();
        private List<Ent.RubroEvaluacion>? _rubros;
        private List<Ent.PeriodoLectivo>? _periodos;
        private ConfiguracionCalificacionDTO? _configuracion;

        public ResultadoCalificacionService(
            ICalificacionRepository calificacionRepository,
            IPeriodoLectivoRepository periodoRepository)
        {
            _calificacionRepository = calificacionRepository;
            _periodoRepository = periodoRepository;
        }

        private sealed class DatosClase
        {
            public Dictionary<int, Ent.ActividadEvaluacion> Actividades { get; init; } = new();
            public Dictionary<int, decimal> Pesos { get; init; } = new();
            public ILookup<int, Ent.Nota> NotasPorMatricula { get; init; } = null!;
            public ILookup<int, Ent.Asistencia> AsistenciaPorEstudiante { get; init; } = null!;
        }

        // ---------- Configuración ----------

        public async Task<ConfiguracionCalificacionDTO> GetConfiguracion()
        {
            if (_configuracion != null)
                return _configuracion;

            var config = await _calificacionRepository.GetConfiguracion();

            // Si la fila no existe se usan los valores del MEP
            _configuracion = new ConfiguracionCalificacionDTO
            {
                NotaMinima = config?.NotaMinima ?? 70m,
                ConsideraAsistencia = config?.ConsiderarAsistencia ?? true,
                PorcentajeAsistencia = config?.PorcentajeAsistencia ?? 5m
            };

            return _configuracion;
        }

        // ---------- Resultado de un estudiante en un curso ----------

        public async Task<ResultadoCursoDTO> GetResultadoCurso(int idEstudiante, int idCurso, int idPeriodo)
        {
            var config = await GetConfiguracion();
            var periodosDelAnio = await GetPeriodosDelAnio(idPeriodo);

            var resultado = new ResultadoCursoDTO { IdCurso = idCurso, NotaMinima = config.NotaMinima };

            foreach (var periodo in periodosDelAnio)
            {
                var matricula = await _calificacionRepository.GetMatriculaDeEstudiante(idEstudiante, periodo.IdPeriodo, idCurso);

                if (matricula == null)
                    continue;

                if (resultado.Curso.Length == 0)
                    resultado.Curso = matricula.MatriculasCursos.FirstOrDefault(mc => mc.IdCurso == idCurso)?.Curso?.Nombre ?? string.Empty;

                var datos = await CargarClase(periodo, idCurso, matricula.IdSeccion, matricula.IdGrupo);

                resultado.Periodos.Add(CalcularPeriodo(periodo, datos, matricula.IdMatricula, idEstudiante, config));
            }

            resultado.Acumulado = resultado.Periodos.Sum(p => p.Acumulado);
            resultado.Evaluado = resultado.Periodos.Sum(p => p.Evaluado);
            resultado.NotaAnual = resultado.Evaluado > 0
                ? Math.Round(resultado.Acumulado / resultado.Evaluado * 100m, 2)
                : null;

            // El resultado es definitivo cuando el estudiante tiene todos los semestres del año y todos están cerrados
            resultado.Definitivo = resultado.Periodos.Count >= ReglasCalificacion.PeriodosPorAnio
                                   && resultado.Periodos.All(p => !p.Activo);

            resultado.Estado = resultado.NotaAnual == null
                ? "Sin notas"
                : resultado.Definitivo
                    ? (resultado.NotaAnual >= config.NotaMinima ? "Aprobado" : "Reprobado")
                    : (resultado.NotaAnual >= config.NotaMinima ? "Aprobando" : "Reprobando");

            return resultado;
        }

        // ---------- Promoción del año ----------

        public async Task<RespuestaDTO<PromocionDTO>> GetPromocion(int idEstudiante, int idPeriodo)
        {
            try
            {
                var periodosDelAnio = await GetPeriodosDelAnio(idPeriodo);

                if (periodosDelAnio.Count == 0)
                    return RespuestaDTO<PromocionDTO>.Error("El período lectivo seleccionado no existe", 1404);

                var matriculas = await _calificacionRepository.GetMatriculasDeEstudiante(
                    idEstudiante, periodosDelAnio.Select(p => p.IdPeriodo).ToList());

                if (matriculas.Count == 0)
                    return RespuestaDTO<PromocionDTO>.Error("El estudiante no tiene matrícula activa en ese año lectivo", 1428);

                var config = await GetConfiguracion();

                var promocion = new PromocionDTO
                {
                    IdEstudiante = idEstudiante,
                    Anio = periodosDelAnio.First().FechaInicio.Year,
                    NotaMinima = config.NotaMinima
                };

                var idsCurso = matriculas
                    .SelectMany(m => m.MatriculasCursos)
                    .Select(mc => mc.IdCurso)
                    .Distinct()
                    .ToList();

                foreach (var idCurso in idsCurso)
                    promocion.Cursos.Add(await GetResultadoCurso(idEstudiante, idCurso, idPeriodo));

                promocion.Cursos = promocion.Cursos.OrderBy(c => c.Curso).ToList();
                promocion.Aprobados = promocion.Cursos.Count(c => c.Estado == "Aprobado");
                promocion.Reprobados = promocion.Cursos.Count(c => c.Estado == "Reprobado");
                promocion.EnCurso = promocion.Cursos.Count - promocion.Aprobados - promocion.Reprobados;

                // Aprueba el año si aprueba todas las materias
                promocion.Estado = promocion.Cursos.Count == 0
                    ? "Sin cursos"
                    : promocion.Reprobados > 0
                        ? "No promovido"
                        : promocion.Aprobados == promocion.Cursos.Count
                            ? "Promovido"
                            : "En curso";

                return RespuestaDTO<PromocionDTO>.Exito(promocion, null);
            }
            catch (Exception)
            {
                return RespuestaDTO<PromocionDTO>.Error("No fue posible calcular la promoción del estudiante", 1409);
            }
        }

        // ---------- Cálculo de un semestre ----------

        private ResultadoPeriodoDTO CalcularPeriodo(
            Ent.PeriodoLectivo periodo, DatosClase datos, int idMatricula, int idEstudiante, ConfiguracionCalificacionDTO config)
        {
            var notas = datos.NotasPorMatricula[idMatricula].ToList();

            // Cada nota vale (puntos obtenidos / puntos de la actividad) x peso de la actividad
            var rubros = _rubros!.Select(r =>
            {
                decimal acumulado = 0, evaluado = 0;

                foreach (var n in notas)
                {
                    if (!datos.Actividades.TryGetValue(n.IdActividad, out var act)
                        || act.IdRubro != r.IdRubro || act.Puntos <= 0)
                        continue;

                    decimal peso = datos.Pesos[act.IdActividad];

                    acumulado += n.PuntosObtenidos / act.Puntos * peso;
                    evaluado += peso;
                }

                return new RubroResumenDTO
                {
                    IdRubro = r.IdRubro,
                    Acumulado = Math.Round(acumulado, 2),
                    Evaluado = Math.Round(evaluado, 2)
                };
            }).ToList();

            // MCNF-01-04: la asistencia sale de los registros de la tabla Asistencia del período.
            // Solo "Ausente" resta; cada semestre aporta la mitad del porcentaje anual de asistencia.
            var registros = datos.AsistenciaPorEstudiante[idEstudiante].ToList();
            int ausencias = registros.Count(a => a.Estado.Trim().Equals("Ausente", StringComparison.OrdinalIgnoreCase));
            decimal cuota = config.ConsideraAsistencia ? config.PorcentajeAsistencia / ReglasCalificacion.PeriodosPorAnio : 0m;

            decimal? porcentaje = registros.Count > 0
                ? Math.Round((decimal)(registros.Count - ausencias) / registros.Count * 100m, 2)
                : null;

            bool cuentaAsistencia = cuota > 0 && porcentaje != null;

            var asistencia = new AsistenciaResumenDTO
            {
                Registros = registros.Count,
                Ausencias = ausencias,
                Porcentaje = porcentaje,
                Acumulado = cuentaAsistencia ? Math.Round(porcentaje!.Value / 100m * cuota, 2) : 0m,
                Evaluado = cuentaAsistencia ? Math.Round(cuota, 2) : 0m
            };

            // MCNF-01-05: el promedio es lo ganado entre lo ya evaluado (rubros y asistencia), por 100
            decimal totalAcumulado = rubros.Sum(x => x.Acumulado) + asistencia.Acumulado;
            decimal totalEvaluado = rubros.Sum(x => x.Evaluado) + asistencia.Evaluado;

            return new ResultadoPeriodoDTO
            {
                IdPeriodo = periodo.IdPeriodo,
                Periodo = periodo.Nombre,
                Activo = periodo.Activo,
                Rubros = rubros,
                Asistencia = asistencia,
                Acumulado = totalAcumulado,
                Evaluado = totalEvaluado,
                Promedio = totalEvaluado > 0 ? Math.Round(totalAcumulado / totalEvaluado * 100m, 2) : null
            };
        }

        // ---------- Carga de datos ----------

        private async Task<List<Ent.RubroEvaluacion>> GetRubros()
        {
            _rubros ??= (await _calificacionRepository.GetRubros()).ToList();

            return _rubros;
        }

        // Períodos del mismo año que el indicado (los semestres), del más antiguo al más reciente
        private async Task<List<Ent.PeriodoLectivo>> GetPeriodosDelAnio(int idPeriodo)
        {
            _periodos ??= (await _periodoRepository.GetPeriodos()).ToList();

            var periodo = _periodos.FirstOrDefault(p => p.IdPeriodo == idPeriodo);

            if (periodo == null)
                return new List<Ent.PeriodoLectivo>();

            return _periodos
                .Where(p => p.FechaInicio.Year == periodo.FechaInicio.Year)
                .OrderBy(p => p.FechaInicio)
                .ToList();
        }

        private async Task<DatosClase> CargarClase(Ent.PeriodoLectivo periodo, int idCurso, int idSeccion, int idGrupo)
        {
            var clave = (periodo.IdPeriodo, idCurso, idSeccion, idGrupo);

            if (_clases.TryGetValue(clave, out var guardada))
                return guardada;

            var rubros = await GetRubros();
            var actividades = (await _calificacionRepository.GetActividadesDeClase(
                periodo.IdPeriodo, idCurso, idSeccion, idGrupo)).ToList();

            var notas = await _calificacionRepository.GetNotasDeActividades(
                actividades.Select(a => a.IdActividad).ToList());

            var asistencias = await _calificacionRepository.GetAsistenciasDeClase(
                periodo.IdPeriodo, idCurso, idSeccion, idGrupo, periodo.FechaInicio, periodo.FechaFin);

            var datos = new DatosClase
            {
                Actividades = actividades.ToDictionary(a => a.IdActividad),
                Pesos = ReglasCalificacion.CalcularPesos(rubros, actividades),
                NotasPorMatricula = notas.ToLookup(n => n.IdMatricula),
                AsistenciaPorEstudiante = asistencias.ToLookup(a => a.IdEstudiante)
            };

            _clases[clave] = datos;

            return datos;
        }
    }
}
