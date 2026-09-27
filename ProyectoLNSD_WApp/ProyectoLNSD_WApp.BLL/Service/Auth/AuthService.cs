using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Email;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;
using ProyectoLNSD_WApp.DAL.Repositories.UsuarioTokenReset;

namespace ProyectoLNSD_WApp.BLL.Service.Auth
{
    /// <summary>
    /// Autenticación y recuperación de acceso: login y restablecimiento de
    /// contraseña por correo. La gestión de usuarios (CRUD) vive en UsuarioService.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioTokenResetRepository _tokenResetRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IEmailService _emailService;

        public AuthService(
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

        public async Task<RespuestaDTO<ResultadoLoginDTO>> Login(LoginDTO login)
        {
            const string mensajeCredencialesInvalidas = "Correo o contraseña incorrectos";

            if (string.IsNullOrWhiteSpace(login.Correo) || string.IsNullOrWhiteSpace(login.Password))
                return RespuestaDTO<ResultadoLoginDTO>.Error(mensajeCredencialesInvalidas, 1010);

            var usuario = await _usuarioRepository.GetUsuarioByCorreo(login.Correo.Trim());

            if (usuario == null || !_passwordHashService.VerifyPassword(usuario.PasswordHash, login.Password))
                return RespuestaDTO<ResultadoLoginDTO>.Error(mensajeCredencialesInvalidas, 1011);

            if (!usuario.Estado)
                return RespuestaDTO<ResultadoLoginDTO>.Error("El usuario está inactivo. Contacte al administrador.", 1012);

            return RespuestaDTO<ResultadoLoginDTO>.Exito(new ResultadoLoginDTO
            {
                Autenticado = true,
                Mensaje = "Inicio de sesión exitoso",
                Usuario = _mapper.Map<UsuarioDTO>(usuario)
            });
        }

        public async Task<RespuestaDTO<bool>> SolicitarRecuperacionPassword(
            SolicitarRecuperacionDTO dto,
            string urlBaseRestablecer)
        {

            var respuestaGenerica = RespuestaDTO<bool>.Exito(
                true,
                "Si el correo se encuentra registrado, se envió un enlace de recuperación.");

            if (string.IsNullOrWhiteSpace(dto.Correo))
                return respuestaGenerica;

            var usuario = await _usuarioRepository.GetUsuarioByCorreo(dto.Correo.Trim());

            if (usuario == null || !usuario.Estado)
                return respuestaGenerica;

            byte[] tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);

            string tokenRaw = Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            await _tokenResetRepository.InvalidarTokensActivos(usuario.IdUsuario);

            await _tokenResetRepository.CrearToken(new DAL.Entities.UsuarioTokenReset
            {
                IdUsuario = usuario.IdUsuario,
                TokenHash = HashToken(tokenRaw),
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(30),
                Usado = false
            });

            await _emailService.EnviarCorreoRecuperacion(
                usuario.Correo,
                $"{usuario.Nombre} {usuario.Apellido}",
                $"{urlBaseRestablecer}?token={tokenRaw}");

            return respuestaGenerica;
        }

        public async Task<RespuestaDTO<bool>> RestablecerPassword(RestablecerPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return RespuestaDTO<bool>.Error("El enlace de recuperación no es válido.", 1013);

            if (dto.NuevaPassword != dto.ConfirmarPassword)
                return RespuestaDTO<bool>.Error("Las contraseñas no coinciden.", 1014);

            if (dto.NuevaPassword.Length < 8)
                return RespuestaDTO<bool>.Error("La contraseña debe tener al menos 8 caracteres.", 1015);

            var tokenEntity = await _tokenResetRepository.ObtenerTokenValidoPorHash(HashToken(dto.Token));

            if (tokenEntity == null)
                return RespuestaDTO<bool>.Error("El enlace de recuperación es inválido o ya expiró.", 1016);

            byte[] nuevoHash = _passwordHashService.HashPassword(dto.NuevaPassword);

            if (!await _usuarioRepository.ActualizarPassword(tokenEntity.IdUsuario, nuevoHash))
                return RespuestaDTO<bool>.Error("No se pudo actualizar la contraseña.", 1017);

            await _tokenResetRepository.MarcarComoUsado(tokenEntity.IdToken);

            return RespuestaDTO<bool>.Exito(true, "Contraseña actualizada correctamente.");
        }
        private static byte[] HashToken(string tokenRaw) =>
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(tokenRaw));
    }
}