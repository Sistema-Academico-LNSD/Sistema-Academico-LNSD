using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Email;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;
using ProyectoLNSD_WApp.DAL.Repositories.UsuarioTokenReset;

namespace ProyectoLNSD_WApp.BLL.Service.Usuario
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioTokenResetRepository _tokenResetRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IEmailService _emailService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IUsuarioTokenResetRepository tokenResetRepository,
            IMapper mapper,
            IPasswordHashService passwordHashService,
            IEmailService emailService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenResetRepository = tokenResetRepository;
            _mapper = mapper;
            _passwordHashService = passwordHashService;
            _emailService = emailService;
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
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Usuario no encontrado";
                respuesta.Codigo = 404;

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
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Usuario no encontrado";
                respuesta.Codigo = 404;

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
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El nombre es requerido";
                respuesta.Codigo = 1001;

                return respuesta;
            }

            if (string.IsNullOrWhiteSpace(usuario.Apellido))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El apellido es requerido";
                respuesta.Codigo = 1002;

                return respuesta;
            }

            if (string.IsNullOrWhiteSpace(usuario.Correo))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El correo es requerido";
                respuesta.Codigo = 1003;

                return respuesta;
            }

            if (string.IsNullOrWhiteSpace(usuario.Password) || usuario.Password.Length < 8)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "La contraseña debe tener al menos 8 caracteres";
                respuesta.Codigo = 1009;

                return respuesta;
            }

            var existeUsuario =
                await _usuarioRepository
                .GetUsuarioByCorreo(usuario.Correo);

            if (existeUsuario != null)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El correo ya existe";
                respuesta.Codigo = 1004;

                return respuesta;
            }

            var entidad =
                _mapper.Map<DAL.Entities.Usuario>(usuario);

            entidad.PasswordHash =
                _passwordHashService.HashPassword(usuario.Password);

            entidad.Estado = true;

            if (!await _usuarioRepository.CreateUsuario(entidad))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo crear el usuario";
                respuesta.Codigo = 1005;

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
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo actualizar el usuario";
                respuesta.Codigo = 1006;

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
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo eliminar el usuario";
                respuesta.Codigo = 1007;
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
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo cambiar el estado";
                respuesta.Codigo = 1008;
            }

            return respuesta;
        }

        public async Task<RespuestaDTO<ResultadoLoginDTO>> Login(LoginDTO login)
        {
            var respuesta = new RespuestaDTO<ResultadoLoginDTO>();

            // Mensaje genérico para no revelar si el correo existe o no (buena práctica de seguridad)
            const string mensajeCredencialesInvalidas = "Correo o contraseña incorrectos";

            if (string.IsNullOrWhiteSpace(login.Correo) || string.IsNullOrWhiteSpace(login.Password))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = mensajeCredencialesInvalidas;
                respuesta.Codigo = 1010;

                respuesta.Dato = new ResultadoLoginDTO
                {
                    Autenticado = false,
                    Mensaje = mensajeCredencialesInvalidas
                };

                return respuesta;
            }

            var usuario = await _usuarioRepository.GetUsuarioByCorreo(login.Correo);

            if (usuario == null || !_passwordHashService.VerifyPassword(usuario.PasswordHash, login.Password))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = mensajeCredencialesInvalidas;
                respuesta.Codigo = 1011;

                respuesta.Dato = new ResultadoLoginDTO
                {
                    Autenticado = false,
                    Mensaje = mensajeCredencialesInvalidas
                };

                return respuesta;
            }

            if (!usuario.Estado)
            {
                const string mensajeInactivo = "El usuario está inactivo. Contacte al administrador.";

                respuesta.EsCorrecto = false;
                respuesta.Mensaje = mensajeInactivo;
                respuesta.Codigo = 1012;

                respuesta.Dato = new ResultadoLoginDTO
                {
                    Autenticado = false,
                    Mensaje = mensajeInactivo
                };

                return respuesta;
            }

            respuesta.Dato = new ResultadoLoginDTO
            {
                Autenticado = true,
                Mensaje = "Inicio de sesión exitoso",
                Usuario = _mapper.Map<UsuarioDTO>(usuario)
            };

            return respuesta;
        }

        public async Task<RespuestaDTO<bool>> SolicitarRecuperacionPassword(
            SolicitarRecuperacionDTO dto,
            string urlBaseRestablecer)
        {
            // Mensaje genérico siempre, exista o no el correo, para no revelar
            // qué correos están registrados en el sistema.
            const string mensajeGenerico =
                "Si el correo se encuentra registrado, se envió un enlace de recuperación.";

            var respuesta = new RespuestaDTO<bool>
            {
                Dato = true,
                Mensaje = mensajeGenerico
            };

            if (string.IsNullOrWhiteSpace(dto.Correo))
            {
                return respuesta;
            }

            var usuario = await _usuarioRepository.GetUsuarioByCorreo(dto.Correo);

            if (usuario == null || !usuario.Estado)
            {
                // No se revela si el correo existe o si el usuario está inactivo.
                return respuesta;
            }

            // Token aleatorio criptográficamente seguro, codificado en Base64 URL-safe.
            byte[] tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);

            string tokenRaw = Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            byte[] tokenHash = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(tokenRaw));

            // Invalida cualquier token anterior aún activo para este usuario.
            await _tokenResetRepository.InvalidarTokensActivos(usuario.IdUsuario);

            var tokenEntity = new DAL.Entities.UsuarioTokenReset
            {
                IdUsuario = usuario.IdUsuario,
                TokenHash = tokenHash,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(30),
                Usado = false
            };

            await _tokenResetRepository.CrearToken(tokenEntity);

            string enlace = $"{urlBaseRestablecer}?token={tokenRaw}";

            await _emailService.EnviarCorreoRecuperacion(
                usuario.Correo,
                $"{usuario.Nombre} {usuario.Apellido}",
                enlace);

            return respuesta;
        }

        public async Task<RespuestaDTO<bool>> RestablecerPassword(RestablecerPasswordDTO dto)
        {
            var respuesta = new RespuestaDTO<bool>();

            if (string.IsNullOrWhiteSpace(dto.Token))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El enlace de recuperación no es válido.";
                respuesta.Codigo = 1013;

                return respuesta;
            }

            if (dto.NuevaPassword != dto.ConfirmarPassword)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Las contraseñas no coinciden.";
                respuesta.Codigo = 1014;

                return respuesta;
            }

            if (dto.NuevaPassword.Length < 8)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "La contraseña debe tener al menos 8 caracteres.";
                respuesta.Codigo = 1015;

                return respuesta;
            }

            byte[] tokenHash = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(dto.Token));

            var tokenEntity = await _tokenResetRepository.ObtenerTokenValidoPorHash(tokenHash);

            if (tokenEntity == null)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "El enlace de recuperación es inválido o ya expiró.";
                respuesta.Codigo = 1016;

                return respuesta;
            }

            byte[] nuevoHash = _passwordHashService.HashPassword(dto.NuevaPassword);

            if (!await _usuarioRepository.ActualizarPassword(tokenEntity.IdUsuario, nuevoHash))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudo actualizar la contraseña.";
                respuesta.Codigo = 1017;

                return respuesta;
            }

            await _tokenResetRepository.MarcarComoUsado(tokenEntity.IdToken);

            respuesta.Dato = true;
            respuesta.Mensaje = "Contraseña actualizada correctamente.";

            return respuesta;
        }
    }
}