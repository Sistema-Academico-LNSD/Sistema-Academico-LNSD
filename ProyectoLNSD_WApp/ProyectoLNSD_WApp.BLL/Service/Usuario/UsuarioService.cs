using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Usuario
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IMapper mapper)
        {
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<UsuarioDTO>>> GetUsuarios()
        {
            var respuesta = new RespuestaDTO<List<UsuarioDTO>>();

            var usuarios = await _usuarioRepository.GetUsuarios();

            respuesta.Dato =
                _mapper.Map<List<UsuarioDTO>>(usuarios);

            return respuesta;
        }

        public async Task<RespuestaDTO<UsuarioDTO?>> GetUsuarioById(int id)
        {
            var respuesta = new RespuestaDTO<UsuarioDTO?>();

            var usuario =
                await _usuarioRepository.GetUsuarioById(id);

            if (usuario == null)
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "Usuario no encontrado";
                respuesta.codigo = 404;

                return respuesta;
            }

            respuesta.Dato =
                _mapper.Map<UsuarioDTO>(usuario);

            return respuesta;
        }

        public async Task<RespuestaDTO<UsuarioDTO?>> GetUsuarioByCorreo(string correo)
        {
            var respuesta = new RespuestaDTO<UsuarioDTO?>();

            var usuario =
                await _usuarioRepository.GetUsuarioByCorreo(correo);

            if (usuario == null)
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "Usuario no encontrado";
                respuesta.codigo = 404;

                return respuesta;
            }

            respuesta.Dato =
                _mapper.Map<UsuarioDTO>(usuario);

            return respuesta;
        }

        public async Task<RespuestaDTO<List<UsuarioDTO>>> BuscarUsuarios(
            string? nombre,
            string? correo,
            int? idRol,
            bool? estado)
        {
            var respuesta =
                new RespuestaDTO<List<UsuarioDTO>>();

            var usuarios =
                await _usuarioRepository.BuscarUsuarios(
                    nombre,
                    correo,
                    idRol,
                    estado);

            respuesta.Dato =
                _mapper.Map<List<UsuarioDTO>>(usuarios);

            return respuesta;
        }

        public async Task<RespuestaDTO<UsuarioDTO>> CreateUsuario(
            UsuarioCrearDTO usuario)
        {
            var respuesta =
                new RespuestaDTO<UsuarioDTO>();

            // Validaciones de negocio

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "El nombre es requerido";
                respuesta.codigo = 1001;

                return respuesta;
            }

            if (string.IsNullOrWhiteSpace(usuario.Apellido))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "El apellido es requerido";
                respuesta.codigo = 1002;

                return respuesta;
            }

            if (string.IsNullOrWhiteSpace(usuario.Correo))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "El correo es requerido";
                respuesta.codigo = 1003;

                return respuesta;
            }

            var existeUsuario =
                await _usuarioRepository
                .GetUsuarioByCorreo(usuario.Correo);

            if (existeUsuario != null)
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "El correo ya existe";
                respuesta.codigo = 1004;

                return respuesta;
            }

            var entidad =
                _mapper.Map<DAL.Entities.Usuario>(usuario);

            // Hash temporal
            entidad.PasswordHash =
                System.Text.Encoding.UTF8.GetBytes(
                    usuario.Password);

            entidad.Estado = true;

            if (!await _usuarioRepository.CreateUsuario(entidad))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "No se pudo crear el usuario";
                respuesta.codigo = 1005;

                return respuesta;
            }

            return respuesta;
        }

        public async Task<RespuestaDTO<UsuarioDTO>> UpdateUsuario(
            UsuarioActualizarDTO usuario)
        {
            var respuesta =
                new RespuestaDTO<UsuarioDTO>();

            var entidad =
                _mapper.Map<DAL.Entities.Usuario>(usuario);

            if (!await _usuarioRepository.UpdateUsuario(entidad))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "No se pudo actualizar el usuario";
                respuesta.codigo = 1006;

                return respuesta;
            }

            return respuesta;
        }

        public async Task<RespuestaDTO<UsuarioDTO>> DeleteUsuario(int id)
        {
            var respuesta =
                new RespuestaDTO<UsuarioDTO>();

            if (!await _usuarioRepository.DeleteUsuario(id))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "No se pudo eliminar el usuario";
                respuesta.codigo = 1007;
            }

            return respuesta;
        }

        public async Task<RespuestaDTO<UsuarioDTO>> CambiarEstado(
            int id,
            bool estado)
        {
            var respuesta =
                new RespuestaDTO<UsuarioDTO>();

            if (!await _usuarioRepository.CambiarEstado(id, estado))
            {
                respuesta.esCorrecto = false;
                respuesta.mensaje = "No se pudo cambiar el estado";
                respuesta.codigo = 1008;
            }

            return respuesta;
        }
    }
}