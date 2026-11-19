using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Calificacion;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using Ent = ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    public class CalificacionService : ICalificacionService
    {
        private const string MensajeSinPeriodoActivo =
            "No hay un período lectivo activo. Debe activarlo el administrador en Configuración.";

        private const string MensajeSoloDocentes =
            "Solo el personal docente activo puede registrar calificaciones";

        private const decimal PuntosMaximosActividad = 100m;

        // El año lectivo tiene dos períodos (semestres) y los porcentajes de los rubros son de la nota anual
        private const int PeriodosPorAnio = ReglasCalificacion.PeriodosPorAnio;

        private readonly ICalificacionRepository _calificacionRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IResultadoCalificacionService _resultados;

        public CalificacionService(
            ICalificacionRepository calificacionRepository,
            IDocenteRepository docenteRepository,
            IPeriodoLectivoRepository periodoRepository,
            IResultadoCalificacionService resultados)
        {
            _calificacionRepository = calificacionRepository;
            _docenteRepository = docenteRepository;
            _periodoRepository = periodoRepository;
            _resultados = resultados;
        }

        // ---------- Períodos ----------

        public async Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> GetPeriodos()
        {
            try
            {
                var periodos = await _periodoRepository.GetPeriodos();

                return RespuestaDTO<List<PeriodoCalificacionDTO>>.Exito(
                    periodos
                        .OrderByDescending(p => p.FechaInicio)
                        .Select(p => new PeriodoCalificacionDTO
                        {
                            Id = p.IdPeriodo,
                            Nombre = p.Nombre,
                            Activo = p.Activo
                        }).ToList());
            }
            catch (Exception)
            {
                return RespuestaDTO<List<PeriodoCalificacionDTO>>.Error("No fue posible cargar los períodos lectivos", 1409);
            }
        }

        // ---------- Mis cursos y grupos (MCNF-01-02) ----------

        public async Task<RespuestaDTO<List<ClaseDocenteDTO>>> GetMisClases(int idUsuario, int? idPeriodo)
        {
            try
            {
                var docente = await ObtenerDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<List<ClaseDocenteDTO>>.Error(MensajeSoloDocentes, 1401);

                int? periodoId = idPeriodo ?? (await _periodoRepository.GetPeriodoActivo())?.IdPeriodo;

                if (periodoId == null)
                    return RespuestaDTO<List<ClaseDocenteDTO>>.Error(MensajeSinPeriodoActivo, 1402);

                var horarios = await _calificacionRepository.GetClasesActivasDeDocente(docente.IdDocente, periodoId.Value);

                // Un curso con varios días de horario cuenta una sola vez por sección y grupo
                var clases = horarios
                    .GroupBy(h => new { h.IdCurso, h.IdSeccion, h.IdGrupo })
                    .Select(g => g.First())
                    .Select(h => new ClaseDocenteDTO
                    {
                        IdPeriodo = periodoId.Value,
                        IdCurso = h.IdCurso,
                        IdSeccion = h.IdSeccion,
                        IdGrupo = h.IdGrupo,
                        Curso = h.Curso?.Nombre ?? string.Empty,
                        Grado = h.Seccion?.Grado?.Nombre ?? string.Empty,
                        Seccion = h.Seccion?.Nombre ?? string.Empty,
                        Grupo = h.Grupo?.Nombre ?? string.Empty,
                        Descripcion = $"{h.Curso?.Nombre} · {h.Seccion?.Grado?.Nombre} {h.Seccion?.Nombre} · Grupo {h.Grupo?.Nombre}"
                    })
                    .OrderBy(c => c.Curso)
                    .ThenBy(c => c.Grado)
                    .ThenBy(c => c.Seccion)
                    .ThenBy(c => c.Grupo)
                    .ToList();

                return RespuestaDTO<List<ClaseDocenteDTO>>.Exito(
                    clases,
                    clases.Count == 0 ? "No tiene cursos ni grupos asignados en el período seleccionado" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<ClaseDocenteDTO>>.Error("No fue posible consultar sus cursos", 1409);
            }
        }

        // ---------- Planilla del curso (MCNF-01-01) ----------

        public async Task<RespuestaDTO<PlanillaDTO>> GetPlanilla(
            int idUsuario, int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            try
            {
                var docente = await ObtenerDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<PlanillaDTO>.Error(MensajeSoloDocentes, 1401);

                // MCNF-01-02: el docente solo ve lo que tiene asignado
                if (!await _calificacionRepository.DocenteImparteClase(
                        docente.IdDocente, idPeriodo, idCurso, idSeccion, idGrupo))
                    return RespuestaDTO<PlanillaDTO>.Error("No tiene asignado este curso y grupo", 1403);

                var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

                if (periodo == null)
                    return RespuestaDTO<PlanillaDTO>.Error("El período lectivo seleccionado no existe", 1404);

                var rubros = await _calificacionRepository.GetRubros();
                var actividades = await _calificacionRepository.GetActividadesDeClase(idPeriodo, idCurso, idSeccion, idGrupo);
                var matriculas = await _calificacionRepository.GetMatriculasDeClase(idPeriodo, idCurso, idSeccion, idGrupo);

                var notas = await _calificacionRepository.GetNotasDeActividades(
                    actividades.Select(a => a.IdActividad).ToList());

                // Peso de cada actividad en la nota anual (mismas reglas que usa el cálculo de resultados)
                var pesoPorActividad = ReglasCalificacion.CalcularPesos(rubros, actividades);

                var actividadesAnio = await GetActividadesDelAnio(periodo, idCurso, idSeccion, idGrupo);
                var idsMatricula = matriculas.Select(m => m.IdMatricula).ToHashSet();
                var notasPorMatricula = notas
                    .Where(n => idsMatricula.Contains(n.IdMatricula))
                    .ToLookup(n => n.IdMatricula);

                var config = await _resultados.GetConfiguracion();
                var estudiantes = new List<PlanillaEstudianteDTO>();

                // El promedio, la nota anual y el resultado los calcula el servicio de resultados (una sola fuente)
                foreach (var m in matriculas)
                {
                    var usuario = m.Estudiante?.Usuario;
                    var resultado = await _resultados.GetResultadoCurso(m.IdEstudiante, idCurso, idPeriodo);
                    var delPeriodo = resultado.Periodos.FirstOrDefault(p => p.IdPeriodo == idPeriodo);

                    estudiantes.Add(new PlanillaEstudianteDTO
                    {
                        IdMatricula = m.IdMatricula,
                        IdEstudiante = m.IdEstudiante,
                        Carnet = m.Estudiante?.Carnet ?? string.Empty,
                        Nombre = usuario == null ? string.Empty : $"{usuario.Apellido} {usuario.Nombre}",
                        Notas = notasPorMatricula[m.IdMatricula].Select(n => new NotaCeldaDTO
                        {
                            IdActividad = n.IdActividad,
                            IdNota = n.IdNota,
                            Puntos = n.PuntosObtenidos,
                            Observacion = n.Observacion
                        }).ToList(),
                        Resumen = delPeriodo?.Rubros ?? new List<RubroResumenDTO>(),
                        Asistencia = delPeriodo?.Asistencia ?? new AsistenciaCalificacionDTO(),
                        Acumulado = delPeriodo?.Acumulado ?? 0m,
                        Evaluado = delPeriodo?.Evaluado ?? 0m,
                        Promedio = delPeriodo?.Promedio,
                        NotaAnual = resultado.NotaAnual,
                        Resultado = resultado.Estado,
                        ResultadoDefinitivo = resultado.Definitivo
                    });
                }

                var planilla = new PlanillaDTO
                {
                    IdPeriodo = idPeriodo,
                    IdCurso = idCurso,
                    IdSeccion = idSeccion,
                    IdGrupo = idGrupo,
                    PeriodoActivo = periodo.Activo,
                    NotaMinima = config.NotaMinima,
                    ConsideraAsistencia = config.ConsideraAsistencia,
                    PorcentajeAsistencia = config.PorcentajeAsistencia,
                    Rubros = rubros.Select(r => new RubroPlanillaDTO
                    {
                        IdRubro = r.IdRubro,
                        Nombre = r.Nombre,
                        Porcentaje = r.Porcentaje,
                        PuntosMinimo = r.PuntosMinimo,
                        PuntosMaximo = r.PuntosMaximo,
                        CantidadFija = r.CantidadActividades,
                        PorcentajePeriodo = r.Porcentaje / PeriodosPorAnio,
                        PesoManual = r.PesoManual,
                        PesoRestante = r.PesoManual
                            ? Math.Max(0m, r.Porcentaje - actividadesAnio.Where(x => x.IdRubro == r.IdRubro).Sum(x => x.Porcentaje ?? 0m))
                            : 0m,
                        ActividadesFaltantesAnio = r.PesoManual && r.CantidadActividades.HasValue
                            ? Math.Max(0, r.CantidadActividades.Value * PeriodosPorAnio - actividadesAnio.Count(x => x.IdRubro == r.IdRubro))
                            : 0,
                        Actividades = actividades
                            .Where(a => a.IdRubro == r.IdRubro)
                            .Select(a => new ActividadDTO
                            {
                                IdActividad = a.IdActividad,
                                Nombre = a.Nombre,
                                Puntos = a.Puntos,
                                Porcentaje = Math.Round(pesoPorActividad[a.IdActividad], 2),
                                Fecha = a.Fecha
                            }).ToList()
                    }).ToList(),
                    Estudiantes = estudiantes
                };

                return RespuestaDTO<PlanillaDTO>.Exito(
                    planilla,
                    planilla.Estudiantes.Count == 0 ? "No hay estudiantes matriculados en este curso y grupo" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<PlanillaDTO>.Error("No fue posible cargar la planilla de calificaciones", 1409);
            }
        }

        // ---------- Crear una actividad (MCNF-01-03) ----------

        public async Task<RespuestaDTO<ActividadDTO>> CrearActividad(int idUsuario, ActividadCrearDTO dto)
        {
            try
            {
                string nombre = (dto.Nombre ?? string.Empty).Trim();

                if (nombre.Length == 0)
                    return RespuestaDTO<ActividadDTO>.Error("Debe indicar el nombre de la actividad", 1417);

                var docente = await ObtenerDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<ActividadDTO>.Error(MensajeSoloDocentes, 1401);

                if (!await _calificacionRepository.DocenteImparteClase(
                        docente.IdDocente, dto.IdPeriodo, dto.IdCurso, dto.IdSeccion, dto.IdGrupo))
                    return RespuestaDTO<ActividadDTO>.Error("No tiene asignado este curso y grupo", 1403);

                var periodo = await _periodoRepository.GetPeriodo(dto.IdPeriodo);

                if (periodo == null || !periodo.Activo)
                    return RespuestaDTO<ActividadDTO>.Error(
                        "El período lectivo está cerrado. No se pueden agregar actividades", 1410);

                var rubro = await _calificacionRepository.GetRubro(dto.IdRubro);

                if (rubro == null || !rubro.Estado)
                    return RespuestaDTO<ActividadDTO>.Error("El rubro de evaluación no es válido", 1411);

                decimal puntos = Math.Round(dto.Puntos, 2, MidpointRounding.AwayFromZero);

                if (puntos <= 0 || puntos > PuntosMaximosActividad)
                    return RespuestaDTO<ActividadDTO>.Error("El valor de la actividad debe estar entre 0.01 y 100 puntos", 1414);

                // Cada rubro puede tener su propio rango (por ejemplo, los exámenes entre 15 y 30 puntos)
                if ((rubro.PuntosMinimo.HasValue && puntos < rubro.PuntosMinimo.Value)
                    || (rubro.PuntosMaximo.HasValue && puntos > rubro.PuntosMaximo.Value))
                {
                    return RespuestaDTO<ActividadDTO>.Error(
                        $"En {rubro.Nombre} el valor de cada actividad debe estar entre {Formato(rubro.PuntosMinimo ?? 0.01m)} y {Formato(rubro.PuntosMaximo ?? PuntosMaximosActividad)} puntos",
                        1414);
                }

                // Rubros con cantidad fija por período (pruebas: 2 por semestre, 4 al año según el MEP)
                if (rubro.CantidadActividades.HasValue)
                {
                    var delPeriodo = await _calificacionRepository.GetActividadesDeClase(
                        dto.IdPeriodo, dto.IdCurso, dto.IdSeccion, dto.IdGrupo);

                    if (delPeriodo.Count(x => x.IdRubro == dto.IdRubro) >= rubro.CantidadActividades.Value)
                        return RespuestaDTO<ActividadDTO>.Error(
                            $"{rubro.Nombre} ya tiene sus {rubro.CantidadActividades.Value} actividades en este período",
                            1419);
                }

                // Rubros de peso manual (pruebas): el docente reparte el % anual del rubro entre las actividades del año
                decimal? porcentaje = null;

                if (rubro.PesoManual)
                {
                    var delAnio = (await GetActividadesDelAnio(periodo, dto.IdCurso, dto.IdSeccion, dto.IdGrupo))
                        .Where(x => x.IdRubro == dto.IdRubro)
                        .ToList();

                    decimal restante = rubro.Porcentaje - delAnio.Sum(x => x.Porcentaje ?? 0m);
                    int faltantes = rubro.CantidadActividades.HasValue
                        ? rubro.CantidadActividades.Value * PeriodosPorAnio - delAnio.Count
                        : 1;

                    if (faltantes <= 1)
                    {
                        // La última actividad del año toma lo que queda del rubro
                        porcentaje = restante;
                    }
                    else
                    {
                        // Debe quedar al menos 0.01% para cada actividad que falte
                        decimal maximo = restante - 0.01m * (faltantes - 1);

                        porcentaje = dto.Porcentaje.HasValue
                            ? Math.Round(dto.Porcentaje.Value, 2, MidpointRounding.AwayFromZero)
                            : null;

                        if (porcentaje == null || porcentaje <= 0 || porcentaje > maximo)
                            return RespuestaDTO<ActividadDTO>.Error(
                                $"El porcentaje debe estar entre 0.01 y {Formato(maximo)}% (quedan {Formato(restante)}% por repartir en el año)",
                                1420);
                    }

                    if (porcentaje <= 0)
                        return RespuestaDTO<ActividadDTO>.Error(
                            $"{rubro.Nombre} ya no tiene porcentaje disponible en el año", 1420);
                }

                if (await _calificacionRepository.ExisteActividadConNombre(
                        dto.IdPeriodo, dto.IdCurso, dto.IdSeccion, dto.IdGrupo, dto.IdRubro, nombre))
                    return RespuestaDTO<ActividadDTO>.Error("Ya existe una actividad con ese nombre en este rubro", 1415);

                var actividad = await _calificacionRepository.CrearActividad(new Ent.ActividadEvaluacion
                {
                    IdRubro = dto.IdRubro,
                    IdPeriodo = dto.IdPeriodo,
                    IdCurso = dto.IdCurso,
                    IdSeccion = dto.IdSeccion,
                    IdGrupo = dto.IdGrupo,
                    IdDocente = docente.IdDocente,
                    Nombre = nombre,
                    Puntos = puntos,
                    Porcentaje = porcentaje,
                    Fecha = dto.Fecha?.Date,
                    Estado = true,
                    FechaCreacion = DateTime.Now
                });

                return RespuestaDTO<ActividadDTO>.Exito(
                    new ActividadDTO
                    {
                        IdActividad = actividad.IdActividad,
                        Nombre = actividad.Nombre,
                        Puntos = actividad.Puntos,
                        Fecha = actividad.Fecha
                    },
                    "Actividad agregada correctamente");
            }
            catch (Exception)
            {
                return RespuestaDTO<ActividadDTO>.Error("No fue posible agregar la actividad", 1409);
            }
        }

        // ---------- Registrar una nota (MCNF-01-03 y 01-12) ----------

        public async Task<RespuestaDTO<NotaCeldaDTO>> GuardarNota(int idUsuario, NotaGuardarDTO dto)
        {
            try
            {
                var docente = await ObtenerDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error(MensajeSoloDocentes, 1401);

                var actividad = await _calificacionRepository.GetActividad(dto.IdActividad);

                if (actividad == null || !actividad.Estado)
                    return RespuestaDTO<NotaCeldaDTO>.Error("La actividad no existe", 1413);

                // MCNF-01-12: la nota debe estar entre 0 y lo que vale la actividad
                if (dto.PuntosObtenidos < 0 || dto.PuntosObtenidos > actividad.Puntos)
                    return RespuestaDTO<NotaCeldaDTO>.Error(
                        $"La nota debe estar entre 0 y {Formato(actividad.Puntos)}", 1405);

                var matricula = await _calificacionRepository.GetMatricula(dto.IdMatricula);

                if (matricula == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error("La matrícula seleccionada no existe", 1406);

                if (matricula.Estado != "Activa")
                    return RespuestaDTO<NotaCeldaDTO>.Error("Solo se registran notas a estudiantes con matrícula activa", 1407);

                // La actividad y el estudiante deben ser de la misma clase
                if (matricula.IdPeriodo != actividad.IdPeriodo
                    || matricula.IdSeccion != actividad.IdSeccion
                    || matricula.IdGrupo != actividad.IdGrupo
                    || !matricula.MatriculasCursos.Any(mc => mc.IdCurso == actividad.IdCurso))
                    return RespuestaDTO<NotaCeldaDTO>.Error("El estudiante no pertenece a la clase de esta actividad", 1408);

                // MCNF-01-02: solo en los cursos y grupos que el docente tiene asignados
                if (!await _calificacionRepository.DocenteImparteClase(
                        docente.IdDocente, actividad.IdPeriodo, actividad.IdCurso, actividad.IdSeccion, actividad.IdGrupo))
                    return RespuestaDTO<NotaCeldaDTO>.Error("No tiene asignado este curso y grupo", 1403);

                var periodo = await _periodoRepository.GetPeriodo(actividad.IdPeriodo);

                if (periodo == null || !periodo.Activo)
                    return RespuestaDTO<NotaCeldaDTO>.Error(
                        "El período lectivo está cerrado. No se pueden registrar notas nuevas", 1410);

                if (await _calificacionRepository.GetNota(dto.IdMatricula, dto.IdActividad) != null)
                    return RespuestaDTO<NotaCeldaDTO>.Error(
                        "Esta nota ya fue registrada. Para cambiarla use la opción de editar", 1412);

                decimal puntos = Math.Round(dto.PuntosObtenidos, 2, MidpointRounding.AwayFromZero);
                string? observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();
                DateTime ahora = DateTime.Now;

                var nota = new Ent.Nota
                {
                    IdMatricula = dto.IdMatricula,
                    IdActividad = dto.IdActividad,
                    PuntosObtenidos = puntos,
                    Observacion = observacion,
                    IdUsuarioRegistro = idUsuario,
                    FechaRegistro = ahora
                };

                var historial = new Ent.NotaHistorial
                {
                    IdUsuario = idUsuario,
                    TipoCambio = "Registro",
                    ValorAnterior = null,
                    ValorNuevo = puntos,
                    Observacion = observacion,
                    FechaCambio = ahora
                };

                if (!await _calificacionRepository.CrearNota(nota, historial))
                    return RespuestaDTO<NotaCeldaDTO>.Error("No fue posible registrar la nota", 1409);

                return RespuestaDTO<NotaCeldaDTO>.Exito(
                    new NotaCeldaDTO
                    {
                        IdActividad = nota.IdActividad,
                        IdNota = nota.IdNota,
                        Puntos = nota.PuntosObtenidos,
                        Observacion = nota.Observacion
                    },
                    "Nota registrada correctamente");
            }
            catch (Exception)
            {
                return RespuestaDTO<NotaCeldaDTO>.Error("No fue posible registrar la nota", 1409);
            }
        }

        // ---------- Guardar varias notas a la vez (el docente revisa y luego guarda) ----------

        public async Task<RespuestaDTO<List<NotaLoteResultadoDTO>>> GuardarNotas(int idUsuario, List<NotaGuardarDTO> notas)
        {
            if (notas == null || notas.Count == 0)
                return RespuestaDTO<List<NotaLoteResultadoDTO>>.Error("No hay notas para guardar", 1418);

            if (notas.Count > 500)
                return RespuestaDTO<List<NotaLoteResultadoDTO>>.Error("Se pueden guardar hasta 500 notas a la vez", 1418);

            var resultados = new List<NotaLoteResultadoDTO>();

            // Cada nota pasa por las mismas validaciones de GuardarNota; las que fallen no frenan al resto
            foreach (var dto in notas)
            {
                var r = await GuardarNota(idUsuario, dto);

                resultados.Add(new NotaLoteResultadoDTO
                {
                    IdMatricula = dto.IdMatricula,
                    IdActividad = dto.IdActividad,
                    Exito = r.EsCorrecto,
                    Mensaje = r.EsCorrecto ? null : r.Mensaje,
                    Nota = r.Dato
                });
            }

            int guardadas = resultados.Count(x => x.Exito);

            return RespuestaDTO<List<NotaLoteResultadoDTO>>.Exito(
                resultados,
                guardadas == resultados.Count
                    ? $"{guardadas} {(guardadas == 1 ? "nota guardada" : "notas guardadas")} correctamente"
                    : $"Se guardaron {guardadas} de {resultados.Count} notas");
        }

        // ---------- Editar o corregir una nota (MCNF-01-06 y 01-07) ----------

        public async Task<RespuestaDTO<NotaCeldaDTO>> EditarNota(int idUsuario, NotaEditarDTO dto)
        {
            try
            {
                var docente = await ObtenerDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error(MensajeSoloDocentes, 1401);

                var nota = await _calificacionRepository.GetNotaConActividad(dto.IdNota);
                var actividad = nota?.Actividad;

                if (nota == null || actividad == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error("La nota no existe", 1421);

                // Solo en los cursos y grupos que el docente tiene asignados
                if (!await _calificacionRepository.DocenteImparteClase(
                        docente.IdDocente, actividad.IdPeriodo, actividad.IdCurso, actividad.IdSeccion, actividad.IdGrupo))
                    return RespuestaDTO<NotaCeldaDTO>.Error("No tiene asignado este curso y grupo", 1403);

                // MCNF-01-12: la nota debe estar entre 0 y lo que vale la actividad
                if (dto.PuntosObtenidos < 0 || dto.PuntosObtenidos > actividad.Puntos)
                    return RespuestaDTO<NotaCeldaDTO>.Error(
                        $"La nota debe estar entre 0 y {Formato(actividad.Puntos)}", 1405);

                decimal puntos = Math.Round(dto.PuntosObtenidos, 2, MidpointRounding.AwayFromZero);
                string? observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();
                bool cambiaValor = puntos != nota.PuntosObtenidos;

                if (!cambiaValor && observacion == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error("No hay cambios para guardar", 1422);

                // MCNF-01-07: todo cambio de nota se justifica con una observación
                if (cambiaValor && observacion == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error(
                        "Debe escribir la observación que justifica el cambio de la nota", 1423);

                var periodo = await _periodoRepository.GetPeriodo(actividad.IdPeriodo);

                if (periodo == null)
                    return RespuestaDTO<NotaCeldaDTO>.Error("El período lectivo de la nota no existe", 1404);

                // Con el período cerrado solo se corrige con una autorización sin usar (sirve para UN cambio)
                Ent.CorreccionAutorizada? autorizacion = null;
                string tipo = "Edicion";

                if (!periodo.Activo)
                {
                    autorizacion = await _calificacionRepository.GetCorreccionPendiente(nota.IdNota);

                    if (autorizacion == null)
                        return RespuestaDTO<NotaCeldaDTO>.Error(
                            "El período lectivo está cerrado. Para cambiar esta nota necesita la autorización del administrador o director",
                            1424);

                    tipo = "Correccion";
                }

                DateTime ahora = DateTime.Now;
                decimal anterior = nota.PuntosObtenidos;

                nota.PuntosObtenidos = puntos;
                nota.Observacion = observacion;
                nota.FechaModificacion = ahora;

                if (autorizacion != null)
                {
                    autorizacion.Usada = true;
                    autorizacion.FechaUso = ahora;
                }

                var historial = new Ent.NotaHistorial
                {
                    IdNota = nota.IdNota,
                    IdUsuario = idUsuario,
                    TipoCambio = tipo,
                    ValorAnterior = anterior,
                    ValorNuevo = puntos,
                    Observacion = observacion,
                    FechaCambio = ahora
                };

                if (!await _calificacionRepository.GuardarCambioNota(historial))
                    return RespuestaDTO<NotaCeldaDTO>.Error("No fue posible guardar el cambio", 1409);

                return RespuestaDTO<NotaCeldaDTO>.Exito(
                    new NotaCeldaDTO
                    {
                        IdActividad = nota.IdActividad,
                        IdNota = nota.IdNota,
                        Puntos = nota.PuntosObtenidos,
                        Observacion = nota.Observacion
                    },
                    tipo == "Correccion" ? "Corrección guardada correctamente" : "Nota actualizada correctamente");
            }
            catch (Exception)
            {
                return RespuestaDTO<NotaCeldaDTO>.Error("No fue posible guardar el cambio", 1409);
            }
        }

        // ---------- Historial de un estudiante en un curso (MCNF-01-08) ----------

        public async Task<RespuestaDTO<List<HistorialNotaDTO>>> GetHistorial(
            int idUsuario, int idMatricula, int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            try
            {
                var docente = await ObtenerDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<List<HistorialNotaDTO>>.Error(MensajeSoloDocentes, 1401);

                if (!await _calificacionRepository.DocenteImparteClase(
                        docente.IdDocente, idPeriodo, idCurso, idSeccion, idGrupo))
                    return RespuestaDTO<List<HistorialNotaDTO>>.Error("No tiene asignado este curso y grupo", 1403);

                var matricula = await _calificacionRepository.GetMatricula(idMatricula);

                if (matricula == null
                    || matricula.IdPeriodo != idPeriodo
                    || matricula.IdSeccion != idSeccion
                    || matricula.IdGrupo != idGrupo
                    || !matricula.MatriculasCursos.Any(mc => mc.IdCurso == idCurso))
                    return RespuestaDTO<List<HistorialNotaDTO>>.Error("El estudiante no pertenece a este curso y grupo", 1408);

                var rubros = (await _calificacionRepository.GetRubros()).ToDictionary(r => r.IdRubro);
                var actividades = (await _calificacionRepository.GetActividadesDeClase(idPeriodo, idCurso, idSeccion, idGrupo))
                    .ToDictionary(a => a.IdActividad);

                var notas = (await _calificacionRepository.GetNotasDeActividades(actividades.Keys.ToList()))
                    .Where(n => n.IdMatricula == idMatricula)
                    .ToList();

                var actividadDeNota = notas.ToDictionary(n => n.IdNota, n => n.IdActividad);
                var historial = await _calificacionRepository.GetHistorialDeNotas(notas.Select(n => n.IdNota).ToList());
                var usuarios = await _calificacionRepository.GetNombresUsuarios(
                    historial.Select(h => h.IdUsuario).Distinct().ToList());

                var lista = historial.Select(h =>
                {
                    var act = actividades[actividadDeNota[h.IdNota]];

                    return new HistorialNotaDTO
                    {
                        Fecha = h.FechaCambio,
                        Rubro = rubros.TryGetValue(act.IdRubro, out var rubro) ? rubro.Nombre : string.Empty,
                        Actividad = act.Nombre,
                        PuntosActividad = act.Puntos,
                        Tipo = h.TipoCambio,
                        ValorAnterior = h.ValorAnterior,
                        ValorNuevo = h.ValorNuevo,
                        Observacion = h.Observacion,
                        Usuario = usuarios.TryGetValue(h.IdUsuario, out var nombre) ? nombre : string.Empty
                    };
                }).ToList();

                return RespuestaDTO<List<HistorialNotaDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "Este estudiante todavía no tiene notas registradas en el curso" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<HistorialNotaDTO>>.Error("No fue posible consultar el historial", 1409);
            }
        }

        // ---------- Autorizar correcciones con el período cerrado (MCNF-01-13) ----------

        public async Task<RespuestaDTO<List<ClaseDocenteDTO>>> GetClasesDelPeriodo(int idPeriodo)
        {
            try
            {
                var horarios = await _calificacionRepository.GetClasesConActividades(idPeriodo);

                var clases = horarios
                    .GroupBy(h => new { h.IdCurso, h.IdSeccion, h.IdGrupo })
                    .Select(g => g.First())
                    .Select(h => new ClaseDocenteDTO
                    {
                        IdPeriodo = idPeriodo,
                        IdCurso = h.IdCurso,
                        IdSeccion = h.IdSeccion,
                        IdGrupo = h.IdGrupo,
                        Curso = h.Curso?.Nombre ?? string.Empty,
                        Grado = h.Seccion?.Grado?.Nombre ?? string.Empty,
                        Seccion = h.Seccion?.Nombre ?? string.Empty,
                        Grupo = h.Grupo?.Nombre ?? string.Empty,
                        Descripcion = $"{h.Curso?.Nombre} · {h.Seccion?.Grado?.Nombre} {h.Seccion?.Nombre} · Grupo {h.Grupo?.Nombre}"
                    })
                    .OrderBy(c => c.Curso)
                    .ThenBy(c => c.Grado)
                    .ThenBy(c => c.Seccion)
                    .ThenBy(c => c.Grupo)
                    .ToList();

                return RespuestaDTO<List<ClaseDocenteDTO>>.Exito(
                    clases,
                    clases.Count == 0 ? "No hay cursos con notas en el período seleccionado" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<ClaseDocenteDTO>>.Error("No fue posible consultar los cursos del período", 1409);
            }
        }

        public async Task<RespuestaDTO<List<CorreccionNotaDTO>>> GetNotasParaCorreccion(
            int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

                if (periodo == null)
                    return RespuestaDTO<List<CorreccionNotaDTO>>.Error("El período lectivo seleccionado no existe", 1404);

                // Con el período activo el docente edita sin autorización
                if (periodo.Activo)
                    return RespuestaDTO<List<CorreccionNotaDTO>>.Exito(
                        new List<CorreccionNotaDTO>(),
                        "El período está activo: el docente puede editar las notas sin autorización. Solo se autorizan correcciones en períodos cerrados");

                var rubros = (await _calificacionRepository.GetRubros()).ToDictionary(r => r.IdRubro);
                var actividades = (await _calificacionRepository.GetActividadesDeClase(idPeriodo, idCurso, idSeccion, idGrupo))
                    .ToDictionary(a => a.IdActividad);
                var matriculas = (await _calificacionRepository.GetMatriculasDeClase(idPeriodo, idCurso, idSeccion, idGrupo))
                    .ToDictionary(m => m.IdMatricula);

                var notas = (await _calificacionRepository.GetNotasDeActividades(actividades.Keys.ToList()))
                    .Where(n => matriculas.ContainsKey(n.IdMatricula))
                    .ToList();

                // Estado según la autorización más reciente de cada nota
                var ultimaAutorizacion = (await _calificacionRepository.GetCorreccionesDeNotas(notas.Select(n => n.IdNota).ToList()))
                    .GroupBy(c => c.IdNota)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.IdAutorizacion).First());

                var filas = notas.Select(n =>
                {
                    var act = actividades[n.IdActividad];
                    var usuario = matriculas[n.IdMatricula].Estudiante?.Usuario;
                    ultimaAutorizacion.TryGetValue(n.IdNota, out var aut);

                    return new CorreccionNotaDTO
                    {
                        IdNota = n.IdNota,
                        Carnet = matriculas[n.IdMatricula].Estudiante?.Carnet ?? string.Empty,
                        Estudiante = usuario == null ? string.Empty : $"{usuario.Apellido} {usuario.Nombre}",
                        Rubro = rubros.TryGetValue(act.IdRubro, out var rubro) ? rubro.Nombre : string.Empty,
                        Actividad = act.Nombre,
                        Puntos = n.PuntosObtenidos,
                        PuntosActividad = act.Puntos,
                        Estado = aut == null ? "Sin autorización" : (aut.Usada ? "Usada" : "Autorizada"),
                        Motivo = aut?.Motivo,
                        Autorizable = aut == null || aut.Usada
                    };
                })
                .OrderBy(f => f.Estudiante)
                .ThenBy(f => f.Rubro)
                .ThenBy(f => f.Actividad)
                .ToList();

                return RespuestaDTO<List<CorreccionNotaDTO>>.Exito(
                    filas,
                    filas.Count == 0 ? "Este curso no tiene notas registradas en el período" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<CorreccionNotaDTO>>.Error("No fue posible consultar las notas del curso", 1409);
            }
        }

        public async Task<RespuestaDTO<bool>> AutorizarCorreccion(int idUsuario, CorreccionAutorizarDTO dto)
        {
            try
            {
                string motivo = (dto.Motivo ?? string.Empty).Trim();

                if (motivo.Length == 0)
                    return RespuestaDTO<bool>.Error("Debe indicar el motivo de la corrección", 1427);

                var nota = await _calificacionRepository.GetNotaConActividad(dto.IdNota);

                if (nota == null || nota.Actividad == null)
                    return RespuestaDTO<bool>.Error("La nota no existe", 1421);

                var periodo = await _periodoRepository.GetPeriodo(nota.Actividad.IdPeriodo);

                if (periodo == null)
                    return RespuestaDTO<bool>.Error("El período lectivo de la nota no existe", 1404);

                if (periodo.Activo)
                    return RespuestaDTO<bool>.Error(
                        "El período lectivo está activo: el docente puede editar la nota sin autorización", 1425);

                if (await _calificacionRepository.GetCorreccionPendiente(nota.IdNota) != null)
                    return RespuestaDTO<bool>.Error("Esta nota ya tiene una corrección autorizada sin usar", 1426);

                var creada = await _calificacionRepository.CrearCorreccion(new Ent.CorreccionAutorizada
                {
                    IdNota = nota.IdNota,
                    IdUsuarioAutoriza = idUsuario,
                    Motivo = motivo,
                    Usada = false,
                    FechaAutorizacion = DateTime.Now
                });

                if (!creada)
                    return RespuestaDTO<bool>.Error("No fue posible autorizar la corrección", 1409);

                return RespuestaDTO<bool>.Exito(true, "Corrección autorizada. El docente podrá hacer un cambio en esta nota");
            }
            catch (Exception)
            {
                return RespuestaDTO<bool>.Error("No fue posible autorizar la corrección", 1409);
            }
        }

        // ---------- Auxiliares ----------

        // Actividades de la clase en todos los períodos del mismo año (los dos semestres)
        private async Task<List<Ent.ActividadEvaluacion>> GetActividadesDelAnio(
            Ent.PeriodoLectivo periodo, int idCurso, int idSeccion, int idGrupo)
        {
            var periodos = await _periodoRepository.GetPeriodos();
            var actividades = new List<Ent.ActividadEvaluacion>();

            foreach (var p in periodos.Where(q => q.FechaInicio.Year == periodo.FechaInicio.Year))
                actividades.AddRange(await _calificacionRepository.GetActividadesDeClase(p.IdPeriodo, idCurso, idSeccion, idGrupo));

            return actividades;
        }

        private async Task<Ent.Docente?> ObtenerDocenteActivo(int idUsuario)
        {
            var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

            return docente != null && docente.Estado ? docente : null;
        }

        // 20.00 -> "20" y 17.50 -> "17.5"
        private static string Formato(decimal valor)
        {
            return valor.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
