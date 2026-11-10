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
            return (await CalcularCurso(idEstudiante, idCurso, idPeriodo)).Resultado;
        }

        private sealed record ContextoPeriodo(int IdMatricula, DatosClase Datos);

        // Calcula cada semestre del año en que el estudiante lleva el curso y los junta en la nota anual
        private async Task<(ResultadoCursoDTO Resultado, List<ContextoPeriodo> Contextos)> CalcularCurso(
            int idEstudiante, int idCurso, int idPeriodo)
        {
            var config = await GetConfiguracion();
            var periodosDelAnio = await GetPeriodosDelAnio(idPeriodo);

            var resultado = new ResultadoCursoDTO { IdCurso = idCurso, NotaMinima = config.NotaMinima };
            var contextos = new List<ContextoPeriodo>();

            foreach (var periodo in periodosDelAnio)
            {
                var matricula = await _calificacionRepository.GetMatriculaDeEstudiante(idEstudiante, periodo.IdPeriodo, idCurso);

                if (matricula == null)
                    continue;

                if (resultado.Curso.Length == 0)
                    resultado.Curso = matricula.MatriculasCursos.FirstOrDefault(mc => mc.IdCurso == idCurso)?.Curso?.Nombre ?? string.Empty;

                var datos = await CargarClase(periodo, idCurso, matricula.IdSeccion, matricula.IdGrupo);

                resultado.Periodos.Add(CalcularPeriodo(periodo, datos, matricula.IdMatricula, idEstudiante, config));
                contextos.Add(new ContextoPeriodo(matricula.IdMatricula, datos));
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

            return (resultado, contextos);
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

                (promocion.Aprobados, promocion.Reprobados, promocion.EnCurso, promocion.Estado) =
                    ClasificarPromocion(promocion.Cursos.Select(c => c.Estado));

                return RespuestaDTO<PromocionDTO>.Exito(promocion, null);
            }
            catch (Exception)
            {
                return RespuestaDTO<PromocionDTO>.Error("No fue posible calcular la promoción del estudiante", 1409);
            }
        }

        // ---------- Detalle completo (MCNF-01-15) ----------

        public async Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalle(
            int idEstudiante, int idPeriodo, int? idCurso, bool incluirObservaciones)
        {
            try
            {
                var periodosDelAnio = await GetPeriodosDelAnio(idPeriodo);

                if (periodosDelAnio.Count == 0)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("El período lectivo seleccionado no existe", 1404);

                var matriculas = await _calificacionRepository.GetMatriculasDeEstudiante(
                    idEstudiante, periodosDelAnio.Select(p => p.IdPeriodo).ToList());

                var idsCurso = matriculas
                    .SelectMany(m => m.MatriculasCursos)
                    .Select(mc => mc.IdCurso)
                    .Distinct()
                    .Where(c => idCurso == null || c == idCurso)
                    .ToList();

                if (idsCurso.Count == 0)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("El estudiante no tiene cursos matriculados en ese año lectivo", 1428);

                var config = await GetConfiguracion();

                var detalle = new DetalleEstudianteDTO
                {
                    IdEstudiante = idEstudiante,
                    Anio = periodosDelAnio.First().FechaInicio.Year,
                    NotaMinima = config.NotaMinima,
                    IncluyeObservaciones = incluirObservaciones
                };

                foreach (var id in idsCurso)
                    detalle.Cursos.Add(await GetDetalleCurso(idEstudiante, id, idPeriodo, incluirObservaciones));

                detalle.Cursos = detalle.Cursos.OrderBy(c => c.Curso).ToList();

                // La promoción solo tiene sentido con todos los cursos del estudiante
                if (idCurso == null)
                    (detalle.Aprobados, detalle.Reprobados, detalle.EnCurso, detalle.Estado) =
                        ClasificarPromocion(detalle.Cursos.Select(c => c.Estado));

                return RespuestaDTO<DetalleEstudianteDTO>.Exito(detalle, null);
            }
            catch (Exception)
            {
                return RespuestaDTO<DetalleEstudianteDTO>.Error("No fue posible consultar las calificaciones", 1409);
            }
        }

        private async Task<DetalleCursoDTO> GetDetalleCurso(
            int idEstudiante, int idCurso, int idPeriodo, bool incluirObservaciones)
        {
            var (resultado, contextos) = await CalcularCurso(idEstudiante, idCurso, idPeriodo);
            var rubros = await GetRubros();

            var detalle = new DetalleCursoDTO
            {
                IdCurso = idCurso,
                Curso = resultado.Curso,
                NotaMinima = resultado.NotaMinima,
                NotaAnual = resultado.NotaAnual,
                Estado = resultado.Estado,
                Definitivo = resultado.Definitivo
            };

            for (int i = 0; i < resultado.Periodos.Count; i++)
            {
                var res = resultado.Periodos[i];
                var datos = contextos[i].Datos;
                var notas = datos.NotasPorMatricula[contextos[i].IdMatricula].ToDictionary(n => n.IdActividad);

                detalle.Periodos.Add(new DetallePeriodoDTO
                {
                    IdPeriodo = res.IdPeriodo,
                    Periodo = res.Periodo,
                    Activo = res.Activo,
                    Asistencia = res.Asistencia,
                    Acumulado = res.Acumulado,
                    Evaluado = res.Evaluado,
                    Promedio = res.Promedio,
                    Rubros = rubros.Select(r =>
                    {
                        var resumen = res.Rubros.First(x => x.IdRubro == r.IdRubro);

                        return new DetalleRubroDTO
                        {
                            IdRubro = r.IdRubro,
                            Nombre = r.Nombre,
                            Porcentaje = r.Porcentaje,
                            Acumulado = resumen.Acumulado,
                            Evaluado = resumen.Evaluado,
                            Actividades = datos.Actividades.Values
                                .Where(a => a.IdRubro == r.IdRubro)
                                .OrderBy(a => a.Fecha)
                                .ThenBy(a => a.IdActividad)
                                .Select(a =>
                                {
                                    notas.TryGetValue(a.IdActividad, out var nota);
                                    decimal peso = datos.Pesos[a.IdActividad];
                                    bool calificada = nota != null && a.Puntos > 0;

                                    return new DetalleActividadDTO
                                    {
                                        Actividad = a.Nombre,
                                        Fecha = a.Fecha,
                                        Puntos = a.Puntos,
                                        PuntosObtenidos = nota?.PuntosObtenidos,
                                        Porcentaje = calificada ? Math.Round(nota!.PuntosObtenidos / a.Puntos * 100m, 2) : null,
                                        Peso = Math.Round(peso, 2),
                                        Aporte = calificada ? Math.Round(nota!.PuntosObtenidos / a.Puntos * peso, 2) : null,
                                        Observacion = incluirObservaciones ? nota?.Observacion : null
                                    };
                                }).ToList()
                        };
                    }).ToList()
                });
            }

            return detalle;
        }

        // Aprueba el año el estudiante que aprueba todas las materias
        private static (int Aprobados, int Reprobados, int EnCurso, string Estado) ClasificarPromocion(IEnumerable<string> estados)
        {
            var lista = estados.ToList();
            int aprobados = lista.Count(e => e == "Aprobado");
            int reprobados = lista.Count(e => e == "Reprobado");

            string estado = lista.Count == 0
                ? "Sin cursos"
                : reprobados > 0
                    ? "No promovido"
                    : aprobados == lista.Count
                        ? "Promovido"
                        : "En curso";

            return (aprobados, reprobados, lista.Count - aprobados - reprobados, estado);
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
