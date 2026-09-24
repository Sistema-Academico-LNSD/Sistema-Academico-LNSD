using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;

namespace ProyectoLNSD_WApp.BLL.Service.Rol
{
    public class RolService : IRolService
    {
        private readonly IRolRepository _rolRepository;
        private readonly IMapper _mapper;

        public RolService(
            IRolRepository rolRepository,
            IMapper mapper)
        {
            _rolRepository = rolRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<RolDTO>>> GetRoles()
        {
            var respuesta = new RespuestaDTO<List<RolDTO>>();

            var roles = await _rolRepository.GetRoles();

            respuesta.Dato =
                _mapper.Map<List<RolDTO>>(roles);

            return respuesta;
        }

        public async Task<RespuestaDTO<RolDTO?>> GetRolById(int id)
        {
            var respuesta = new RespuestaDTO<RolDTO?>();

            var rol = await _rolRepository.GetRolById(id);

            if (rol == null)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Rol no encontrado";
                respuesta.Codigo = 404;

                return respuesta;
            }

            respuesta.Dato =
                _mapper.Map<RolDTO>(rol);

            return respuesta;
        }

        public async Task<RespuestaDTO<RolDTO>> CreateRol(RolDTO rol)
        {
            var respuesta = new RespuestaDTO<RolDTO>();

            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El nombre del rol es requerido";
                respuesta.Codigo = 1001;

                return respuesta;
            }

            var entity =
                _mapper.Map<DAL.Entities.Rol>(rol);

            if (!await _rolRepository.CreateRol(entity))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo crear el rol";
                respuesta.Codigo = 1002;

                return respuesta;
            }

            respuesta.Dato = rol;

            return respuesta;
        }

        public async Task<RespuestaDTO<RolDTO>> UpdateRol(RolDTO rol)
        {
            var respuesta = new RespuestaDTO<RolDTO>();

            if (rol.IdRol <= 0)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Rol inválido";
                respuesta.Codigo = 1003;

                return respuesta;
            }

            var entity =
                _mapper.Map<DAL.Entities.Rol>(rol);

            if (!await _rolRepository.UpdateRol(entity))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo actualizar el rol";
                respuesta.Codigo = 1004;

                return respuesta;
            }

            respuesta.Dato = rol;

            return respuesta;
        }

        public async Task<RespuestaDTO<RolDTO>> DeleteRol(int id)
        {
            var respuesta = new RespuestaDTO<RolDTO>();

            if (!await _rolRepository.DeleteRol(id))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo eliminar el rol";
                respuesta.Codigo = 1005;
            }

            return respuesta;
        }
    }
}