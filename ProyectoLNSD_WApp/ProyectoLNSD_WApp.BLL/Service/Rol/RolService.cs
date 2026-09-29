using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;

namespace ProyectoLNSD_WApp.BLL.Service.Rol
{
    public class RolService : IRolService
    {
        private const string RolProtegido = "Administrador";

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
            var roles = await _rolRepository.GetRoles();

            return RespuestaDTO<List<RolDTO>>.Exito(_mapper.Map<List<RolDTO>>(roles));
        }

        public async Task<RespuestaDTO<RolDTO?>> GetRolById(int id)
        {
            var rol = await _rolRepository.GetRolById(id);

            if (rol == null)
                return RespuestaDTO<RolDTO?>.Error("Rol no encontrado", 404);

            return RespuestaDTO<RolDTO?>.Exito(_mapper.Map<RolDTO>(rol));
        }
        public async Task<RespuestaDTO<RolDTO>> CreateRol(RolDTO rol)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre))
                return RespuestaDTO<RolDTO>.Error("El nombre del rol es requerido", 1001);

            if (await _rolRepository.ExisteNombre(rol.Nombre))
                return RespuestaDTO<RolDTO>.Error("Ya existe un rol con ese nombre", 1006);

            rol.Nombre = rol.Nombre.Trim();

            if (!await _rolRepository.CreateRol(_mapper.Map<DAL.Entities.Rol>(rol)))
                return RespuestaDTO<RolDTO>.Error("No se pudo crear el rol", 1002);

            return RespuestaDTO<RolDTO>.Exito(rol);
        }
        public async Task<RespuestaDTO<RolDTO>> UpdateRol(RolDTO rol)
        {
            if (rol.IdRol <= 0)
                return RespuestaDTO<RolDTO>.Error("Rol inválido", 1003);

            var existente = await _rolRepository.GetRolById(rol.IdRol);

            if (existente == null)
                return RespuestaDTO<RolDTO>.Error("Rol no encontrado", 404);

            if (EsRolProtegido(existente.Nombre) && !EsRolProtegido(rol.Nombre.Trim()))
                return RespuestaDTO<RolDTO>.Error("El rol Administrador no puede renombrarse", 1007);

            if (await _rolRepository.ExisteNombre(rol.Nombre, rol.IdRol))
                return RespuestaDTO<RolDTO>.Error("Ya existe un rol con ese nombre", 1006);

            rol.Nombre = rol.Nombre.Trim();

            if (!await _rolRepository.UpdateRol(_mapper.Map<DAL.Entities.Rol>(rol)))
                return RespuestaDTO<RolDTO>.Error("No se pudo actualizar el rol", 1004);

            return RespuestaDTO<RolDTO>.Exito(rol);
        }

        public async Task<RespuestaDTO<RolDTO>> DeleteRol(int id)
        {
            var rol = await _rolRepository.GetRolById(id);

            if (rol == null)
                return RespuestaDTO<RolDTO>.Error("Rol no encontrado", 404);

            if (EsRolProtegido(rol.Nombre))
                return RespuestaDTO<RolDTO>.Error("El rol Administrador no puede eliminarse", 1008);

            if (await _rolRepository.TieneUsuariosAsignados(id))
                return RespuestaDTO<RolDTO>.Error(
                    "No se puede eliminar el rol porque tiene usuarios asignados. Reasigne esos usuarios a otro rol primero.",
                    1009);

            if (!await _rolRepository.DeleteRol(id))
                return RespuestaDTO<RolDTO>.Error("No se pudo eliminar el rol", 1005);

            return RespuestaDTO<RolDTO>.Exito();
        }

        private static bool EsRolProtegido(string nombre) =>
            string.Equals(nombre, RolProtegido, StringComparison.OrdinalIgnoreCase);
    }
}