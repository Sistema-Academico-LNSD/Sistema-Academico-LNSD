using System.Security.Cryptography;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.DAL.Repositories.Encargado;
using ProyectoLNSD_WApp.DAL.Repositories.Estudiante;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Encargado
{
    public class EncargadoService : IEncargadoService
    {
        private const string RolEncargado = "Encargado";

        public static readonly string[] Parentescos =
        {
            "Padre", "Madre", "Abuelo", "Abuela", "Tío", "Tía",
            "Hermano", "Hermana", "Tutor legal", "Otro"
        };

        private readonly IEncargadoRepository _encargadoRepository;
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IPasswordHashService _passwordHashService;

        public EncargadoService(
            IEncargadoRepository encargadoRepository,
            IEstudianteRepository estudianteRepository,
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            IPasswordHashService passwordHashService)
        {
            _encargadoRepository = encargadoRepository;
            _estudianteRepository = estudianteRepository;
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
            _passwordHashService = passwordHashService;
        }

        public async Task<RespuestaDTO<List<EncargadoVinculoDTO>>> GetPorEstudiante(int idEstudiante)
        {
            if (await _estudianteRepository.GetById(idEstudiante) == null)
                return RespuestaDTO<List<EncargadoVinculoDTO>>.Error("Estudiante no encontrado", 404);

            var vinculos = await _encargadoRepository.GetPorEstudiante(idEstudiante);

            var lista = vinculos.Select(v => new EncargadoVinculoDTO
            {
                IdEncargado = v.IdEncargado,
                IdEstudiante = v.IdEstudiante,
                Nombre = v.Encargado?.Usuario?.Nombre ?? string.Empty,
                Apellidos = v.Encargado?.Usuario?.Apellido ?? string.Empty,
                Correo = v.Encargado?.Usuario?.Correo ?? string.Empty,
                Telefono = v.Encargado?.Telefono,
                Parentesco = v.Parentesco,
                EsPrincipal = v.EsPrincipal
            }).ToList();

            return RespuestaDTO<List<EncargadoVinculoDTO>>.Exito(lista);
        }

        public async Task<RespuestaDTO<List<EstudianteEncargadoDTO>>> GetMisEstudiantes(int idUsuario)
        {
            var vinculos = await _encargadoRepository.GetPorUsuario(idUsuario);

            // Solo información básica: sin datos médicos ni de atención.
            var lista = vinculos
                .Where(v => v.Estudiante != null)
                .Select(v => new EstudianteEncargadoDTO
                {
                    IdEstudiante = v.IdEstudiante,
                    Nombre = v.Estudiante!.Usuario?.Nombre ?? string.Empty,
                    Apellidos = v.Estudiante.Usuario?.Apellido ?? string.Empty,
                    Carnet = v.Estudiante.Carnet,
                    Identificacion = v.Estudiante.Identificacion,
                    FechaNacimiento = v.Estudiante.FechaNacimiento,
                    FechaIngreso = v.Estudiante.FechaIngreso,
                    NombreGrado = v.Estudiante.Grado?.Nombre,
                    EstadoAcademico = v.Estudiante.EstadoAcademico,
                    Estado = v.Estudiante.Estado,
                    Parentesco = v.Parentesco,
                    EsPrincipal = v.EsPrincipal
                }).ToList();

            return RespuestaDTO<List<EstudianteEncargadoDTO>>.Exito(lista);
        }

        public async Task<RespuestaDTO<List<EncargadoBusquedaDTO>>> Buscar(string? texto, int idEstudiante)
        {
            var encargados = await _encargadoRepository.Buscar(texto, idEstudiante);

            var lista = encargados.Select(e => new EncargadoBusquedaDTO
            {
                IdEncargado = e.IdEncargado,
                NombreCompleto = $"{e.Usuario?.Nombre} {e.Usuario?.Apellido}".Trim(),
                Correo = e.Usuario?.Correo ?? string.Empty,
                Telefono = e.Telefono
            }).ToList();

            return RespuestaDTO<List<EncargadoBusquedaDTO>>.Exito(lista);
        }

        public async Task<RespuestaDTO<bool>> Vincular(VincularEncargadoDTO dto)
        {
            if (!Parentescos.Contains(dto.Parentesco))
                return RespuestaDTO<bool>.Error("El parentesco no es válido", 4301);

            var estudiante = await _estudianteRepository.GetById(dto.IdEstudiante);

            if (estudiante == null)
                return RespuestaDTO<bool>.Error("Estudiante no encontrado", 404);

            var encargado = await _encargadoRepository.GetById(dto.IdEncargado);

            if (encargado == null)
                return RespuestaDTO<bool>.Error("El encargado seleccionado no existe", 4302);

            if (encargado.Usuario == null || !encargado.Usuario.Estado)
                return RespuestaDTO<bool>.Error("La cuenta del encargado está inactiva", 4303);

            if (await _encargadoRepository.GetVinculo(dto.IdEncargado, dto.IdEstudiante) != null)
                return RespuestaDTO<bool>.Error("Ese encargado ya está asociado al estudiante", 4304);

            if (!await _encargadoRepository.Vincular(dto.IdEncargado, dto.IdEstudiante, dto.Parentesco))
                return RespuestaDTO<bool>.Error("No se pudo asociar el encargado", 4305);

            return RespuestaDTO<bool>.Exito(true, "Encargado asociado correctamente");
        }

        public async Task<RespuestaDTO<bool>> CrearYVincular(CrearEncargadoDTO dto)
        {
            dto.Nombre = dto.Nombre?.Trim() ?? string.Empty;
            dto.Apellidos = dto.Apellidos?.Trim() ?? string.Empty;
            dto.Correo = dto.Correo?.Trim() ?? string.Empty;
            dto.Telefono = dto.Telefono?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Apellidos) ||
                string.IsNullOrWhiteSpace(dto.Correo) ||
                string.IsNullOrWhiteSpace(dto.Telefono))
            {
                return RespuestaDTO<bool>.Error("Nombre, apellidos, correo y teléfono son requeridos", 4306);
            }

            if (!Parentescos.Contains(dto.Parentesco))
                return RespuestaDTO<bool>.Error("El parentesco no es válido", 4301);

            if (await _estudianteRepository.GetById(dto.IdEstudiante) == null)
                return RespuestaDTO<bool>.Error("Estudiante no encontrado", 404);

            if (await _usuarioRepository.GetUsuarioByCorreo(dto.Correo) != null)
                return RespuestaDTO<bool>.Error(
                    "Ya existe una cuenta con ese correo. Si es un encargado, use \"Asociar existente\".", 4307);

            var rol = await _rolRepository.GetRolByNombre(RolEncargado);

            if (rol == null)
                return RespuestaDTO<bool>.Error("No existe el rol Encargado en el sistema", 4308);

            // Sin contraseña inicial, se genera una aleatoria que nadie conoce: el encargado
            // define la suya con "Olvidé mi contraseña".
            string password = string.IsNullOrWhiteSpace(dto.Password)
                ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))
                : dto.Password;

            var usuario = new DAL.Entities.Usuario
            {
                IdRol = rol.IdRol,
                Nombre = dto.Nombre,
                Apellido = dto.Apellidos,
                Correo = dto.Correo,
                PasswordHash = _passwordHashService.HashPassword(password),
                Estado = true
            };

            if (!await _encargadoRepository.CrearYVincular(usuario, dto.Telefono, dto.Parentesco, dto.IdEstudiante))
                return RespuestaDTO<bool>.Error("No se pudo registrar el encargado", 4309);

            return RespuestaDTO<bool>.Exito(true, "Encargado registrado y asociado correctamente");
        }

        public async Task<RespuestaDTO<bool>> DefinirPrincipal(int idEstudiante, int idEncargado)
        {
            var vinculo = await _encargadoRepository.GetVinculo(idEncargado, idEstudiante);

            if (vinculo == null)
                return RespuestaDTO<bool>.Error("El encargado no está asociado a este estudiante", 4310);

            if (vinculo.EsPrincipal)
                return RespuestaDTO<bool>.Error("Ese encargado ya es el principal", 4311);

            if (!await _encargadoRepository.DefinirPrincipal(idEstudiante, idEncargado))
                return RespuestaDTO<bool>.Error("No se pudo definir el encargado principal", 4312);

            return RespuestaDTO<bool>.Exito(true, "Encargado principal actualizado correctamente");
        }

        public async Task<RespuestaDTO<bool>> Desvincular(int idEstudiante, int idEncargado)
        {
            var vinculo = await _encargadoRepository.GetVinculo(idEncargado, idEstudiante);

            if (vinculo == null)
                return RespuestaDTO<bool>.Error("El encargado no está asociado a este estudiante", 4310);

            if (vinculo.EsPrincipal && await _encargadoRepository.ContarVinculos(idEstudiante) > 1)
                return RespuestaDTO<bool>.Error(
                    "No se puede quitar al encargado principal. Defina otro principal primero.", 4313);

            if (!await _encargadoRepository.Desvincular(idEncargado, idEstudiante))
                return RespuestaDTO<bool>.Error("No se pudo quitar al encargado", 4314);

            return RespuestaDTO<bool>.Exito(
                true, "Encargado desasociado. Su cuenta sigue existiendo en Usuarios.");
        }
    }
}
