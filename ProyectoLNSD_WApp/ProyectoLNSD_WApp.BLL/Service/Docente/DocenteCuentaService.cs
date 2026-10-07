using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public class DocenteCuentaService : IDocenteCuentaService
    {
        private const string RolDocente = "Docente";
        private const int LargoMaximoApellidoCuenta = 100;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IMapper _mapper;

        public DocenteCuentaService(
            IDocenteRepository docenteRepository,
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            IPasswordHashService passwordHashService,
            IMapper mapper)
        {
            _docenteRepository = docenteRepository;
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
            _passwordHashService = passwordHashService;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<CuentaDocenteDTO>> GetCuenta(int idDocente)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<CuentaDocenteDTO>.Error("Docente no encontrado", 404);

            return RespuestaDTO<CuentaDocenteDTO>.Exito(new CuentaDocenteDTO
            {
                IdDocente = docente.IdDocente,
                NombreDocente = $"{docente.Nombre} {docente.Apellidos}",
                CorreoDocente = docente.Correo,
                TieneCuenta = docente.Usuario != null,
                IdUsuario = docente.Usuario?.IdUsuario,
                CorreoCuenta = docente.Usuario?.Correo,
                CuentaActiva = docente.Usuario?.Estado ?? false
            });
        }

        public async Task<RespuestaDTO<List<UsuarioDTO>>> GetUsuariosDisponibles()
        {
            var usuarios = await _docenteRepository.GetUsuariosDisponibles(RolDocente);

            return RespuestaDTO<List<UsuarioDTO>>.Exito(_mapper.Map<List<UsuarioDTO>>(usuarios));
        }

        public async Task<RespuestaDTO<bool>> VincularCuentaExistente(int idDocente, int idUsuario)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<bool>.Error("Docente no encontrado", 404);

            if (docente.IdUsuario.HasValue)
                return RespuestaDTO<bool>.Error("El docente ya tiene una cuenta vinculada. Desvincúlela primero.", 4101);

            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);

            if (usuario == null)
                return RespuestaDTO<bool>.Error("La cuenta seleccionada no existe", 4102);

            if (!string.Equals(usuario.Rol?.Nombre, RolDocente, StringComparison.OrdinalIgnoreCase))
                return RespuestaDTO<bool>.Error("Solo se pueden vincular cuentas con el rol Docente", 4103);

            if (!usuario.Estado)
                return RespuestaDTO<bool>.Error("La cuenta seleccionada está inactiva", 4104);

            if (await _docenteRepository.UsuarioEstaAsociado(idUsuario))
                return RespuestaDTO<bool>.Error("Esa cuenta ya está vinculada a otro docente", 4105);

            if (!await _docenteRepository.AsignarUsuario(idDocente, idUsuario))
                return RespuestaDTO<bool>.Error("No se pudo vincular la cuenta", 4106);

            return RespuestaDTO<bool>.Exito(true, "Cuenta vinculada correctamente");
        }

        public async Task<RespuestaDTO<bool>> CrearCuenta(CrearCuentaDocenteDTO dto)
        {
            var docente = await _docenteRepository.GetById(dto.IdDocente);

            if (docente == null)
                return RespuestaDTO<bool>.Error("Docente no encontrado", 404);

            if (docente.IdUsuario.HasValue)
                return RespuestaDTO<bool>.Error("El docente ya tiene una cuenta vinculada. Desvincúlela primero.", 4101);

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                return RespuestaDTO<bool>.Error("La contraseña debe tener al menos 8 caracteres", 4107);

            if (dto.Password != dto.ConfirmarPassword)
                return RespuestaDTO<bool>.Error("Las contraseñas no coinciden", 4108);

            string correo = dto.Correo.Trim();

            if (await _usuarioRepository.GetUsuarioByCorreo(correo) != null)
                return RespuestaDTO<bool>.Error(
                    "Ya existe una cuenta con ese correo. Use la opción \"Vincular cuenta existente\".", 4109);

            if (docente.Apellidos.Length > LargoMaximoApellidoCuenta)
                return RespuestaDTO<bool>.Error(
                    $"Los apellidos del docente superan los {LargoMaximoApellidoCuenta} caracteres que admite una cuenta de usuario", 4110);

            var rol = await _rolRepository.GetRolByNombre(RolDocente);

            if (rol == null)
                return RespuestaDTO<bool>.Error("No existe el rol Docente en el sistema", 4111);

            var usuario = new DAL.Entities.Usuario
            {
                IdRol = rol.IdRol,
                Nombre = docente.Nombre,
                Apellido = docente.Apellidos,
                Correo = correo,
                PasswordHash = _passwordHashService.HashPassword(dto.Password),
                Estado = true
            };

            if (!await _docenteRepository.CrearCuentaYAsociar(dto.IdDocente, usuario))
                return RespuestaDTO<bool>.Error("No se pudo crear la cuenta", 4112);

            return RespuestaDTO<bool>.Exito(true, "Cuenta creada y vinculada correctamente");
        }

        public async Task<RespuestaDTO<bool>> Desvincular(int idDocente)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<bool>.Error("Docente no encontrado", 404);

            if (!docente.IdUsuario.HasValue)
                return RespuestaDTO<bool>.Error("El docente no tiene una cuenta vinculada", 4113);

            if (!await _docenteRepository.AsignarUsuario(idDocente, null))
                return RespuestaDTO<bool>.Error("No se pudo desvincular la cuenta", 4106);

            return RespuestaDTO<bool>.Exito(true, "Cuenta desvinculada. La cuenta sigue existiendo en Usuarios.");
        }
    }
}