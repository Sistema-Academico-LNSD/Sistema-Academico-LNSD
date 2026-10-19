using ClosedXML.Excel;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Storage;
using ProyectoLNSD_WApp.DAL.Repositories.Curso;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.DocenteCurso;
using ProyectoLNSD_WApp.DAL.Repositories.Estudiante;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using ProyectoLNSD_WApp.DAL.Repositories.Recurso;
using System.Drawing;

namespace ProyectoLNSD_WApp.BLL.Service.Recurso
{
    public class RecursoService : IRecursoService
    {
        private const string MensajeSinPeriodoActivo =
            "No hay un período lectivo activo. Debe activarlo el administrador en Configuración.";

        private const string MensajeSoloDocentes =
            "Solo el personal docente activo puede administrar recursos académicos";

        private const string MensajeSoloEstudiantes =
    "Solo los estudiantes activos pueden consultar el material de estudio";

        private const string MensajeSinGrado =
            "Aún no tiene un grado asignado. Consulte con la administración.";

        private readonly IRecursoRepository _recursoRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IDocenteCursoRepository _docenteCursoRepository;
        private readonly ICursoRepository _cursoRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IAlmacenamientoRecursos _almacenamiento;

        public RecursoService(
            IRecursoRepository recursoRepository,
            IDocenteRepository docenteRepository,
            IDocenteCursoRepository docenteCursoRepository,
            ICursoRepository cursoRepository,
            IPeriodoLectivoRepository periodoRepository,
            IEstudianteRepository estudianteRepository,
            IAlmacenamientoRecursos almacenamiento)
        {
            _recursoRepository = recursoRepository;
            _docenteRepository = docenteRepository;
            _docenteCursoRepository = docenteCursoRepository;
            _cursoRepository = cursoRepository;
            _periodoRepository = periodoRepository;
            _estudianteRepository = estudianteRepository;
            _almacenamiento = almacenamiento;
        }

        public async Task<RespuestaDTO<List<CursoRecursoDTO>>> GetMisCursos(int idUsuario)
        {
            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return RespuestaDTO<List<CursoRecursoDTO>>.Error(MensajeSoloDocentes, 1201);

                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return RespuestaDTO<List<CursoRecursoDTO>>.Error(MensajeSinPeriodoActivo, 1202);

                var asignaciones = await _docenteCursoRepository.GetAsignaciones(docente.IdDocente);

                var cursos = new List<CursoRecursoDTO>();

                foreach (var a in asignaciones.Where(a =>
                             a.IdPeriodo == periodo.IdPeriodo &&
                             a.Curso != null &&
                             a.Curso.Estado))
                {
                    var grados = await _recursoRepository.GetGradosActivosDeCurso(a.IdCurso);

                    cursos.Add(new CursoRecursoDTO
                    {
                        IdCurso = a.IdCurso,
                        Codigo = a.Curso!.Codigo,
                        Nombre = a.Curso.Nombre,
                        Grados = grados.Select(g => new GradoRecursoDTO
                        {
                            IdGrado = g.IdGrado,
                            Nombre = g.Nombre
                        }).ToList()
                    });
                }

                return RespuestaDTO<List<CursoRecursoDTO>>.Exito(
                    cursos.OrderBy(c => c.Nombre).ToList(),
                    cursos.Count == 0 ? "No tiene cursos asignados en el período lectivo activo" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<CursoRecursoDTO>>.Error("No fue posible obtener sus cursos", 1214);
            }
        }

        public async Task<RespuestaDTO<RecursoDTO>> Publicar(
            int idUsuario,
            RecursoGuardarDTO dto,
            ArchivoRecursoDTO? archivo)
        {
            // Si algo falla después de guardar el archivo en disco, se borra para no dejar basura
            string? rutaGuardada = null;

            try
            {
                // 1. Solo docentes activos
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return Falla(MensajeSoloDocentes, 1201);

                // 2. El curso debe estar asignado al docente en el período activo (HU 01-01, esc. 4)
                var periodo = await _periodoRepository.GetPeriodoActivo();

                if (periodo == null)
                    return Falla(MensajeSinPeriodoActivo, 1202);

                if (!await _docenteCursoRepository.Existe(docente.IdDocente, dto.IdCurso, periodo.IdPeriodo))
                    return Falla(
                        "No tiene autorización para publicar recursos en un curso que no tiene asignado", 1203);

                // 3. Curso activo y grado que corresponda al curso
                var curso = await _cursoRepository.GetCursoById(dto.IdCurso);

                if (curso == null || !curso.Estado)
                    return Falla("El curso seleccionado no está disponible", 1204);

                if (!await _recursoRepository.CursoImpartidoEnGrado(dto.IdCurso, dto.IdGrado))
                    return Falla("El grado seleccionado no corresponde al curso", 1205);

                // 4. Modalidad y tipo de recurso (HU 01-02, esc. 3)
                bool esArchivo = dto.Modalidad == DAL.Entities.ModalidadRecurso.Archivo;
                bool esEnlace = dto.Modalidad == DAL.Entities.ModalidadRecurso.Enlace;

                if (!esArchivo && !esEnlace)
                    return Falla("Indique si el recurso es un archivo o un enlace", 1206);

                string[] tiposValidos = esArchivo ? RecursoReglas.TiposArchivo : RecursoReglas.TiposEnlace;

                if (!tiposValidos.Contains(dto.TipoRecurso))
                    return Falla(
                        $"El tipo de recurso no es válido. Opciones disponibles: {string.Join(", ", tiposValidos)}",
                        1206);

                // 5. Fecha de publicación (HU 01-02, esc. 4): sin fecha se usa hoy; no puede ser futura
                DateTime fecha = (dto.FechaPublicacion ?? DateTime.Today).Date;

                if (fecha.Year < 2000 || fecha > DateTime.Today)
                    return Falla("La fecha de publicación no es válida", 1207);

                var recurso = new DAL.Entities.RecursoAcademico
                {
                    IdDocente = docente.IdDocente,
                    IdCurso = dto.IdCurso,
                    IdGrado = dto.IdGrado,
                    Titulo = dto.Titulo.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                    TipoRecurso = dto.TipoRecurso,
                    Modalidad = dto.Modalidad,
                    FechaPublicacion = fecha
                };

                if (esArchivo)
                {
                    // 6a. Archivo: seleccionado, formato permitido y tamaño dentro del límite
                    if (archivo == null || archivo.Tamano <= 0)
                        return Falla("Debe seleccionar un archivo antes de continuar", 1208);

                    string nombreOriginal = Path.GetFileName(archivo.NombreOriginal);
                    string extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();

                    if (!RecursoReglas.ExtensionesPermitidas.Contains(extension))
                        return Falla(
                            $"Formato no permitido. Formatos admitidos: {string.Join(", ", RecursoReglas.ExtensionesPermitidas)}",
                            1209);

                    if (archivo.Tamano > RecursoReglas.TamanoMaximoBytes)
                        return Falla(MensajeTamanoMaximo(), 1210);

                    // 7a. Se guarda en disco y se calcula el hash para detectar duplicados en el curso
                    var guardado = await _almacenamiento.GuardarAsync(archivo.Contenido, extension);
                    rutaGuardada = guardado.Ruta;

                    if (guardado.Tamano > RecursoReglas.TamanoMaximoBytes)
                        return Falla(MensajeTamanoMaximo(), 1210);

                    if (await _recursoRepository.ExisteHash(dto.IdCurso, guardado.Hash))
                        return Falla("Este recurso ya fue registrado en el curso", 1211);

                    recurso.NombreArchivoOriginal = nombreOriginal.Length > 260
                        ? nombreOriginal[..260]
                        : nombreOriginal;
                    recurso.RutaArchivo = guardado.Ruta;
                    recurso.Extension = extension;
                    recurso.TamanoBytes = guardado.Tamano;
                    recurso.HashArchivo = guardado.Hash;
                }
                else
                {
                    // 6b. Enlace: URL válida con http o https (HU 01-03, esc. 2)
                    string texto = (dto.UrlEnlace ?? string.Empty).Trim();

                    if (!Uri.TryCreate(texto, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                        return Falla("Ingrese un enlace válido que comience con http:// o https://", 1212);

                    // Se guarda normalizada, así dos formas de escribir el mismo enlace cuentan como duplicado
                    string url = uri.AbsoluteUri;

                    if (url.Length > 500)
                        return Falla("El enlace es demasiado largo", 1212);

                    if (await _recursoRepository.ExisteUrl(dto.IdCurso, url))
                        return Falla("Este recurso ya fue registrado en el curso", 1211);

                    recurso.UrlEnlace = url;
                }

                // 8. Registro en la base de datos
                if (!await _recursoRepository.Crear(recurso))
                {
                    if (rutaGuardada != null)
                        _almacenamiento.Eliminar(rutaGuardada);

                    return Falla("No se pudo registrar el recurso. Intente nuevamente.", 1213);
                }

                // Ya quedó registrado: el archivo de disco pertenece al recurso y no se debe borrar
                rutaGuardada = null;

                var creado = await _recursoRepository.GetById(recurso.IdRecurso);

                return RespuestaDTO<RecursoDTO>.Exito(
                    ToDTO(creado ?? recurso),
                    "Recurso publicado correctamente");
            }
            catch (Exception)
            {
                if (rutaGuardada != null)
                    _almacenamiento.Eliminar(rutaGuardada);

                return Falla("No se pudo registrar el recurso. Intente nuevamente.", 1213);
            }
            finally
            {
                // Si se rechazó por validación después de guardar (duplicado o tamaño), se limpia el archivo
                if (rutaGuardada != null)
                    _almacenamiento.Eliminar(rutaGuardada);
            }
        }

        public async Task<RespuestaDTO<List<RecursoDTO>>> GetMisRecursos(int idUsuario, RecursoFiltroDTO filtro)
        {
            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return RespuestaDTO<List<RecursoDTO>>.Error(MensajeSoloDocentes, 1201);

                if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue &&
                    filtro.FechaDesde.Value.Date > filtro.FechaHasta.Value.Date)
                    return RespuestaDTO<List<RecursoDTO>>.Error(
                        "La fecha inicial no puede ser posterior a la fecha final", 1218);

                // HU 01-06, esc. 3: no se pueden consultar los recursos de un curso ajeno
                if (filtro.IdCurso.HasValue)
                {
                    var asignaciones = await _docenteCursoRepository.GetAsignaciones(docente.IdDocente);

                    if (!asignaciones.Any(a => a.IdCurso == filtro.IdCurso.Value))
                        return RespuestaDTO<List<RecursoDTO>>.Error(
                            "No tiene acceso a los recursos de un curso que no tiene asignado", 1215);
                }

                // El docente siempre consulta solo lo suyo, aunque el navegador envíe otro IdDocente
                var recursos = await _recursoRepository.Buscar(
                    docente.IdDocente,
                    filtro.IdCurso,
                    filtro.IdGrado,
                    filtro.Titulo,
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.Orden);

                var lista = recursos.Select(ToDTO).ToList();

                string? mensaje = null;

                if (lista.Count == 0)
                {
                    mensaje = HayFiltros(filtro)
                        ? "No se encontraron recursos con los filtros indicados"
                        : "No existen recursos publicados";
                }

                return RespuestaDTO<List<RecursoDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<RecursoDTO>>.Error("No fue posible consultar los recursos", 1220);
            }
        }

        public async Task<RespuestaDTO<RecursoDTO>> GetRecurso(int idUsuario, int idRecurso)
        {
            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return Falla(MensajeSoloDocentes, 1201);

                var recurso = await _recursoRepository.GetById(idRecurso);

                // Un recurso de otro docente se trata igual que uno inexistente
                if (recurso == null || recurso.IdDocente != docente.IdDocente)
                    return Falla("El recurso no fue encontrado", 1216);

                return RespuestaDTO<RecursoDTO>.Exito(ToDTO(recurso));
            }
            catch (Exception)
            {
                return Falla("No fue posible consultar el recurso", 1220);
            }
        }

        public async Task<RespuestaDTO<RecursoDTO>> Actualizar(
            int idUsuario,
            RecursoGuardarDTO dto,
            ArchivoRecursoDTO? archivo)
        {
            string? rutaNueva = null;
            bool actualizado = false;

            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return Falla(MensajeSoloDocentes, 1201);

                // HU 01-04, esc. 2: el recurso debe existir y ser del docente
                var actual = await _recursoRepository.GetById(dto.IdRecurso);

                if (actual == null || actual.IdDocente != docente.IdDocente)
                    return Falla("El recurso no fue encontrado", 1216);

                // El curso y la modalidad no cambian al editar (se toman del recurso guardado)
                if (!await _recursoRepository.CursoImpartidoEnGrado(actual.IdCurso, dto.IdGrado))
                    return Falla("El grado seleccionado no corresponde al curso", 1205);

                bool esArchivo = actual.Modalidad == DAL.Entities.ModalidadRecurso.Archivo;
                string[] tiposValidos = esArchivo ? RecursoReglas.TiposArchivo : RecursoReglas.TiposEnlace;

                if (!tiposValidos.Contains(dto.TipoRecurso))
                    return Falla(
                        $"El tipo de recurso no es válido. Opciones disponibles: {string.Join(", ", tiposValidos)}",
                        1206);

                DateTime fecha = (dto.FechaPublicacion ?? actual.FechaPublicacion).Date;

                if (fecha.Year < 2000 || fecha > DateTime.Today)
                    return Falla("La fecha de publicación no es válida", 1207);

                var editado = new DAL.Entities.RecursoAcademico
                {
                    IdRecurso = actual.IdRecurso,
                    IdDocente = actual.IdDocente,
                    IdCurso = actual.IdCurso,
                    IdGrado = dto.IdGrado,
                    Titulo = dto.Titulo.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                    TipoRecurso = dto.TipoRecurso,
                    Modalidad = actual.Modalidad,
                    NombreArchivoOriginal = actual.NombreArchivoOriginal,
                    RutaArchivo = actual.RutaArchivo,
                    Extension = actual.Extension,
                    TamanoBytes = actual.TamanoBytes,
                    HashArchivo = actual.HashArchivo,
                    UrlEnlace = actual.UrlEnlace,
                    FechaPublicacion = fecha,
                    FechaModificacion = DateTime.UtcNow   // HU 01-04, esc. 4
                };

                string? rutaAnterior = null;

                if (esArchivo)
                {
                    // Archivo nuevo opcional: si no se envía, se conserva el actual
                    if (archivo != null && archivo.Tamano > 0)
                    {
                        string nombreOriginal = Path.GetFileName(archivo.NombreOriginal);
                        string extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();

                        if (!RecursoReglas.ExtensionesPermitidas.Contains(extension))
                            return Falla(
                                $"Formato no permitido. Formatos admitidos: {string.Join(", ", RecursoReglas.ExtensionesPermitidas)}",
                                1209);

                        if (archivo.Tamano > RecursoReglas.TamanoMaximoBytes)
                            return Falla(MensajeTamanoMaximo(), 1210);

                        var guardado = await _almacenamiento.GuardarAsync(archivo.Contenido, extension);
                        rutaNueva = guardado.Ruta;

                        if (guardado.Tamano > RecursoReglas.TamanoMaximoBytes)
                            return Falla(MensajeTamanoMaximo(), 1210);

                        // Duplicado en el curso, sin compararlo contra sí mismo
                        if (await _recursoRepository.ExisteHash(actual.IdCurso, guardado.Hash, actual.IdRecurso))
                            return Falla("Este recurso ya fue registrado en el curso", 1211);

                        rutaAnterior = actual.RutaArchivo;

                        editado.NombreArchivoOriginal = nombreOriginal.Length > 260
                            ? nombreOriginal[..260]
                            : nombreOriginal;
                        editado.RutaArchivo = guardado.Ruta;
                        editado.Extension = extension;
                        editado.TamanoBytes = guardado.Tamano;
                        editado.HashArchivo = guardado.Hash;
                    }
                }
                else
                {
                    string texto = (dto.UrlEnlace ?? string.Empty).Trim();

                    if (!Uri.TryCreate(texto, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                        return Falla("Ingrese un enlace válido que comience con http:// o https://", 1212);

                    string url = uri.AbsoluteUri;

                    if (url.Length > 500)
                        return Falla("El enlace es demasiado largo", 1212);

                    if (await _recursoRepository.ExisteUrl(actual.IdCurso, url, actual.IdRecurso))
                        return Falla("Este recurso ya fue registrado en el curso", 1211);

                    editado.UrlEnlace = url;
                }

                if (!await _recursoRepository.Actualizar(editado))
                    return Falla("No se pudo actualizar el recurso. Intente nuevamente.", 1217);

                actualizado = true;

                // El archivo reemplazado ya no se usa
                if (rutaAnterior != null)
                    _almacenamiento.Eliminar(rutaAnterior);

                var resultado = await _recursoRepository.GetById(actual.IdRecurso);

                return RespuestaDTO<RecursoDTO>.Exito(
                    ToDTO(resultado ?? editado),
                    "Recurso actualizado correctamente");
            }
            catch (Exception)
            {
                return Falla("No se pudo actualizar el recurso. Intente nuevamente.", 1217);
            }
            finally
            {
                // Si no se actualizó, el archivo nuevo (si llegó a guardarse) se descarta
                if (!actualizado && rutaNueva != null)
                    _almacenamiento.Eliminar(rutaNueva);
            }
        }

        public async Task<RespuestaDTO<bool>> Eliminar(int idUsuario, int idRecurso)
        {
            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return RespuestaDTO<bool>.Error(MensajeSoloDocentes, 1201);

                var recurso = await _recursoRepository.GetById(idRecurso);

                if (recurso == null || recurso.IdDocente != docente.IdDocente)
                    return RespuestaDTO<bool>.Error("El recurso no fue encontrado", 1216);

                if (!await _recursoRepository.Eliminar(idRecurso))
                    return RespuestaDTO<bool>.Error("El recurso no fue encontrado", 1216);

                // Primero la base de datos y después el archivo: si el borrado del archivo
                // fallara, el recurso ya no aparece y no queda un registro apuntando a la nada.
                if (!string.IsNullOrEmpty(recurso.RutaArchivo))
                    _almacenamiento.Eliminar(recurso.RutaArchivo);

                return RespuestaDTO<bool>.Exito(true, "Recurso eliminado correctamente");
            }
            catch (Exception)
            {
                return RespuestaDTO<bool>.Error("No se pudo eliminar el recurso. Intente nuevamente.", 1219);
            }
        }

        public async Task<RespuestaDTO<FiltrosMaterialDTO>> GetFiltrosMaterial(int idUsuario)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetByUsuario(idUsuario);

                if (estudiante == null || !estudiante.Estado)
                    return RespuestaDTO<FiltrosMaterialDTO>.Error(MensajeSoloEstudiantes, 1221);

                if (!estudiante.IdGrado.HasValue)
                    return RespuestaDTO<FiltrosMaterialDTO>.Error(MensajeSinGrado, 1227);

                var cursos = await _recursoRepository.GetCursosActivosDeGrado(estudiante.IdGrado.Value);

                return RespuestaDTO<FiltrosMaterialDTO>.Exito(new FiltrosMaterialDTO
                {
                    IdGrado = estudiante.IdGrado.Value,
                    NombreGrado = estudiante.Grado?.Nombre ?? string.Empty,
                    Cursos = cursos.Select(c => new CursoRecursoDTO
                    {
                        IdCurso = c.IdCurso,
                        Codigo = c.Codigo,
                        Nombre = c.Nombre
                    }).ToList()
                });
            }
            catch (Exception)
            {
                return RespuestaDTO<FiltrosMaterialDTO>.Error("No fue posible cargar los filtros", 1220);
            }
        }

        public async Task<RespuestaDTO<List<RecursoDTO>>> GetMaterialEstudiante(int idUsuario, int? idCurso)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetByUsuario(idUsuario);

                if (estudiante == null || !estudiante.Estado)
                    return RespuestaDTO<List<RecursoDTO>>.Error(MensajeSoloEstudiantes, 1221);

                if (!estudiante.IdGrado.HasValue)
                    return RespuestaDTO<List<RecursoDTO>>.Error(MensajeSinGrado, 1227);

                // El curso pedido debe pertenecer al grado del estudiante
                if (idCurso.HasValue &&
                    !await _recursoRepository.CursoImpartidoEnGrado(idCurso.Value, estudiante.IdGrado.Value))
                    return RespuestaDTO<List<RecursoDTO>>.Error(
                        "No tiene acceso a los recursos de ese curso", 1222);

                // El grado siempre es el del estudiante, aunque el navegador envíe otro
                var recursos = await _recursoRepository.Buscar(
                    null, idCurso, estudiante.IdGrado, null, null, null, "fecha_desc");

                var lista = recursos.Select(ToDTO).ToList();

                string? mensaje = null;

                if (lista.Count == 0)
                {
                    mensaje = idCurso.HasValue
                        ? "No hay material disponible para el curso seleccionado"
                        : "No hay material disponible por el momento";
                }

                return RespuestaDTO<List<RecursoDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<RecursoDTO>>.Error("No fue posible consultar el material", 1220);
            }
        }

        public async Task<RespuestaDTO<RecursoDescargaDTO>> Descargar(int idUsuario, int idRecurso, bool esSupervisor)
        {
            try
            {
                var recurso = await _recursoRepository.GetById(idRecurso);

                if (recurso == null)
                    return RespuestaDTO<RecursoDescargaDTO>.Error("El recurso no fue encontrado", 1216);

                // HU 01-09, esc. 4: sin permisos sobre el recurso no se entrega nada
                if (!await TieneAcceso(idUsuario, recurso, esSupervisor))
                    return RespuestaDTO<RecursoDescargaDTO>.Error(
                        "No tiene permisos para acceder a este recurso", 1223);

                if (recurso.Modalidad != DAL.Entities.ModalidadRecurso.Archivo ||
                    string.IsNullOrEmpty(recurso.RutaArchivo))
                    return RespuestaDTO<RecursoDescargaDTO>.Error(
                        "Este recurso es un enlace externo y no tiene archivo para descargar", 1224);

                // HU 01-09, esc. 2: el archivo puede faltar en el servidor
                var contenido = _almacenamiento.Abrir(recurso.RutaArchivo);

                if (contenido == null)
                    return RespuestaDTO<RecursoDescargaDTO>.Error("El archivo no está disponible", 1225);

                return RespuestaDTO<RecursoDescargaDTO>.Exito(new RecursoDescargaDTO
                {
                    Contenido = contenido,
                    NombreArchivo = recurso.NombreArchivoOriginal ?? $"recurso{recurso.Extension}",
                    ContentType = RecursoReglas.ObtenerContentType(recurso.Extension)
                });
            }
            catch (Exception)
            {
                return RespuestaDTO<RecursoDescargaDTO>.Error("No fue posible descargar el archivo", 1226);
            }
        }


        public async Task<RespuestaDTO<FiltrosSupervisionDTO>> GetFiltrosSupervision()
        {
            try
            {
                var docentes = await _recursoRepository.GetDocentesConRecursos();
                var cursos = await _recursoRepository.GetCursosConRecursos();

                return RespuestaDTO<FiltrosSupervisionDTO>.Exito(new FiltrosSupervisionDTO
                {
                    Docentes = docentes.Select(d => new OpcionFiltroDTO { Id = d.Id, Nombre = d.Nombre }).ToList(),
                    Cursos = cursos.Select(c => new OpcionFiltroDTO { Id = c.Id, Nombre = c.Nombre }).ToList()
                });
            }
            catch (Exception)
            {
                return RespuestaDTO<FiltrosSupervisionDTO>.Error("No fue posible cargar los filtros", 1220);
            }
        }

        public async Task<RespuestaDTO<List<RecursoDTO>>> GetRecursosSupervision(RecursoFiltroDTO filtro)
        {
            try
            {
                // Solo se usan los filtros de docente y curso; el orden es por docente y curso
                var recursos = await _recursoRepository.Buscar(
                    filtro.IdDocente, filtro.IdCurso, null, null, null, null, "docente");

                var lista = recursos.Select(ToDTO).ToList();

                string? mensaje = null;

                if (lista.Count == 0)
                {
                    // HU 01-10, esc. 4
                    mensaje = filtro.IdDocente.HasValue || filtro.IdCurso.HasValue
                        ? "No se encontraron registros para los filtros seleccionados"
                        : "No existen recursos publicados";
                }

                return RespuestaDTO<List<RecursoDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<RecursoDTO>>.Error("No fue posible consultar los recursos", 1220);
            }
        }

        public async Task<RespuestaDTO<byte[]>> ExportarReporte(RecursoFiltroDTO filtro)
        {
            try
            {
                var recursos = await _recursoRepository.Buscar(
                    filtro.IdDocente, filtro.IdCurso, null, null, null, null, "docente");

                if (recursos.Count == 0)
                    return RespuestaDTO<byte[]>.Error(
                        "No hay registros para exportar con los filtros seleccionados", 1228);

                var lista = recursos.Select(ToDTO).ToList();

                return RespuestaDTO<byte[]>.Exito(GenerarLibro(lista, filtro));
            }
            catch (Exception)
            {
                return RespuestaDTO<byte[]>.Error("No fue posible generar el reporte", 1229);
            }
        }

        // --- Auxiliares ---

        private static RespuestaDTO<RecursoDTO> Falla(string mensaje, int codigo) =>
            RespuestaDTO<RecursoDTO>.Error(mensaje, codigo);

        private static string MensajeTamanoMaximo() =>
            $"El archivo supera el tamaño máximo permitido de {RecursoReglas.TamanoMaximoBytes / (1024 * 1024)} MB";

        private static RecursoDTO ToDTO(DAL.Entities.RecursoAcademico r) => new()
        {
            IdRecurso = r.IdRecurso,
            IdDocente = r.IdDocente,
            NombreDocente = r.Docente == null ? string.Empty : $"{r.Docente.Nombre} {r.Docente.Apellidos}",
            IdCurso = r.IdCurso,
            CodigoCurso = r.Curso?.Codigo ?? string.Empty,
            NombreCurso = r.Curso?.Nombre ?? string.Empty,
            IdGrado = r.IdGrado,
            NombreGrado = r.Grado?.Nombre ?? string.Empty,
            Titulo = r.Titulo,
            Descripcion = r.Descripcion,
            TipoRecurso = r.TipoRecurso,
            Modalidad = r.Modalidad,
            NombreArchivo = r.NombreArchivoOriginal,
            Extension = r.Extension,
            TamanoBytes = r.TamanoBytes,
            UrlEnlace = r.UrlEnlace,
            // La fecha de publicación la elige el docente: se devuelve tal cual, sin zona horaria
            FechaPublicacion = r.FechaPublicacion,
            FechaModificacion = r.FechaModificacion.HasValue
                ? DateTime.SpecifyKind(r.FechaModificacion.Value, DateTimeKind.Utc)
                : null
        };

        private static bool HayFiltros(RecursoFiltroDTO f) =>
        f.IdCurso.HasValue ||
        f.IdGrado.HasValue ||
        !string.IsNullOrWhiteSpace(f.Titulo) ||
        f.FechaDesde.HasValue ||
        f.FechaHasta.HasValue;

        // Quién puede abrir un recurso: el supervisor cualquiera, el docente los suyos y el
        // estudiante los de su grado (cuando exista Matrícula, aquí se cambia por sus cursos matriculados)
        private async Task<bool> TieneAcceso(int idUsuario, DAL.Entities.RecursoAcademico recurso, bool esSupervisor)
        {
            if (esSupervisor)
                return true;

            var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

            if (docente != null)
                return docente.Estado && docente.IdDocente == recurso.IdDocente;

            var estudiante = await _estudianteRepository.GetByUsuario(idUsuario);

            return estudiante != null &&
                   estudiante.Estado &&
                   estudiante.IdGrado == recurso.IdGrado;
        }

        private static byte[] GenerarLibro(List<RecursoDTO> lista, RecursoFiltroDTO filtro)
        {
            // Con un filtro activo, todos los registros comparten ese docente o curso
            string docente = filtro.IdDocente.HasValue ? lista[0].NombreDocente : "Todos";
            string curso = filtro.IdCurso.HasValue
                ? $"{lista[0].CodigoCurso} - {lista[0].NombreCurso}"
                : "Todos";

            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Recursos");

            hoja.Cell(1, 1).Value = "Sistema Académico LNSD - Reporte de recursos académicos";
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 14;

            hoja.Cell(2, 1).Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
            hoja.Cell(3, 1).Value = $"Docente: {docente}   |   Curso: {curso}   |   Total: {lista.Count}";

            string[] encabezados =
                { "Docente", "Curso", "Grado", "Título", "Tipo", "Modalidad", "Contenido", "Fecha de publicación" };

            const int filaEncabezado = 5;

            for (int i = 0; i < encabezados.Length; i++)
                hoja.Cell(filaEncabezado, i + 1).Value = encabezados[i];

            var rangoEncabezado = hoja.Range(filaEncabezado, 1, filaEncabezado, encabezados.Length);
            rangoEncabezado.Style.Font.Bold = true;
            rangoEncabezado.Style.Font.FontColor = XLColor.White;
            rangoEncabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#0D6EFD");

            int fila = filaEncabezado + 1;

            foreach (var r in lista)
            {
                hoja.Cell(fila, 1).Value = TextoSeguro(r.NombreDocente);
                hoja.Cell(fila, 2).Value = TextoSeguro($"{r.CodigoCurso} - {r.NombreCurso}");
                hoja.Cell(fila, 3).Value = TextoSeguro(r.NombreGrado);
                hoja.Cell(fila, 4).Value = TextoSeguro(r.Titulo);
                hoja.Cell(fila, 5).Value = TextoSeguro(r.TipoRecurso);
                hoja.Cell(fila, 6).Value = TextoSeguro(r.Modalidad);
                hoja.Cell(fila, 7).Value = TextoSeguro(
                    r.Modalidad == DAL.Entities.ModalidadRecurso.Archivo ? r.NombreArchivo : r.UrlEnlace);
                hoja.Cell(fila, 8).Value = r.FechaPublicacion;
                hoja.Cell(fila, 8).Style.DateFormat.Format = "dd/MM/yyyy";

                fila++;
            }

            hoja.Range(filaEncabezado, 1, fila - 1, encabezados.Length).SetAutoFilter();
            hoja.SheetView.FreezeRows(filaEncabezado);

            // Solo se ajusta el ancho de la tabla (no el del título, que es muy largo)
            hoja.Columns(1, encabezados.Length).AdjustToContents(filaEncabezado, fila - 1);

            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);

            return memoria.ToArray();
        }

        // Evita que Excel interprete como fórmula un texto escrito por un usuario (=, +, -, @)
        private static string TextoSeguro(string? texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return "=+-@".Contains(texto[0]) ? " " + texto : texto;
        }
    }
}