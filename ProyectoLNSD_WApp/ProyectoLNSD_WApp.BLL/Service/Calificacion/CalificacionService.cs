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

        private readonly ICalificacionRepository _calificacionRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;

        public CalificacionService(
            ICalificacionRepository calificacionRepository,
            IDocenteRepository docenteRepository,
            IPeriodoLectivoRepository periodoRepository)
        {
            _calificacionRepository = calificacionRepository;
            _docenteRepository = docenteRepository;
            _periodoRepository = periodoRepository;
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

                var idsMatricula = matriculas.Select(m => m.IdMatricula).ToHashSet();
                var notasPorMatricula = notas
                    .Where(n => idsMatricula.Contains(n.IdMatricula))
                    .ToLookup(n => n.IdMatricula);

                var planilla = new PlanillaDTO
                {
                    IdPeriodo = idPeriodo,
                    IdCurso = idCurso,
                    IdSeccion = idSeccion,
                    IdGrupo = idGrupo,
                    PeriodoActivo = periodo.Activo,
                    Rubros = rubros.Select(r => new RubroPlanillaDTO
                    {
                        IdRubro = r.IdRubro,
                        Nombre = r.Nombre,
                        Porcentaje = r.Porcentaje,
                        PuntosMinimo = r.PuntosMinimo,
                        PuntosMaximo = r.PuntosMaximo,
                        Actividades = actividades
                            .Where(a => a.IdRubro == r.IdRubro)
                            .Select(a => new ActividadDTO
                            {
                                IdActividad = a.IdActividad,
                                Nombre = a.Nombre,
                                Puntos = a.Puntos,
                                Fecha = a.Fecha
                            }).ToList()
                    }).ToList(),
                    Estudiantes = matriculas.Select(m =>
                    {
                        var usuario = m.Estudiante?.Usuario;

                        return new PlanillaEstudianteDTO
                        {
                            IdMatricula = m.IdMatricula,
                            Carnet = m.Estudiante?.Carnet ?? string.Empty,
                            Nombre = usuario == null ? string.Empty : $"{usuario.Apellido} {usuario.Nombre}",
                            Notas = notasPorMatricula[m.IdMatricula].Select(n => new NotaCeldaDTO
                            {
                                IdActividad = n.IdActividad,
                                IdNota = n.IdNota,
                                Puntos = n.PuntosObtenidos,
                                Observacion = n.Observacion
                            }).ToList()
                        };
                    }).ToList()
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

        // ---------- Auxiliares ----------

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
