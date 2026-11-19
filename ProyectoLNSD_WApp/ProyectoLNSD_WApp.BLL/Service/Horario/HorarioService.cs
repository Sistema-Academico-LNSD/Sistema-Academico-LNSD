using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Curso;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.DocenteCurso;
using ProyectoLNSD_WApp.DAL.Repositories.Horario;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using ProyectoLNSD_WApp.DAL.Repositories.Matricula;

namespace ProyectoLNSD_WApp.BLL.Service.Horario
{
    public class HorarioService : IHorarioService
    {
        private const string MensajeSinPeriodoActivo =
            "No hay un período lectivo activo. Debe activarlo el administrador en Configuración.";

        private readonly IHorarioRepository _horarioRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IDocenteCursoRepository _docenteCursoRepository;
        private readonly ICursoRepository _cursoRepository;
        private readonly IMatriculaRepository _matriculaRepository;

        public HorarioService(
            IHorarioRepository horarioRepository,
            IPeriodoLectivoRepository periodoRepository,
            IDocenteRepository docenteRepository,
            IDocenteCursoRepository docenteCursoRepository,
            ICursoRepository cursoRepository,
            IMatriculaRepository matriculaRepository)
        {
            _horarioRepository = horarioRepository;
            _periodoRepository = periodoRepository;
            _docenteRepository = docenteRepository;
            _docenteCursoRepository = docenteCursoRepository;
            _cursoRepository = cursoRepository;
            _matriculaRepository = matriculaRepository;
        }

        // ---------- Opciones del formulario ----------

        public async Task<RespuestaDTO<OpcionesHorarioDTO>> GetOpciones()
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return RespuestaDTO<OpcionesHorarioDTO>.Error(MensajeSinPeriodoActivo, 1301);

                var secciones = await _horarioRepository.GetSeccionesActivas();

                return RespuestaDTO<OpcionesHorarioDTO>.Exito(new OpcionesHorarioDTO
                {
                    IdPeriodo = periodo.IdPeriodo,
                    NombrePeriodo = periodo.Nombre,
                    Dias = HorarioReglas.DiasLectivos.ToList(),
                    Secciones = secciones.Select(s => new SeccionHorarioDTO
                    {
                        IdSeccion = s.IdSeccion,
                        IdGrado = s.IdGrado,
                        NombreGrado = s.Grado?.Nombre ?? string.Empty,
                        Nombre = s.Nombre
                    }).ToList()
                });
            }
            catch (Exception)
            {
                return RespuestaDTO<OpcionesHorarioDTO>.Error("No fue posible cargar las opciones", 1309);
            }
        }

        public async Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetGrupos(int idSeccion)
        {
            try
            {
                var seccion = await _horarioRepository.GetSeccion(idSeccion);

                if (seccion == null || !seccion.Estado || seccion.Grado == null)
                    return RespuestaDTO<List<OpcionFiltroDTO>>.Error("La sección seleccionada no está disponible", 1302);

                // Solo los grupos del mismo nivel que el grado de la sección (igual que la matrícula)
                var grupos = await _horarioRepository.GetGruposPorNivel(seccion.Grado.Nivel);

                return RespuestaDTO<List<OpcionFiltroDTO>>.Exito(
                    grupos.Select(g => new OpcionFiltroDTO
                    {
                        Id = g.IdGrupo,
                        Nombre = $"{g.Nombre} ({g.Anio})"
                    }).ToList(),
                    grupos.Count == 0 ? "No hay grupos para el nivel de esta sección" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<OpcionFiltroDTO>>.Error("No fue posible cargar los grupos", 1309);
            }
        }

        public async Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetCursos(int idSeccion)
        {
            try
            {
                var seccion = await _horarioRepository.GetSeccion(idSeccion);

                if (seccion == null || !seccion.Estado)
                    return RespuestaDTO<List<OpcionFiltroDTO>>.Error("La sección seleccionada no está disponible", 1302);

                var cursos = await _horarioRepository.GetCursosActivosDeGrado(seccion.IdGrado);

                return RespuestaDTO<List<OpcionFiltroDTO>>.Exito(
                    cursos.Select(c => new OpcionFiltroDTO
                    {
                        Id = c.IdCurso,
                        Nombre = $"{c.Codigo} - {c.Nombre}"
                    }).ToList(),
                    cursos.Count == 0 ? "El grado de esta sección no tiene cursos activos" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<OpcionFiltroDTO>>.Error("No fue posible cargar los cursos", 1309);
            }
        }

        public async Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetDocentes(int idCurso)
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return RespuestaDTO<List<OpcionFiltroDTO>>.Error(MensajeSinPeriodoActivo, 1301);

                var docentes = await _horarioRepository.GetDocentesDeCurso(idCurso, periodo.IdPeriodo);

                return RespuestaDTO<List<OpcionFiltroDTO>>.Exito(
                    docentes.Select(d => new OpcionFiltroDTO
                    {
                        Id = d.IdDocente,
                        Nombre = $"{d.Nombre} {d.Apellidos}"
                    }).ToList(),
                    docentes.Count == 0 ? "Ningún docente tiene asignado este curso en el período activo" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<OpcionFiltroDTO>>.Error("No fue posible cargar los docentes", 1309);
            }
        }

        // ---------- Consulta ----------

        public async Task<RespuestaDTO<List<HorarioDTO>>> GetHorarios(HorarioFiltroDTO filtro)
        {
            try
            {
                // Sin período indicado se muestra el período activo
                int? idPeriodo = filtro.IdPeriodo;

                if (!idPeriodo.HasValue)
                {
                    var activo = await _periodoRepository.GetPeriodoActivo();

                    if (activo == null)
                        return RespuestaDTO<List<HorarioDTO>>.Exito(new List<HorarioDTO>(), MensajeSinPeriodoActivo);

                    idPeriodo = activo.IdPeriodo;
                }

                var horarios = await _horarioRepository.Buscar(new HorarioFiltro
                {
                    IdPeriodo = idPeriodo,
                    IdGrado = filtro.IdGrado,
                    IdSeccion = filtro.IdSeccion,
                    IdGrupo = filtro.IdGrupo,
                    IdCurso = filtro.IdCurso,
                    IdDocente = filtro.IdDocente,
                    Dia = filtro.Dia,
                    Aula = filtro.Aula,
                    Estado = filtro.Estado
                });

                var lista = horarios
                    .OrderBy(h => HorarioReglas.OrdenDia(h.Dia))
                    .ThenBy(h => h.HoraInicio)
                    .ThenBy(h => h.Seccion?.Grado?.Nombre)
                    .ThenBy(h => h.Seccion?.Nombre)
                    .Select(ToDTO)
                    .ToList();

                string? mensaje = null;

                if (lista.Count == 0)
                {
                    mensaje = HayFiltros(filtro)
                        ? "No se encontraron horarios con los criterios indicados"
                        : "No existen horarios registrados en este período";
                }

                return RespuestaDTO<List<HorarioDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<HorarioDTO>>.Error("No fue posible consultar los horarios", 1309);
            }
        }

        // ---------- Registro (MHOF-01-01 a 01-04) ----------

        public async Task<RespuestaDTO<HorarioDTO>> Registrar(HorarioGuardarDTO dto)
        {
            try
            {
                // Los horarios se registran siempre en el período lectivo activo
                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return Falla(MensajeSinPeriodoActivo, 1301);

                var (horario, error) = await ValidarBloque(periodo.IdPeriodo, dto, null);

                if (error != null)
                    return error;

                horario!.Estado = true;

                if (!await _horarioRepository.Crear(horario))
                    return Falla("No se pudo registrar el horario. Intente nuevamente.", 1309);

                var creado = await _horarioRepository.GetById(horario.IdHorario);

                return RespuestaDTO<HorarioDTO>.Exito(
                    ToDTO(creado ?? horario),
                    "Horario registrado correctamente");
            }
            catch (Exception)
            {
                return Falla("No se pudo registrar el horario. Intente nuevamente.", 1309);
            }
        }

        // ---------- Detalle y períodos (MHOF-01-12 y 01-11) ----------

        public async Task<RespuestaDTO<HorarioDTO>> GetHorario(int idHorario)
        {
            try
            {
                var horario = await _horarioRepository.GetById(idHorario);

                if (horario == null)
                    return Falla("El horario no fue encontrado", 1308);

                return RespuestaDTO<HorarioDTO>.Exito(ToDTO(horario));
            }
            catch (Exception)
            {
                return Falla("No fue posible consultar el horario", 1309);
            }
        }

        public async Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetPeriodos()
        {
            try
            {
                var periodos = await _horarioRepository.GetPeriodos();
                var activo = await _periodoRepository.GetPeriodoActivo();

                return RespuestaDTO<List<OpcionFiltroDTO>>.Exito(
                    periodos.Select(p => new OpcionFiltroDTO
                    {
                        Id = p.IdPeriodo,
                        Nombre = p.IdPeriodo == activo?.IdPeriodo ? $"{p.Nombre} (activo)" : p.Nombre
                    }).ToList());
            }
            catch (Exception)
            {
                return RespuestaDTO<List<OpcionFiltroDTO>>.Error("No fue posible cargar los períodos", 1309);
            }
        }

        // ---------- Modificación (MHOF-01-05) ----------

        public async Task<RespuestaDTO<HorarioDTO>> Actualizar(HorarioGuardarDTO dto)
        {
            try
            {
                // Esc. 2: el horario puede haber sido eliminado desde otra pantalla
                var actual = await _horarioRepository.GetById(dto.IdHorario);

                if (actual == null)
                    return Falla("El horario no fue encontrado", 1308);

                var errorPeriodo = await VerificarPeriodoVigente(actual);

                if (errorPeriodo != null)
                    return errorPeriodo;

                // Esc. 3: mismas validaciones del registro, sin compararlo contra sí mismo
                var (editado, error) = await ValidarBloque(actual.IdPeriodo, dto, actual.IdHorario);

                if (error != null)
                    return error;

                if (!await _horarioRepository.Actualizar(editado!))
                    return Falla("No se pudo actualizar el horario. Intente nuevamente.", 1309);

                var resultado = await _horarioRepository.GetById(actual.IdHorario);

                return RespuestaDTO<HorarioDTO>.Exito(
                    ToDTO(resultado ?? editado!),
                    "Horario actualizado correctamente");
            }
            catch (Exception)
            {
                return Falla("No se pudo actualizar el horario. Intente nuevamente.", 1309);
            }
        }

        // ---------- Activar o inactivar (MHOF-01-06) ----------

        public async Task<RespuestaDTO<HorarioDTO>> CambiarEstado(int idHorario, bool activar)
        {
            try
            {
                var actual = await _horarioRepository.GetById(idHorario);

                if (actual == null)
                    return Falla("El horario no fue encontrado", 1308);

                if (actual.Estado == activar)
                    return Falla(
                        activar ? "El horario ya se encuentra activo" : "El horario ya se encuentra inactivo",
                        1310);

                if (activar)
                {
                    var errorPeriodo = await VerificarPeriodoVigente(actual);

                    if (errorPeriodo != null)
                        return errorPeriodo;

                    // Esc. 1 y 3: al reactivar se valida de nuevo que todo siga disponible
                    // y que no choque con otro horario que haya quedado vigente mientras tanto
                    var datos = new HorarioGuardarDTO
                    {
                        IdHorario = actual.IdHorario,
                        IdSeccion = actual.IdSeccion,
                        IdGrupo = actual.IdGrupo,
                        IdCurso = actual.IdCurso,
                        IdDocente = actual.IdDocente,
                        Dia = actual.Dia,
                        HoraInicio = HorarioReglas.FormatearHora(actual.HoraInicio),
                        HoraFin = HorarioReglas.FormatearHora(actual.HoraFin),
                        Aula = actual.Aula
                    };

                    var (_, error) = await ValidarBloque(actual.IdPeriodo, datos, actual.IdHorario);

                    if (error != null)
                        return error;
                }

                if (!await _horarioRepository.CambiarEstado(idHorario, activar))
                    return Falla("El horario no fue encontrado", 1308);

                var resultado = await _horarioRepository.GetById(idHorario);

                return RespuestaDTO<HorarioDTO>.Exito(
                    ToDTO(resultado ?? actual),
                    activar ? "Horario activado correctamente" : "Horario inactivado correctamente");
            }
            catch (Exception)
            {
                return Falla("No se pudo cambiar el estado del horario. Intente nuevamente.", 1309);
            }
        }

        // ---------- Horario del docente (MHOF-01-07 y 01-10) ----------

        public async Task<RespuestaDTO<List<HorarioDTO>>> GetMiHorarioDocente(int idUsuario, int? idPeriodo)
        {
            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return RespuestaDTO<List<HorarioDTO>>.Error(
                        "Solo el personal docente activo puede consultar su horario", 1311);

                return await ConsultarClasesDeDocente(
                    docente.IdDocente, idPeriodo, "No tiene clases asignadas en el período seleccionado");
            }
            catch (Exception)
            {
                return RespuestaDTO<List<HorarioDTO>>.Error("No fue posible consultar su horario", 1309);
            }
        }

        public async Task<RespuestaDTO<List<HorarioDTO>>> GetHorariosDeDocente(int idDocente, int? idPeriodo)
        {
            try
            {
                var docente = await _docenteRepository.GetById(idDocente);

                if (docente == null)
                    return RespuestaDTO<List<HorarioDTO>>.Error("El docente seleccionado no existe", 1302);

                return await ConsultarClasesDeDocente(
                    docente.IdDocente, idPeriodo, "El docente no tiene horarios registrados en el período seleccionado");
            }
            catch (Exception)
            {
                return RespuestaDTO<List<HorarioDTO>>.Error("No fue posible consultar los horarios del docente", 1309);
            }
        }

        public async Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetDocentesConsulta()
        {
            try
            {
                // Docentes activos; así también se puede consultar a uno sin clases y ver el mensaje correspondiente
                var docentes = await _docenteRepository.Buscar(null, null, null, true);

                return RespuestaDTO<List<OpcionFiltroDTO>>.Exito(
                    docentes
                        .OrderBy(d => d.Apellidos)
                        .ThenBy(d => d.Nombre)
                        .Select(d => new OpcionFiltroDTO
                        {
                            Id = d.IdDocente,
                            Nombre = $"{d.Apellidos} {d.Nombre}"
                        }).ToList());
            }
            catch (Exception)
            {
                return RespuestaDTO<List<OpcionFiltroDTO>>.Error("No fue posible cargar los docentes", 1309);
            }
        }

        // ---------- Horario según la matrícula (MHOF-01-08 y 01-09) ----------

        public async Task<RespuestaDTO<List<HorarioMatriculaDTO>>> GetHorariosDeMatriculas(
            int idUsuario, bool esEncargado)
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return RespuestaDTO<List<HorarioMatriculaDTO>>.Exito(
                        new List<HorarioMatriculaDTO>(), MensajeSinPeriodoActivo);

                // Solo matrículas vigentes: activas y del período lectivo activo (HU 01-09, esc. 2).
                // El estudiante o el encargado se identifican por la sesión, nunca por un dato del navegador.
                var matriculas = (await _matriculaRepository.MatriculasDeUsuario(idUsuario, esEncargado))
                    .Where(m => m.Estado == "Activa" && m.IdPeriodo == periodo.IdPeriodo)
                    .OrderBy(m => m.Estudiante?.Usuario?.Apellido)
                    .ThenBy(m => m.Estudiante?.Usuario?.Nombre)
                    .ToList();

                if (matriculas.Count == 0)
                    return RespuestaDTO<List<HorarioMatriculaDTO>>.Exito(
                        new List<HorarioMatriculaDTO>(),
                        esEncargado
                            ? "Ninguno de sus estudiantes tiene una matrícula vigente"
                            : "No existe una matrícula vigente para el período lectivo activo");

                var resultado = new List<HorarioMatriculaDTO>();

                foreach (var m in matriculas)
                {
                    // Solo las clases vigentes (HU 01-09, esc. 4) de su sección y grupo,
                    // y únicamente de los cursos en los que está matriculado
                    var horarios = await _horarioRepository.Buscar(new HorarioFiltro
                    {
                        IdPeriodo = m.IdPeriodo,
                        IdSeccion = m.IdSeccion,
                        IdGrupo = m.IdGrupo,
                        Estado = true,
                        IdsCursos = m.MatriculasCursos.Select(c => c.IdCurso).ToList()
                    });

                    var usuario = m.Estudiante?.Usuario;

                    resultado.Add(new HorarioMatriculaDTO
                    {
                        IdMatricula = m.IdMatricula,
                        NombreEstudiante = usuario == null ? string.Empty : $"{usuario.Nombre} {usuario.Apellido}",
                        NombrePeriodo = periodo.Nombre,
                        NombreGrado = m.Seccion?.Grado?.Nombre ?? string.Empty,
                        NombreSeccion = m.Seccion?.Nombre ?? string.Empty,
                        NombreGrupo = m.Grupo?.Nombre ?? string.Empty,
                        Horarios = horarios
                            .OrderBy(h => HorarioReglas.OrdenDia(h.Dia))
                            .ThenBy(h => h.HoraInicio)
                            .Select(ToDTO)
                            .ToList()
                    });
                }

                return RespuestaDTO<List<HorarioMatriculaDTO>>.Exito(resultado);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<HorarioMatriculaDTO>>.Error("No fue posible consultar el horario", 1309);
            }
        }

        // ---------- Validaciones compartidas ----------
        // Las usa el registro y, en el corte 2, también la edición y la reactivación.
        // excluirId es el horario que se edita, para que no choque contra sí mismo.

        private async Task<(DAL.Entities.Horario? Horario, RespuestaDTO<HorarioDTO>? Error)> ValidarBloque(
            int idPeriodo,
            HorarioGuardarDTO dto,
            int? excluirId)
        {
            // 1. Sección y grado activos (HU 01-01, esc. 3)
            var seccion = await _horarioRepository.GetSeccion(dto.IdSeccion);

            if (seccion == null)
                return (null, Falla("La sección seleccionada no existe", 1302));

            if (!seccion.Estado)
                return (null, Falla($"La sección {seccion.Nombre} está inactiva", 1302));

            var grado = seccion.Grado;

            if (grado == null || !grado.Estado)
                return (null, Falla($"El grado de la sección {seccion.Nombre} está inactivo", 1302));

            // 2. Grupo, y que corresponda al nivel del grado (HU 01-01, esc. 4)
            var grupo = await _horarioRepository.GetGrupo(dto.IdGrupo);

            if (grupo == null)
                return (null, Falla("El grupo seleccionado no existe", 1302));

            if (!string.Equals(grupo.Nivel.Trim(), grado.Nivel.Trim(), StringComparison.OrdinalIgnoreCase))
                return (null, Falla($"El grupo {grupo.Nombre} no corresponde al nivel de {grado.Nombre}", 1303));

            // 3. Curso activo y que se imparta en ese grado
            var curso = await _cursoRepository.GetCursoById(dto.IdCurso);

            if (curso == null)
                return (null, Falla("El curso seleccionado no existe", 1302));

            if (!curso.Estado)
                return (null, Falla($"El curso {curso.Nombre} está inactivo", 1302));

            if (!await _horarioRepository.CursoImpartidoEnGrado(curso.IdCurso, grado.IdGrado))
                return (null, Falla($"El curso {curso.Nombre} no se imparte en {grado.Nombre}", 1303));

            // 4. Docente activo y con ese curso asignado en el período
            var docente = await _docenteRepository.GetById(dto.IdDocente);

            if (docente == null)
                return (null, Falla("El docente seleccionado no existe", 1302));

            if (!docente.Estado)
                return (null, Falla($"El docente {docente.Nombre} {docente.Apellidos} está inactivo", 1302));

            if (!await _docenteCursoRepository.Existe(docente.IdDocente, curso.IdCurso, idPeriodo))
                return (null, Falla(
                    $"El docente {docente.Nombre} {docente.Apellidos} no tiene asignado el curso {curso.Nombre} en este período lectivo",
                    1303));

            // 5. Día lectivo, horas y aula (HU 01-02, esc. 2 y 4)
            string? dia = HorarioReglas.NormalizarDia(dto.Dia);

            if (dia == null)
                return (null, Falla(
                    $"El día seleccionado no está disponible para clases. Días lectivos: {string.Join(", ", HorarioReglas.DiasLectivos)}",
                    1304));

            if (!HorarioReglas.TryLeerHora(dto.HoraInicio, out var inicio) ||
                !HorarioReglas.TryLeerHora(dto.HoraFin, out var fin))
                return (null, Falla("Las horas no tienen un formato válido (HH:mm)", 1304));

            if (fin <= inicio)
                return (null, Falla("La hora de finalización debe ser posterior a la hora de inicio", 1304));

            string aula = HorarioReglas.NormalizarAula(dto.Aula);

            if (aula.Length == 0)
                return (null, Falla("Indique el aula", 1304));

            // 6. Conflictos con otros horarios activos del período (HU 01-02 esc. 3, 01-03 y 01-04)
            var conflictoDocente = await _horarioRepository.BuscarConflictoDocente(
                idPeriodo, docente.IdDocente, dia, inicio, fin, excluirId);

            if (conflictoDocente != null)
                return (null, Falla(
                    $"El docente ya tiene otra clase en ese horario: {Describir(conflictoDocente)}", 1305));

            var conflictoGrupo = await _horarioRepository.BuscarConflictoGrupo(
                idPeriodo, seccion.IdSeccion, grupo.IdGrupo, dia, inicio, fin, excluirId);

            if (conflictoGrupo != null)
                return (null, Falla(
                    $"La sección y el grupo ya tienen otra clase en ese horario: {Describir(conflictoGrupo)}", 1306));

            var conflictoAula = await _horarioRepository.BuscarConflictoAula(
                idPeriodo, aula, dia, inicio, fin, excluirId);

            if (conflictoAula != null)
                return (null, Falla(
                    $"El aula {aula} ya está ocupada en ese horario: {Describir(conflictoAula)}", 1307));

            return (new DAL.Entities.Horario
            {
                IdHorario = excluirId ?? 0,
                IdPeriodo = idPeriodo,
                IdSeccion = seccion.IdSeccion,
                IdGrupo = grupo.IdGrupo,
                IdCurso = curso.IdCurso,
                IdDocente = docente.IdDocente,
                Dia = dia,
                HoraInicio = inicio,
                HoraFin = fin,
                Aula = aula
            }, null);
        }

        // ---------- Auxiliares ----------

        private static RespuestaDTO<HorarioDTO> Falla(string mensaje, int codigo) =>
            RespuestaDTO<HorarioDTO>.Error(mensaje, codigo);

        // Texto que identifica el horario con el que se produce un conflicto
        private static string Describir(DAL.Entities.Horario h) =>
            $"{h.Curso?.Nombre} ({h.Seccion?.Grado?.Nombre} {h.Seccion?.Nombre}), " +
            $"{h.Dia} {HorarioReglas.FormatearHora(h.HoraInicio)} a {HorarioReglas.FormatearHora(h.HoraFin)}, aula {h.Aula}";

        private static bool HayFiltros(HorarioFiltroDTO f) =>
            f.IdGrado.HasValue ||
            f.IdSeccion.HasValue ||
            f.IdGrupo.HasValue ||
            f.IdCurso.HasValue ||
            f.IdDocente.HasValue ||
            !string.IsNullOrWhiteSpace(f.Dia) ||
            !string.IsNullOrWhiteSpace(f.Aula) ||
            f.Estado.HasValue;

        // internal: también lo usa AsistenciaService para mostrar las clases igual que Horarios
        internal static HorarioDTO ToDTO(DAL.Entities.Horario h) => new()
        {
            IdHorario = h.IdHorario,
            IdPeriodo = h.IdPeriodo,
            NombrePeriodo = h.Periodo?.Nombre ?? string.Empty,
            IdGrado = h.Seccion?.IdGrado ?? 0,
            NombreGrado = h.Seccion?.Grado?.Nombre ?? string.Empty,
            IdSeccion = h.IdSeccion,
            NombreSeccion = h.Seccion?.Nombre ?? string.Empty,
            IdGrupo = h.IdGrupo,
            NombreGrupo = h.Grupo?.Nombre ?? string.Empty,
            IdCurso = h.IdCurso,
            CodigoCurso = h.Curso?.Codigo ?? string.Empty,
            NombreCurso = h.Curso?.Nombre ?? string.Empty,
            IdDocente = h.IdDocente,
            NombreDocente = h.Docente == null ? string.Empty : $"{h.Docente.Nombre} {h.Docente.Apellidos}",
            Dia = h.Dia,
            HoraInicio = HorarioReglas.FormatearHora(h.HoraInicio),
            HoraFin = HorarioReglas.FormatearHora(h.HoraFin),
            Aula = h.Aula,
            Estado = h.Estado
        };

        // Solo se pueden modificar o activar horarios del período lectivo vigente
        private async Task<RespuestaDTO<HorarioDTO>?> VerificarPeriodoVigente(DAL.Entities.Horario horario)
        {
            var activo = await _periodoRepository.GetPeriodoActivo();

            if (activo == null)
                return Falla(MensajeSinPeriodoActivo, 1301);

            if (activo.IdPeriodo != horario.IdPeriodo)
                return Falla(
                    $"El período lectivo {horario.Periodo?.Nombre} ya no está activo, por lo que el horario no se puede modificar ni activar",
                    1301);

            return null;
        }

        private async Task<RespuestaDTO<List<HorarioDTO>>> ConsultarClasesDeDocente(
    int idDocente, int? idPeriodo, string mensajeVacio)
        {
            // Sin período indicado se usa el activo
            if (!idPeriodo.HasValue)
            {
                var activo = await _periodoRepository.GetPeriodoActivo();

                if (activo == null)
                    return RespuestaDTO<List<HorarioDTO>>.Exito(new List<HorarioDTO>(), MensajeSinPeriodoActivo);

                idPeriodo = activo.IdPeriodo;
            }

            // Solo las clases vigentes: los horarios inactivos no se muestran
            var horarios = await _horarioRepository.Buscar(new HorarioFiltro
            {
                IdPeriodo = idPeriodo,
                IdDocente = idDocente,
                Estado = true
            });

            var lista = horarios
                .OrderBy(h => HorarioReglas.OrdenDia(h.Dia))
                .ThenBy(h => h.HoraInicio)
                .Select(ToDTO)
                .ToList();

            return RespuestaDTO<List<HorarioDTO>>.Exito(lista, lista.Count == 0 ? mensajeVacio : null);
        }
    }
}