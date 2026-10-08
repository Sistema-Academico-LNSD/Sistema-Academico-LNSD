using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Curso;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.DocenteCurso;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public class DocenteCursoService : IDocenteCursoService
    {
        private const string MensajeSinPeriodoActivo =
            "No hay un período lectivo activo. Actívelo en Configuración antes de asignar cursos.";

        private readonly IDocenteCursoRepository _docenteCursoRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly ICursoRepository _cursoRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;

        public DocenteCursoService(
            IDocenteCursoRepository docenteCursoRepository,
            IDocenteRepository docenteRepository,
            ICursoRepository cursoRepository,
            IPeriodoLectivoRepository periodoRepository)
        {
            _docenteCursoRepository = docenteCursoRepository;
            _docenteRepository = docenteRepository;
            _cursoRepository = cursoRepository;
            _periodoRepository = periodoRepository;
        }

        public async Task<RespuestaDTO<CursosDocenteDTO>> GetCursosDocente(int idDocente)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<CursosDocenteDTO>.Error("Docente no encontrado", 404);

            var periodoActivo = await _periodoRepository.GetPeriodoActivo();
            var asignaciones = await _docenteCursoRepository.GetAsignaciones(idDocente);

            var dtos = asignaciones.Select(a => new AsignacionCursoDTO
            {
                IdDocenteCurso = a.IdDocenteCurso,
                IdCurso = a.IdCurso,
                CodigoCurso = a.Curso?.Codigo ?? string.Empty,
                NombreCurso = a.Curso?.Nombre ?? string.Empty,
                NombreArea = a.Curso?.Area?.Nombre,
                IdPeriodo = a.IdPeriodo,
                NombrePeriodo = a.Periodo?.Nombre ?? string.Empty,
                PeriodoActivo = a.Periodo?.Activo ?? false,
                FechaAsignacion = DateTime.SpecifyKind(a.FechaAsignacion, DateTimeKind.Utc)
            }).ToList();

            return RespuestaDTO<CursosDocenteDTO>.Exito(new CursosDocenteDTO
            {
                IdDocente = docente.IdDocente,
                NombreDocente = $"{docente.Nombre} {docente.Apellidos}",
                DocenteActivo = docente.Estado,
                NombrePeriodoActivo = periodoActivo?.Nombre,
                Actuales = dtos.Where(a => a.PeriodoActivo).ToList(),
                Historial = dtos.Where(a => !a.PeriodoActivo).ToList()
            });
        }

        public async Task<RespuestaDTO<List<CursoDisponibleDTO>>> GetCursosDisponibles(int idDocente)
        {
            var periodoActivo = await _periodoRepository.GetPeriodoActivo();

            if (periodoActivo == null)
                return RespuestaDTO<List<CursoDisponibleDTO>>.Exito(new List<CursoDisponibleDTO>());

            var cursos = await _docenteCursoRepository.GetCursosDisponibles(idDocente, periodoActivo.IdPeriodo);

            return RespuestaDTO<List<CursoDisponibleDTO>>.Exito(cursos.Select(c => new CursoDisponibleDTO
            {
                IdCurso = c.IdCurso,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                NombreArea = c.Area?.Nombre
            }).ToList());
        }

        public async Task<RespuestaDTO<bool>> Asignar(int idDocente, int idCurso)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<bool>.Error("Docente no encontrado", 404);

            if (!docente.Estado)
                return RespuestaDTO<bool>.Error("No se pueden asignar cursos a un docente inactivo", 4201);

            var curso = await _cursoRepository.GetCursoById(idCurso);

            if (curso == null)
                return RespuestaDTO<bool>.Error("Curso no encontrado", 4202);

            if (!curso.Estado)
                return RespuestaDTO<bool>.Error("No se puede asignar un curso inactivo", 4203);

            var periodoActivo = await _periodoRepository.GetPeriodoActivo();

            if (periodoActivo == null)
                return RespuestaDTO<bool>.Error(MensajeSinPeriodoActivo, 4204);

            if (await _docenteCursoRepository.Existe(idDocente, idCurso, periodoActivo.IdPeriodo))
                return RespuestaDTO<bool>.Error("El docente ya tiene asignado ese curso en el período actual", 4205);

            var asignacion = new DAL.Entities.DocenteCurso
            {
                IdDocente = idDocente,
                IdCurso = idCurso,
                IdPeriodo = periodoActivo.IdPeriodo
            };

            if (!await _docenteCursoRepository.Crear(asignacion))
                return RespuestaDTO<bool>.Error("No se pudo asignar el curso", 4206);

            return RespuestaDTO<bool>.Exito(true, $"Curso asignado para el período {periodoActivo.Nombre}");
        }

        public async Task<RespuestaDTO<bool>> Quitar(int idDocenteCurso)
        {
            var asignacion = await _docenteCursoRepository.GetById(idDocenteCurso);

            if (asignacion == null)
                return RespuestaDTO<bool>.Error("Asignación no encontrada", 404);

            if (asignacion.Periodo == null || !asignacion.Periodo.Activo)
                return RespuestaDTO<bool>.Error("Solo se pueden quitar asignaciones del período activo. Las anteriores forman parte del historial.", 4207);

            if (!await _docenteCursoRepository.Eliminar(idDocenteCurso))
                return RespuestaDTO<bool>.Error("No se pudo quitar la asignación", 4208);

            return RespuestaDTO<bool>.Exito(true, "Asignación quitada correctamente");
        }
    }
}