using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public class ExpedienteDocenteService : IExpedienteDocenteService
    {
        private readonly IDocenteRepository _docenteRepository;
        private readonly IDocenteCursoService _docenteCursoService;
        private readonly IMapper _mapper;

        public ExpedienteDocenteService(
            IDocenteRepository docenteRepository,
            IDocenteCursoService docenteCursoService,
            IMapper mapper)
        {
            _docenteRepository = docenteRepository;
            _docenteCursoService = docenteCursoService;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<ExpedienteDocenteDTO>> GetPorDocente(int idDocente)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<ExpedienteDocenteDTO>.Error("Docente no encontrado", 404);

            return await ArmarExpediente(docente);
        }

        public async Task<RespuestaDTO<ExpedienteDocenteDTO>> GetPorUsuario(int idUsuario)
        {
            var docente = await _docenteRepository.GetByIdUsuario(idUsuario);

            if (docente == null)
                return RespuestaDTO<ExpedienteDocenteDTO>.Error(
                    "Tu cuenta no está vinculada a un expediente docente. Comunicate con la administración.", 4301);

            return await ArmarExpediente(docente);
        }

        private async Task<RespuestaDTO<ExpedienteDocenteDTO>> ArmarExpediente(DAL.Entities.Docente docente)
        {
            var datos = _mapper.Map<DocenteDTO>(docente);

            datos.FechaRegistro = DateTime.SpecifyKind(datos.FechaRegistro, DateTimeKind.Utc);

            var cursos = await _docenteCursoService.GetCursosDocente(docente.IdDocente);

            if (!cursos.EsCorrecto || cursos.Dato == null)
                return RespuestaDTO<ExpedienteDocenteDTO>.Error(cursos.Mensaje, cursos.Codigo);

            return RespuestaDTO<ExpedienteDocenteDTO>.Exito(new ExpedienteDocenteDTO
            {
                Docente = datos,
                CorreoCuenta = docente.Usuario?.Correo,
                Cursos = cursos.Dato
            });
        }
    }
}