using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Horario;
using ProyectoLNSD_WApp.DAL.Repositories.Asistencia;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.Horario;
using ProyectoLNSD_WApp.DAL.Repositories.Matricula;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;

namespace ProyectoLNSD_WApp.BLL.Service.Asistencia
{
    public class AsistenciaService : IAsistenciaService
    {
        private const string MensajeSinPeriodoActivo =
            "No hay un período lectivo activo. Debe activarlo el administrador en Configuración.";

        private const string MensajeNoDocente =
            "Solo el personal docente activo puede registrar o consultar sus clases.";

        private const string MatriculaAnulada = "Anulada";

        // Tope de filas del detalle, para no cargar un año entero en el navegador
        private const int LimiteRegistros = 1000;

        private readonly IAsistenciaRepository _asistenciaRepository;
        private readonly IHorarioRepository _horarioRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IMatriculaRepository _matriculaRepository;

        public AsistenciaService(
            IAsistenciaRepository asistenciaRepository,
            IHorarioRepository horarioRepository,
            IDocenteRepository docenteRepository,
            IPeriodoLectivoRepository periodoRepository,
            IMatriculaRepository matriculaRepository)
        {
            _asistenciaRepository = asistenciaRepository;
            _horarioRepository = horarioRepository;
            _docenteRepository = docenteRepository;
            _periodoRepository = periodoRepository;
            _matriculaRepository = matriculaRepository;
        }

        // =====================================================================
        // Docente: pasar lista (MCAF-01-01 a 01-05)
        // =====================================================================

        public async Task<RespuestaDTO<List<HorarioDTO>>> GetMisClases(int idUsuario)
        {
            try
            {
                var docente = await GetDocenteActivo(idUsuario);

                if (docente == null)
                    return RespuestaDTO<List<HorarioDTO>>.Error(MensajeNoDocente, 1402);

                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return RespuestaDTO<List<HorarioDTO>>.Exito(new List<HorarioDTO>(), MensajeSinPeriodoActivo);

                var clases = await _horarioRepository.Buscar(new HorarioFiltro
                {
                    IdPeriodo = periodo.IdPeriodo,
                    IdDocente = docente.IdDocente,
                    Estado = true
                });

                var lista = clases
                    .OrderBy(h => HorarioReglas.OrdenDia(h.Dia))
                    .ThenBy(h => h.HoraInicio)
                    .Select(HorarioService.ToDTO)
                    .ToList();

                return RespuestaDTO<List<HorarioDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No tiene clases asignadas en el horario del período activo" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<HorarioDTO>>.Error("No fue posible cargar sus clases", 1409);
            }
        }

        public async Task<RespuestaDTO<AsistenciaListaDTO>> GetLista(
            int idUsuario, int idHorario, DateOnly fecha, bool puedeEditar)
        {
            try
            {
                var (sesion, error) = await ValidarSesion(idUsuario, idHorario, fecha);

                if (error != null)
                    return error;

                return RespuestaDTO<AsistenciaListaDTO>.Exito(await ArmarLista(sesion!, fecha, puedeEditar));
            }
            catch (Exception)
            {
                return FallaLista("No fue posible cargar la lista de estudiantes", 1409);
            }
        }

        public async Task<RespuestaDTO<AsistenciaListaDTO>> Guardar(
            int idUsuario, AsistenciaGuardarDTO dto, bool puedeEditar)
        {
            try
            {
                if (dto.Fecha == null)
                    return FallaLista("Seleccione la fecha", 1404);

                DateOnly fecha = dto.Fecha.Value;

                var (sesion, error) = await ValidarSesion(idUsuario, dto.IdHorario, fecha);

                if (error != null)
                    return error;

                var clase = sesion!.Clase;

                // MCAF-01-13: período cerrado = solo lectura
                if (await _asistenciaRepository.GetCierre(clase.IdPeriodo) != null)
                    return FallaLista(
                        $"El registro de asistencia del período {sesion.Periodo.Nombre} está cerrado; no se puede modificar",
                        1405);

                // La lista que llega debe ser exactamente la de la clase: ni de más ni de menos
                var matriculas = await _asistenciaRepository.GetMatriculasDeClase(
                    clase.IdPeriodo, clase.IdSeccion, clase.IdGrupo, clase.IdCurso);

                var idsClase = matriculas.Select(m => m.IdMatricula).ToHashSet();
                var idsRecibidos = dto.Registros.Select(r => r.IdMatricula).ToList();

                if (idsRecibidos.Distinct().Count() != idsRecibidos.Count)
                    return FallaLista("La lista contiene estudiantes repetidos. Vuelva a cargarla.", 1406);

                if (idsRecibidos.Any(id => !idsClase.Contains(id)))
                    return FallaLista(
                        "La lista cambió: hay estudiantes que ya no pertenecen a esta clase. Vuelva a cargarla.", 1406);

                int faltantes = idsClase.Count(id => !idsRecibidos.Contains(id));

                if (faltantes > 0)
                    return FallaLista(
                        $"Falta marcar la asistencia de {faltantes} estudiante(s). Vuelva a cargar la lista si se matriculó alguien nuevo.",
                        1406);

                var registros = new List<DAL.Entities.Asistencia>();

                foreach (var r in dto.Registros)
                {
                    // MCAF-01-03
                    string? estado = AsistenciaReglas.NormalizarEstado(r.Estado);

                    if (estado == null)
                        return FallaLista("Marque un estado válido (presente, ausente, tardía o justificada) para cada estudiante", 1406);

                    // MCAF-01-04
                    string? observacion = AsistenciaReglas.LimpiarObservacion(r.Observacion);

                    if (observacion?.Length > AsistenciaReglas.MaxObservacion)
                        return FallaLista(
                            $"La observación no puede superar {AsistenciaReglas.MaxObservacion} caracteres", 1406);

                    registros.Add(new DAL.Entities.Asistencia
                    {
                        IdMatricula = r.IdMatricula,
                        Estado = estado,
                        Observacion = observacion
                    });
                }

                // MCAF-01-05: corregir una lista ya guardada requiere el permiso Editar
                var existentes = await _asistenciaRepository.GetDeSesion(clase.IdHorario, fecha);

                if (existentes.Count > 0 && !puedeEditar)
                    return FallaLista("Esta lista ya fue registrada y su usuario no tiene permiso para modificarla", 1407);

                var (nuevos, modificados) = await _asistenciaRepository.GuardarSesion(
                    clase.IdHorario, fecha, registros, idUsuario, DateTime.Now);

                string mensaje = (nuevos, modificados) switch
                {
                    (0, 0) => "No hubo cambios que guardar",
                    (_, 0) => $"Asistencia registrada correctamente ({nuevos} estudiantes)",
                    (0, _) => $"Asistencia actualizada correctamente ({modificados} cambio(s))",
                    _ => $"Asistencia guardada: {nuevos} registro(s) nuevo(s) y {modificados} cambio(s)"
                };

                return RespuestaDTO<AsistenciaListaDTO>.Exito(await ArmarLista(sesion, fecha, puedeEditar), mensaje);
            }
            catch (Exception)
            {
                return FallaLista("No se pudo guardar la asistencia. Intente nuevamente.", 1409);
            }
        }

        public async Task<RespuestaDTO<List<AsistenciaRegistroDTO>>> GetHistorial(
            int idUsuario, bool soloPropias, int idMatricula, int idCurso)
        {
            try
            {
                var filtro = new AsistenciaFiltro { IdMatricula = idMatricula, IdCurso = idCurso };

                // Un docente solo ve el historial de sus propias clases
                string? errorAlcance = await AplicarAlcance(idUsuario, soloPropias, filtro);

                if (errorAlcance != null)
                    return RespuestaDTO<List<AsistenciaRegistroDTO>>.Error(errorAlcance, 1402);

                var registros = await _asistenciaRepository.Buscar(filtro, LimiteRegistros);

                var lista = registros.Select(ToRegistroDTO).ToList();

                return RespuestaDTO<List<AsistenciaRegistroDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "El estudiante no tiene asistencia registrada en este curso" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<AsistenciaRegistroDTO>>.Error("No fue posible consultar el historial", 1409);
            }
        }

        // =====================================================================
        // Consulta y reportes (MCAF-01-09, 01-10 y 01-12)
        // =====================================================================

        public async Task<RespuestaDTO<OpcionesAsistenciaDTO>> GetOpciones(int idUsuario, bool soloPropias, int? idPeriodo)
        {
            try
            {
                var periodos = await _periodoRepository.GetPeriodos();
                var activo = periodos.FirstOrDefault(p => p.Activo);

                int? idSeleccionado = idPeriodo ?? activo?.IdPeriodo ?? periodos.FirstOrDefault()?.IdPeriodo;

                int? idDocente = null;

                if (soloPropias)
                {
                    var docente = await GetDocenteActivo(idUsuario);

                    if (docente == null)
                        return RespuestaDTO<OpcionesAsistenciaDTO>.Error(MensajeNoDocente, 1402);

                    idDocente = docente.IdDocente;
                }

                // Las listas salen de los horarios del período: solo aparece lo que tiene clases
                var horarios = idSeleccionado.HasValue
                    ? await _horarioRepository.Buscar(new HorarioFiltro { IdPeriodo = idSeleccionado, IdDocente = idDocente })
                    : new List<DAL.Entities.Horario>();

                return RespuestaDTO<OpcionesAsistenciaDTO>.Exito(new OpcionesAsistenciaDTO
                {
                    IdPeriodo = idSeleccionado,
                    UmbralSugerido = AsistenciaReglas.UmbralSugerido,

                    Periodos = periodos.Select(p => new OpcionFiltroDTO
                    {
                        Id = p.IdPeriodo,
                        Nombre = p.Activo ? $"{p.Nombre} (activo)" : p.Nombre
                    }).ToList(),

                    Grados = horarios
                        .Where(h => h.Seccion?.Grado != null)
                        .Select(h => h.Seccion!.Grado!)
                        .DistinctBy(g => g.IdGrado)
                        .OrderBy(g => g.Nombre)
                        .Select(g => new OpcionFiltroDTO { Id = g.IdGrado, Nombre = g.Nombre })
                        .ToList(),

                    Secciones = horarios
                        .Where(h => h.Seccion != null)
                        .Select(h => h.Seccion!)
                        .DistinctBy(s => s.IdSeccion)
                        .OrderBy(s => s.Grado?.Nombre)
                        .ThenBy(s => s.Nombre)
                        .Select(s => new SeccionHorarioDTO
                        {
                            IdSeccion = s.IdSeccion,
                            IdGrado = s.IdGrado,
                            NombreGrado = s.Grado?.Nombre ?? string.Empty,
                            Nombre = s.Nombre
                        })
                        .ToList(),

                    Grupos = horarios
                        .Where(h => h.Grupo != null)
                        .Select(h => h.Grupo!)
                        .DistinctBy(g => g.IdGrupo)
                        .OrderBy(g => g.Nombre)
                        .Select(g => new OpcionFiltroDTO { Id = g.IdGrupo, Nombre = g.Nombre })
                        .ToList(),

                    Cursos = horarios
                        .Where(h => h.Curso != null)
                        .Select(h => h.Curso!)
                        .DistinctBy(c => c.IdCurso)
                        .OrderBy(c => c.Nombre)
                        .Select(c => new OpcionFiltroDTO { Id = c.IdCurso, Nombre = $"{c.Codigo} - {c.Nombre}" })
                        .ToList()
                });
            }
            catch (Exception)
            {
                return RespuestaDTO<OpcionesAsistenciaDTO>.Error("No fue posible cargar los filtros", 1409);
            }
        }

        public async Task<RespuestaDTO<List<AsistenciaResumenDTO>>> GetResumen(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro)
        {
            try
            {
                var (filtroDal, error) = await PrepararFiltro(idUsuario, soloPropias, filtro);

                if (error != null)
                    return RespuestaDTO<List<AsistenciaResumenDTO>>.Error(error, 1406);

                var lista = await CalcularResumen(filtroDal!, filtro);

                return RespuestaDTO<List<AsistenciaResumenDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No hay asistencia registrada con los filtros seleccionados" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<AsistenciaResumenDTO>>.Error("No fue posible generar el resumen", 1409);
            }
        }

        public async Task<RespuestaDTO<List<AsistenciaRegistroDTO>>> GetRegistros(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro)
        {
            try
            {
                var (filtroDal, error) = await PrepararFiltro(idUsuario, soloPropias, filtro);

                if (error != null)
                    return RespuestaDTO<List<AsistenciaRegistroDTO>>.Error(error, 1406);

                // Se pide uno de más para saber si hay más registros que el tope
                var registros = await _asistenciaRepository.Buscar(filtroDal!, LimiteRegistros + 1);

                string? mensaje = null;

                if (registros.Count == 0)
                    mensaje = "No hay registros de asistencia con los filtros seleccionados";
                else if (registros.Count > LimiteRegistros)
                {
                    registros = registros.Take(LimiteRegistros).ToList();
                    mensaje = $"Se muestran los {LimiteRegistros} registros más recientes. Ajuste los filtros para ver otros.";
                }

                return RespuestaDTO<List<AsistenciaRegistroDTO>>.Exito(registros.Select(ToRegistroDTO).ToList(), mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<AsistenciaRegistroDTO>>.Error("No fue posible consultar la asistencia", 1409);
            }
        }

        public async Task<RespuestaDTO<byte[]>> ExportarResumen(int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro)
        {
            try
            {
                var (filtroDal, error) = await PrepararFiltro(idUsuario, soloPropias, filtro);

                if (error != null)
                    return RespuestaDTO<byte[]>.Error(error, 1406);

                var lista = await CalcularResumen(filtroDal!, filtro);

                if (lista.Count == 0)
                    return RespuestaDTO<byte[]>.Error("No hay datos para exportar con los filtros seleccionados", 1406);

                var periodo = await _periodoRepository.GetPeriodo(filtroDal!.IdPeriodo!.Value);

                return RespuestaDTO<byte[]>.Exito(AsistenciaReporteExcel.Generar(lista, periodo?.Nombre ?? string.Empty, filtro));
            }
            catch (Exception)
            {
                return RespuestaDTO<byte[]>.Error("No fue posible generar el reporte", 1409);
            }
        }

        // =====================================================================
        // Estudiante y encargado (MCAF-01-07 y 01-08)
        // =====================================================================

        public async Task<RespuestaDTO<MiAsistenciaDTO>> GetMiAsistencia(int idUsuario, bool esEncargado, int? idPeriodo)
        {
            try
            {
                // La persona se identifica por la sesión: nunca ve matrículas que no son suyas
                var matriculas = (await _matriculaRepository.MatriculasDeUsuario(idUsuario, esEncargado))
                    .Where(m => m.Estado != MatriculaAnulada)
                    .ToList();

                if (matriculas.Count == 0)
                    return RespuestaDTO<MiAsistenciaDTO>.Exito(
                        new MiAsistenciaDTO(),
                        esEncargado
                            ? "Ninguno de sus estudiantes tiene una matrícula registrada"
                            : "No tiene matrículas registradas");

                var periodos = matriculas
                    .Where(m => m.PeriodoLectivo != null)
                    .Select(m => m.PeriodoLectivo!)
                    .DistinctBy(p => p.IdPeriodo)
                    .OrderByDescending(p => p.FechaInicio)
                    .ToList();

                // Período pedido (si es uno de los suyos), si no el activo, si no el más reciente
                int idSeleccionado =
                    periodos.Any(p => p.IdPeriodo == idPeriodo) ? idPeriodo!.Value
                    : periodos.FirstOrDefault(p => p.Activo)?.IdPeriodo
                      ?? periodos.First().IdPeriodo;

                var delPeriodo = matriculas
                    .Where(m => m.IdPeriodo == idSeleccionado)
                    .OrderBy(m => m.Estudiante?.Usuario?.Apellido)
                    .ThenBy(m => m.Estudiante?.Usuario?.Nombre)
                    .ToList();

                var ids = delPeriodo.Select(m => m.IdMatricula).ToList();

                var resumen = await CalcularResumen(
                    new AsistenciaFiltro { IdsMatriculas = ids },
                    new AsistenciaFiltroDTO());

                var incidencias = await _asistenciaRepository.Buscar(
                    new AsistenciaFiltro { IdsMatriculas = ids, SoloIncidencias = true },
                    LimiteRegistros);

                var resultado = new MiAsistenciaDTO
                {
                    IdPeriodo = idSeleccionado,

                    Periodos = periodos.Select(p => new OpcionFiltroDTO
                    {
                        Id = p.IdPeriodo,
                        Nombre = p.Activo ? $"{p.Nombre} (activo)" : p.Nombre
                    }).ToList(),

                    Estudiantes = delPeriodo.Select(m => new AsistenciaMatriculaDTO
                    {
                        IdMatricula = m.IdMatricula,
                        NombreEstudiante = NombreEstudiante(m.Estudiante),
                        Carnet = m.Estudiante?.Carnet ?? string.Empty,
                        NombrePeriodo = m.PeriodoLectivo?.Nombre ?? string.Empty,
                        NombreGrado = m.Seccion?.Grado?.Nombre ?? string.Empty,
                        NombreSeccion = m.Seccion?.Nombre ?? string.Empty,
                        NombreGrupo = m.Grupo?.Nombre ?? string.Empty,
                        Cursos = resumen
                            .Where(r => r.IdMatricula == m.IdMatricula)
                            .OrderBy(r => r.NombreCurso)
                            .ToList(),
                        Incidencias = incidencias
                            .Where(a => a.IdMatricula == m.IdMatricula)
                            .Select(ToRegistroDTO)
                            .ToList()
                    }).ToList()
                };

                bool sinRegistros = resultado.Estudiantes.All(e => e.Cursos.Count == 0);

                return RespuestaDTO<MiAsistenciaDTO>.Exito(
                    resultado,
                    sinRegistros ? "Todavía no hay asistencia registrada en este período" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<MiAsistenciaDTO>.Error("No fue posible consultar la asistencia", 1409);
            }
        }

        // =====================================================================
        // Cierre del período (MCAF-01-13)
        // =====================================================================

        public async Task<RespuestaDTO<CierreAsistenciaDTO>> GetCierre(int idPeriodo)
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

                if (periodo == null)
                    return FallaCierre("El período lectivo no existe", 1401);

                return RespuestaDTO<CierreAsistenciaDTO>.Exito(await ArmarCierre(periodo));
            }
            catch (Exception)
            {
                return FallaCierre("No fue posible consultar el estado del período", 1409);
            }
        }

        public async Task<RespuestaDTO<CierreAsistenciaDTO>> CerrarPeriodo(int idUsuario, int idPeriodo)
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

                if (periodo == null)
                    return FallaCierre("El período lectivo no existe", 1401);

                var estado = await ArmarCierre(periodo);

                if (estado.Cerrado)
                    return FallaCierre($"El registro de asistencia del período {periodo.Nombre} ya está cerrado", 1408);

                if (!estado.PuedeCerrar)
                    return FallaCierre(estado.Motivo ?? "El período todavía no se puede cerrar", 1408);

                if (!await _asistenciaRepository.Cerrar(new DAL.Entities.AsistenciaCierre
                {
                    IdPeriodo = periodo.IdPeriodo,
                    FechaCierre = DateTime.Now,
                    IdUsuario = idUsuario
                }))
                    return FallaCierre($"El registro de asistencia del período {periodo.Nombre} ya está cerrado", 1408);

                return RespuestaDTO<CierreAsistenciaDTO>.Exito(
                    await ArmarCierre(periodo),
                    $"Se cerró el registro de asistencia del período {periodo.Nombre}. Desde ahora solo se puede consultar.");
            }
            catch (Exception)
            {
                return FallaCierre("No se pudo cerrar el período. Intente nuevamente.", 1409);
            }
        }

        // =====================================================================
        // Integración con Notas (MCAF-01-11)
        // =====================================================================

        public async Task<RespuestaDTO<decimal?>> ObtenerPorcentaje(int idMatricula, int idCurso)
        {
            try
            {
                var conteo = (await _asistenciaRepository.Contar(new AsistenciaFiltro
                {
                    IdMatricula = idMatricula,
                    IdCurso = idCurso
                })).FirstOrDefault();

                decimal? porcentaje = conteo == null
                    ? null
                    : AsistenciaReglas.CalcularPorcentaje(conteo.Presentes, conteo.Ausentes, conteo.Tardias, conteo.Justificadas);

                return RespuestaDTO<decimal?>.Exito(
                    porcentaje,
                    porcentaje == null ? "No hay asistencia registrada para este curso" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<decimal?>.Error("No fue posible calcular el porcentaje de asistencia", 1409);
            }
        }

        // =====================================================================
        // Auxiliares
        // =====================================================================

        private sealed record Sesion(DAL.Entities.Horario Clase, DAL.Entities.PeriodoLectivo Periodo);

        private async Task<DAL.Entities.Docente?> GetDocenteActivo(int idUsuario)
        {
            var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

            return docente != null && docente.Estado ? docente : null;
        }

        /// Reglas comunes para abrir o guardar una lista: la clase es del docente,
        /// está activa y la fecha corresponde a esa clase y a su período.
        private async Task<(Sesion? Sesion, RespuestaDTO<AsistenciaListaDTO>? Error)> ValidarSesion(
            int idUsuario, int idHorario, DateOnly fecha)
        {
            var docente = await GetDocenteActivo(idUsuario);

            if (docente == null)
                return (null, FallaLista(MensajeNoDocente, 1402));

            var clase = await _horarioRepository.GetById(idHorario);

            // Misma respuesta si no existe o es de otro docente: no se revela qué clases existen
            if (clase == null || clase.IdDocente != docente.IdDocente)
                return (null, FallaLista("La clase seleccionada no existe o no está asignada a usted", 1403));

            if (!clase.Estado)
                return (null, FallaLista("La clase seleccionada está inactiva en el horario", 1403));

            var periodo = clase.Periodo ?? await _periodoRepository.GetPeriodo(clase.IdPeriodo);

            if (periodo == null)
                return (null, FallaLista("El período lectivo de la clase no existe", 1401));

            if (fecha > AsistenciaReglas.Hoy())
                return (null, FallaLista("No se puede registrar asistencia de una fecha futura", 1404));

            if (fecha < periodo.FechaInicio || fecha > periodo.FechaFin)
                return (null, FallaLista(
                    $"La fecha debe estar dentro del período {periodo.Nombre} " +
                    $"({periodo.FechaInicio:dd/MM/yyyy} al {periodo.FechaFin:dd/MM/yyyy})",
                    1404));

            string? dia = AsistenciaReglas.NombreDia(fecha);

            if (!string.Equals(dia, clase.Dia, StringComparison.OrdinalIgnoreCase))
                return (null, FallaLista(
                    $"Esta clase se imparte los {clase.Dia}; el {fecha:dd/MM/yyyy} es {dia ?? "fin de semana"}",
                    1404));

            return (new Sesion(clase, periodo), null);
        }

        private async Task<AsistenciaListaDTO> ArmarLista(Sesion sesion, DateOnly fecha, bool puedeEditar)
        {
            var clase = sesion.Clase;

            var matriculas = await _asistenciaRepository.GetMatriculasDeClase(
                clase.IdPeriodo, clase.IdSeccion, clase.IdGrupo, clase.IdCurso);

            var registrados = (await _asistenciaRepository.GetDeSesion(clase.IdHorario, fecha))
                .ToDictionary(a => a.IdMatricula);

            bool cerrado = await _asistenciaRepository.GetCierre(clase.IdPeriodo) != null;
            bool yaRegistrada = registrados.Count > 0;

            string? aviso = null;

            if (cerrado)
                aviso = $"El registro de asistencia del período {sesion.Periodo.Nombre} está cerrado; solo se puede consultar.";
            else if (matriculas.Count == 0)
                aviso = "No hay estudiantes con matrícula activa en este curso para la sección y el grupo de la clase.";
            else if (yaRegistrada && !puedeEditar)
                aviso = "Esta lista ya fue registrada y su usuario no tiene permiso para modificarla.";

            return new AsistenciaListaDTO
            {
                Clase = HorarioService.ToDTO(clase),
                Fecha = fecha,
                YaRegistrada = yaRegistrada,
                PuedeGuardar = !cerrado && matriculas.Count > 0 && (!yaRegistrada || puedeEditar),
                Aviso = aviso,
                Estudiantes = matriculas.Select(m =>
                {
                    registrados.TryGetValue(m.IdMatricula, out var registro);

                    return new AsistenciaFilaDTO
                    {
                        IdMatricula = m.IdMatricula,
                        NombreEstudiante = NombreEstudiante(m.Estudiante),
                        Carnet = m.Estudiante?.Carnet ?? string.Empty,
                        Estado = registro?.Estado,
                        Observacion = registro?.Observacion
                    };
                }).ToList()
            };
        }

        /// Si quien consulta es docente, limita la búsqueda a sus clases. Devuelve un error o null.
        private async Task<string?> AplicarAlcance(int idUsuario, bool soloPropias, AsistenciaFiltro filtro)
        {
            if (!soloPropias)
                return null;

            var docente = await GetDocenteActivo(idUsuario);

            if (docente == null)
                return MensajeNoDocente;

            filtro.IdDocente = docente.IdDocente;

            return null;
        }

        /// Pasa los filtros de pantalla a la DAL. Sin período se usa el activo.
        private async Task<(AsistenciaFiltro? Filtro, string? Error)> PrepararFiltro(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO dto)
        {
            int? idPeriodo = dto.IdPeriodo ?? (await _periodoRepository.GetPeriodoActivo())?.IdPeriodo;

            if (!idPeriodo.HasValue)
                return (null, "Seleccione un período lectivo");

            if (dto.Desde.HasValue && dto.Hasta.HasValue && dto.Desde > dto.Hasta)
                return (null, "La fecha inicial no puede ser posterior a la fecha final");

            string? estado = null;

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                estado = AsistenciaReglas.NormalizarEstado(dto.Estado);

                if (estado == null)
                    return (null, "El estado seleccionado no es válido");
            }

            var filtro = new AsistenciaFiltro
            {
                IdPeriodo = idPeriodo,
                IdGrado = dto.IdGrado,
                IdSeccion = dto.IdSeccion,
                IdGrupo = dto.IdGrupo,
                IdCurso = dto.IdCurso,
                Texto = string.IsNullOrWhiteSpace(dto.Texto) ? null : dto.Texto.Trim(),
                Desde = dto.Desde,
                Hasta = dto.Hasta,
                Estado = estado
            };

            string? errorAlcance = await AplicarAlcance(idUsuario, soloPropias, filtro);

            if (errorAlcance != null)
                return (null, errorAlcance);

            return (filtro, null);
        }

        /// Conteos de la BD + nombres de estudiantes y cursos + porcentaje (MCAF-01-10)
        private async Task<List<AsistenciaResumenDTO>> CalcularResumen(AsistenciaFiltro filtroDal, AsistenciaFiltroDTO dto)
        {
            var conteos = await _asistenciaRepository.Contar(filtroDal);

            if (conteos.Count == 0)
                return new List<AsistenciaResumenDTO>();

            var matriculas = (await _asistenciaRepository.GetMatriculas(conteos.Select(c => c.IdMatricula)))
                .ToDictionary(m => m.IdMatricula);

            var cursos = (await _asistenciaRepository.GetCursos(conteos.Select(c => c.IdCurso)))
                .ToDictionary(c => c.IdCurso);

            decimal umbral = dto.Umbral is > 0 and <= 100 ? dto.Umbral.Value : AsistenciaReglas.UmbralSugerido;

            var lista = conteos.Select(c =>
            {
                matriculas.TryGetValue(c.IdMatricula, out var m);
                cursos.TryGetValue(c.IdCurso, out var curso);

                decimal porcentaje = AsistenciaReglas.CalcularPorcentaje(
                    c.Presentes, c.Ausentes, c.Tardias, c.Justificadas) ?? 0m;

                return new AsistenciaResumenDTO
                {
                    IdMatricula = c.IdMatricula,
                    NombreEstudiante = NombreEstudiante(m?.Estudiante),
                    Carnet = m?.Estudiante?.Carnet ?? string.Empty,
                    NombreGrado = m?.Seccion?.Grado?.Nombre ?? string.Empty,
                    NombreSeccion = m?.Seccion?.Nombre ?? string.Empty,
                    NombreGrupo = m?.Grupo?.Nombre ?? string.Empty,
                    IdCurso = c.IdCurso,
                    NombreCurso = curso == null ? string.Empty : $"{curso.Codigo} - {curso.Nombre}",
                    Presentes = c.Presentes,
                    Ausentes = c.Ausentes,
                    Tardias = c.Tardias,
                    Justificadas = c.Justificadas,
                    Total = c.Presentes + c.Ausentes + c.Tardias + c.Justificadas,
                    Porcentaje = porcentaje,
                    BajoUmbral = porcentaje < umbral
                };
            });

            if (dto.SoloBajoUmbral)
                lista = lista.Where(r => r.BajoUmbral);

            // Primero los casos de menor asistencia: es lo que se busca en un reporte
            return lista
                .OrderBy(r => r.Porcentaje)
                .ThenBy(r => r.NombreEstudiante)
                .ThenBy(r => r.NombreCurso)
                .ToList();
        }

        private async Task<CierreAsistenciaDTO> ArmarCierre(DAL.Entities.PeriodoLectivo periodo)
        {
            var cierre = await _asistenciaRepository.GetCierre(periodo.IdPeriodo);

            // Solo se cierra un período finalizado: a partir de su último día
            bool finalizado = AsistenciaReglas.Hoy() >= periodo.FechaFin;

            return new CierreAsistenciaDTO
            {
                IdPeriodo = periodo.IdPeriodo,
                NombrePeriodo = periodo.Nombre,
                Cerrado = cierre != null,
                FechaCierre = cierre?.FechaCierre,
                CerradoPor = cierre?.Usuario == null ? null : $"{cierre.Usuario.Nombre} {cierre.Usuario.Apellido}",
                PuedeCerrar = cierre == null && finalizado,
                Motivo = cierre == null && !finalizado
                    ? $"El período termina el {periodo.FechaFin:dd/MM/yyyy}; se podrá cerrar a partir de esa fecha"
                    : null
            };
        }

        private static AsistenciaRegistroDTO ToRegistroDTO(DAL.Entities.Asistencia a) => new()
        {
            IdAsistencia = a.IdAsistencia,
            Fecha = a.Fecha,
            Dia = a.Horario?.Dia ?? string.Empty,
            HoraInicio = a.Horario == null ? string.Empty : HorarioReglas.FormatearHora(a.Horario.HoraInicio),
            HoraFin = a.Horario == null ? string.Empty : HorarioReglas.FormatearHora(a.Horario.HoraFin),
            IdCurso = a.Horario?.IdCurso ?? 0,
            NombreCurso = a.Horario?.Curso == null ? string.Empty : $"{a.Horario.Curso.Codigo} - {a.Horario.Curso.Nombre}",
            NombreGrado = a.Horario?.Seccion?.Grado?.Nombre ?? string.Empty,
            NombreSeccion = a.Horario?.Seccion?.Nombre ?? string.Empty,
            NombreGrupo = a.Horario?.Grupo?.Nombre ?? string.Empty,
            NombreDocente = a.Horario?.Docente == null
                ? string.Empty
                : $"{a.Horario.Docente.Nombre} {a.Horario.Docente.Apellidos}",
            IdMatricula = a.IdMatricula,
            NombreEstudiante = NombreEstudiante(a.Matricula?.Estudiante),
            Carnet = a.Matricula?.Estudiante?.Carnet ?? string.Empty,
            Estado = a.Estado,
            Observacion = a.Observacion,
            Modificado = a.FechaModificacion.HasValue
        };

        // "Apellido Nombre": así se ordenan las listas de clase
        private static string NombreEstudiante(DAL.Entities.Estudiante? e) =>
            e?.Usuario == null ? string.Empty : $"{e.Usuario.Apellido} {e.Usuario.Nombre}";

        private static RespuestaDTO<AsistenciaListaDTO> FallaLista(string mensaje, int codigo) =>
            RespuestaDTO<AsistenciaListaDTO>.Error(mensaje, codigo);

        private static RespuestaDTO<CierreAsistenciaDTO> FallaCierre(string mensaje, int codigo) =>
            RespuestaDTO<CierreAsistenciaDTO>.Error(mensaje, codigo);
    }
}
