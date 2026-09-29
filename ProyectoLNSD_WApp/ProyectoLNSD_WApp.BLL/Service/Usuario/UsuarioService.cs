using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Usuario
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHashService _passwordHashService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IMapper mapper,
            IPasswordHashService passwordHashService)
        {
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
            _passwordHashService = passwordHashService;
        }

        public async Task<RespuestaDTO<List<UsuarioDTO>>> GetUsuarios()
        {
            var usuarios = await _usuarioRepository.GetUsuarios();

            return RespuestaDTO<List<UsuarioDTO>>.Exito(_mapper.Map<List<UsuarioDTO>>(usuarios));
        }

        public async Task<RespuestaDTO<UsuarioDTO?>> GetUsuarioById(int id)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(id);

            if (usuario == null)
                return RespuestaDTO<UsuarioDTO?>.Error("Usuario no encontrado", 404);

            return RespuestaDTO<UsuarioDTO?>.Exito(_mapper.Map<UsuarioDTO>(usuario));
        }

        public async Task<RespuestaDTO<List<UsuarioDTO>>> BuscarUsuarios(
            string? nombre,
            string? correo,
            int? idRol,
            bool? estado)
        {
            var usuarios = await _usuarioRepository.BuscarUsuarios(nombre, correo, idRol, estado);

            return RespuestaDTO<List<UsuarioDTO>>.Exito(_mapper.Map<List<UsuarioDTO>>(usuarios));
        }

        public async Task<RespuestaDTO<UsuarioDTO>> CreateUsuario(UsuarioCrearDTO usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                return RespuestaDTO<UsuarioDTO>.Error("El nombre es requerido", 1001);

            if (string.IsNullOrWhiteSpace(usuario.Apellido))
                return RespuestaDTO<UsuarioDTO>.Error("El apellido es requerido", 1002);

            if (string.IsNullOrWhiteSpace(usuario.Correo))
                return RespuestaDTO<UsuarioDTO>.Error("El correo es requerido", 1003);

            if (string.IsNullOrWhiteSpace(usuario.Password) || usuario.Password.Length < 8)
                return RespuestaDTO<UsuarioDTO>.Error("La contraseña debe tener al menos 8 caracteres", 1009);

            if (await _usuarioRepository.GetUsuarioByCorreo(usuario.Correo.Trim()) != null)
                return RespuestaDTO<UsuarioDTO>.Error("El correo ya existe", 1004);

            usuario.Nombre = usuario.Nombre.Trim();
            usuario.Apellido = usuario.Apellido.Trim();
            usuario.Correo = usuario.Correo.Trim();

            var entidad = _mapper.Map<DAL.Entities.Usuario>(usuario);

            entidad.PasswordHash = _passwordHashService.HashPassword(usuario.Password);
            entidad.Estado = true;

            if (!await _usuarioRepository.CreateUsuario(entidad))
                return RespuestaDTO<UsuarioDTO>.Error("No se pudo crear el usuario", 1005);

            return RespuestaDTO<UsuarioDTO>.Exito();
        }

        public async Task<RespuestaDTO<UsuarioDTO>> UpdateUsuario(UsuarioActualizarDTO usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.Apellido) ||
                string.IsNullOrWhiteSpace(usuario.Correo))
            {
                return RespuestaDTO<UsuarioDTO>.Error("Nombre, apellido y correo son requeridos", 1021);
            }

            if (await _usuarioRepository.GetUsuarioById(usuario.IdUsuario) == null)
                return RespuestaDTO<UsuarioDTO>.Error("Usuario no encontrado", 404);

            var usuarioConMismoCorreo = await _usuarioRepository.GetUsuarioByCorreo(usuario.Correo.Trim());

            if (usuarioConMismoCorreo != null && usuarioConMismoCorreo.IdUsuario != usuario.IdUsuario)
                return RespuestaDTO<UsuarioDTO>.Error("El correo ya está registrado para otro usuario", 1004);

            usuario.Nombre = usuario.Nombre.Trim();
            usuario.Apellido = usuario.Apellido.Trim();
            usuario.Correo = usuario.Correo.Trim();

            var entidad = _mapper.Map<DAL.Entities.Usuario>(usuario);

            if (!await _usuarioRepository.UpdateUsuario(entidad))
                return RespuestaDTO<UsuarioDTO>.Error("No se pudo actualizar el usuario", 1006);

            return RespuestaDTO<UsuarioDTO>.Exito();
        }

        public async Task<RespuestaDTO<UsuarioDTO>> CambiarEstado(int id, bool estado)
        {
            if (!await _usuarioRepository.CambiarEstado(id, estado))
                return RespuestaDTO<UsuarioDTO>.Error("No se pudo cambiar el estado", 1008);

            return RespuestaDTO<UsuarioDTO>.Exito();
        }
    }
}