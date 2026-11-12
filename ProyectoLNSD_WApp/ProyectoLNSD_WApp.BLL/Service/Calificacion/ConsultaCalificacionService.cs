using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Calificacion;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.Encargado;
using ProyectoLNSD_WApp.DAL.Repositories.Estudiante;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using Ent = ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    public class ConsultaCalificacionService : IConsultaCalificacionService
    {
        private const int MaximoFilas = 500;

        private readonly IResultadoCalificacionService _resultados;
        private readonly ICalificacionRepository _calificacionRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IEncargadoRepository _encargadoRepository;
        private readonly IDocenteRepository _docenteRepository;

        public ConsultaCalificacionService(
            IResultadoCalificacionService resultados,
            ICalificacionRepository calificacionRepository,
            IPeriodoLectivoRepository periodoRepository,
            IEstudianteRepository estudianteRepository,
            IEncargadoRepository encargadoRepository,
            IDocenteRepository docenteRepository)
        {
            _resultados = resultados;
            _calificacionRepository = calificacionRepository;
            _periodoRepository = periodoRepository;
            _estudianteRepository = estudianteRepository;
            _encargadoRepository = encargadoRepository;
            _docenteRepository = docenteRepository;
        }

        // ---------- Estudiante (MCNF-01-09) ----------
        // El estudiante se obtiene de la sesión, nunca de lo que envía el navegador.
        // Ve sus notas por actividad, pero no las observaciones internas del docente.

        public async Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> GetMisPeriodos(int idUsuario)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetByUsuario(idUsuario);

                if (estudiante == null)
                    return RespuestaDTO<List<PeriodoCalificacionDTO>>.Error("Su usuario no tiene expediente de estudiante", 1429);

                return await PeriodosDeEstudiante(estudiante.IdEstudiante);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<PeriodoCalificacionDTO>>.Error("No fue posible cargar sus períodos", 1409);
            }
        }

        public async Task<RespuestaDTO<DetalleEstudianteDTO>> GetMiDetalle(int idUsuario, int idPeriodo)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetByUsuario(idUsuario);

                if (estudiante == null)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("Su usuario no tiene expediente de estudiante", 1429);

                return await ArmarDetalle(estudiante, idPeriodo, null, false);
            }
            catch (Exception)
            {
                return RespuestaDTO<DetalleEstudianteDTO>.Error("No fue posible consultar sus calificaciones", 1409);
            }
        }

        // ---------- Encargado (MCNF-01-10) ----------

        public async Task<RespuestaDTO<List<EstudianteResumenDTO>>> GetMisEstudiantes(int idUsuario)
        {
            try
            {
                var vinculos = await _encargadoRepository.GetPorUsuario(idUsuario);

                var lista = vinculos
                    .Where(v => v.Estudiante != null)
                    .Select(v => new EstudianteResumenDTO
                    {
                        IdEstudiante = v.IdEstudiante,
                        Carnet = v.Estudiante!.Carnet,
                        Nombre = v.Estudiante.Usuario == null
                            ? string.Empty
                            : $"{v.Estudiante.Usuario.Apellido} {v.Estudiante.Usuario.Nombre}",
                        Grado = v.Estudiante.Grado?.Nombre
                    }).ToList();

                return RespuestaDTO<List<EstudianteResumenDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No tiene estudiantes asociados a su cuenta" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<EstudianteResumenDTO>>.Error("No fue posible cargar sus estudiantes", 1409);
            }
        }

        public async Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> GetPeriodosDeMiEstudiante(int idUsuario, int idEstudiante)
        {
            try
            {
                if (await ObtenerEstudianteDelEncargado(idUsuario, idEstudiante) == null)
                    return RespuestaDTO<List<PeriodoCalificacionDTO>>.Error("El estudiante no está asociado a su cuenta", 1430);

                return await PeriodosDeEstudiante(idEstudiante);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<PeriodoCalificacionDTO>>.Error("No fue posible cargar los períodos", 1409);
            }
        }

        public async Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalleDeMiEstudiante(int idUsuario, int idEstudiante, int idPeriodo)
        {
            try
            {
                var estudiante = await ObtenerEstudianteDelEncargado(idUsuario, idEstudiante);

                if (estudiante == null)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("El estudiante no está asociado a su cuenta", 1430);

                return await ArmarDetalle(estudiante, idPeriodo, null, false);
            }
            catch (Exception)
            {
                return RespuestaDTO<DetalleEstudianteDTO>.Error("No fue posible consultar las calificaciones", 1409);
            }
        }

        // ---------- Administrador y director (MCNF-01-11) ----------

        public async Task<RespuestaDTO<List<ConsultaFilaDTO>>> GetConsulta(
            int idPeriodo, int? idCurso, int? idSeccion, int? idGrupo, string? texto)
        {
            try
            {
                var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

                if (periodo == null)
                    return RespuestaDTO<List<ConsultaFilaDTO>>.Error("El período lectivo seleccionado no existe", 1404);

                string filtro = (texto ?? string.Empty).Trim().ToLowerInvariant();

                var clases = (await _calificacionRepository.GetClasesConActividades(idPeriodo))
                    .Where(h => (idCurso == null || h.IdCurso == idCurso)
                             && (idSeccion == null || h.IdSeccion == idSeccion)
                             && (idGrupo == null || h.IdGrupo == idGrupo))
                    .GroupBy(h => new { h.IdCurso, h.IdSeccion, h.IdGrupo })
                    .Select(g => g.First())
                    .ToList();

                var filas = new List<ConsultaFilaDTO>();
                bool truncado = false;

                foreach (var clase in clases)
                {
                    var matriculas = await _calificacionRepository.GetMatriculasDeClase(
                        idPeriodo, clase.IdCurso, clase.IdSeccion, clase.IdGrupo);

                    foreach (var m in matriculas)
                    {
                        var usuario = m.Estudiante?.Usuario;
                        string nombre = usuario == null ? string.Empty : $"{usuario.Apellido} {usuario.Nombre}";
                        string carnet = m.Estudiante?.Carnet ?? string.Empty;

                        if (filtro.Length > 0 && !$"{nombre} {carnet}".ToLowerInvariant().Contains(filtro))
                            continue;

                        if (filas.Count >= MaximoFilas)
                        {
                            truncado = true;
                            break;
                        }

                        var resultado = await _resultados.GetResultadoCurso(m.IdEstudiante, clase.IdCurso, idPeriodo);
                        var delPeriodo = resultado.Periodos.FirstOrDefault(p => p.IdPeriodo == idPeriodo);

                        filas.Add(new ConsultaFilaDTO
                        {
                            IdEstudiante = m.IdEstudiante,
                            Carnet = carnet,
                            Estudiante = nombre,
                            IdCurso = clase.IdCurso,
                            Curso = clase.Curso?.Nombre ?? string.Empty,
                            Grado = clase.Seccion?.Grado?.Nombre ?? string.Empty,
                            Seccion = clase.Seccion?.Nombre ?? string.Empty,
                            Grupo = clase.Grupo?.Nombre ?? string.Empty,
                            Promedio = delPeriodo?.Promedio,
                            NotaAnual = resultado.NotaAnual,
                            Estado = resultado.Estado
                        });
                    }

                    if (truncado)
                        break;
                }

                filas = filas.OrderBy(f => f.Estudiante).ThenBy(f => f.Curso).ToList();

                return RespuestaDTO<List<ConsultaFilaDTO>>.Exito(
                    filas,
                    truncado
                        ? $"Se muestran los primeros {MaximoFilas} resultados; use los filtros para acotar la búsqueda"
                        : filas.Count == 0 ? "No hay calificaciones con los filtros seleccionados" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<ConsultaFilaDTO>>.Error("No fue posible consultar las calificaciones", 1409);
            }
        }

        public async Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalleAdministrativo(int idEstudiante, int idPeriodo)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetById(idEstudiante);

                if (estudiante == null)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("El estudiante no existe", 1431);

                return await ArmarDetalle(estudiante, idPeriodo, null, true);
            }
            catch (Exception)
            {
                return RespuestaDTO<DetalleEstudianteDTO>.Error("No fue posible consultar las calificaciones", 1409);
            }
        }

        // ---------- Docente (MCNF-01-15) ----------

        public async Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalleDocente(
            int idUsuario, int idEstudiante, int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            try
            {
                var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

                if (docente == null || !docente.Estado)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("Solo el personal docente activo puede consultar este detalle", 1401);

                if (!await _calificacionRepository.DocenteImparteClase(docente.IdDocente, idPeriodo, idCurso, idSeccion, idGrupo))
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("No tiene asignado este curso y grupo", 1403);

                // El estudiante debe pertenecer a esa clase: el docente solo ve su curso
                var matricula = await _calificacionRepository.GetMatriculaDeEstudiante(idEstudiante, idPeriodo, idCurso);

                if (matricula == null || matricula.IdSeccion != idSeccion || matricula.IdGrupo != idGrupo)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("El estudiante no pertenece a este curso y grupo", 1408);

                var estudiante = await _estudianteRepository.GetById(idEstudiante);

                if (estudiante == null)
                    return RespuestaDTO<DetalleEstudianteDTO>.Error("El estudiante no existe", 1431);

                return await ArmarDetalle(estudiante, idPeriodo, idCurso, true);
            }
            catch (Exception)
            {
                return RespuestaDTO<DetalleEstudianteDTO>.Error("No fue posible consultar el detalle del estudiante", 1409);
            }
        }

        // ---------- Auxiliares ----------

        private async Task<RespuestaDTO<DetalleEstudianteDTO>> ArmarDetalle(
            Ent.Estudiante estudiante, int idPeriodo, int? idCurso, bool incluirObservaciones)
        {
            var respuesta = await _resultados.GetDetalle(estudiante.IdEstudiante, idPeriodo, idCurso, incluirObservaciones);

            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return respuesta;

            respuesta.Dato.Carnet = estudiante.Carnet;
            respuesta.Dato.Nombre = estudiante.Usuario == null
                ? string.Empty
                : $"{estudiante.Usuario.Apellido} {estudiante.Usuario.Nombre}";
            respuesta.Dato.Grado = estudiante.Grado?.Nombre;

            return respuesta;
        }

        // Solo los estudiantes asociados a la cuenta del encargado
        private async Task<Ent.Estudiante?> ObtenerEstudianteDelEncargado(int idUsuario, int idEstudiante)
        {
            var vinculos = await _encargadoRepository.GetPorUsuario(idUsuario);

            return vinculos.FirstOrDefault(v => v.IdEstudiante == idEstudiante)?.Estudiante;
        }

        // Períodos en los que el estudiante tiene matrícula activa
        private async Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> PeriodosDeEstudiante(int idEstudiante)
        {
            var periodos = (await _periodoRepository.GetPeriodos()).ToList();
            var matriculas = await _calificacionRepository.GetMatriculasDeEstudiante(
                idEstudiante, periodos.Select(p => p.IdPeriodo).ToList());

            var conMatricula = matriculas.Select(m => m.IdPeriodo).ToHashSet();

            var lista = periodos
                .Where(p => conMatricula.Contains(p.IdPeriodo))
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new PeriodoCalificacionDTO { Id = p.IdPeriodo, Nombre = p.Nombre, Activo = p.Activo })
                .ToList();

            return RespuestaDTO<List<PeriodoCalificacionDTO>>.Exito(
                lista,
                lista.Count == 0 ? "Todavía no hay matrícula en ningún período lectivo" : null);
        }
    }
}
