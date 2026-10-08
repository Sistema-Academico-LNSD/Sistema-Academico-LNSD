using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.DAL.Repositories.Estudiante;
using ProyectoLNSD_WApp.DAL.Repositories.Grado;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Estudiante
{
    public class EstudianteService : IEstudianteService
    {
        private const string RolEstudiante = "Estudiante";

        public static readonly string[] EstadosAcademicos = { "Regular", "Repitente", "Retirado", "Egresado" };

        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IGradoRepository _gradoRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IMapper _mapper;

        public EstudianteService(
            IEstudianteRepository estudianteRepository,
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            IGradoRepository gradoRepository,
            IPasswordHashService passwordHashService,
            IMapper mapper)
        {
            _estudianteRepository = estudianteRepository;
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
            _gradoRepository = gradoRepository;
            _passwordHashService = passwordHashService;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<EstudianteDTO>>> Buscar(string? texto, int? idGrado, bool? estado)
        {
            var estudiantes = await _estudianteRepository.Buscar(texto, idGrado, estado);

            return RespuestaDTO<List<EstudianteDTO>>.Exito(_mapper.Map<List<EstudianteDTO>>(estudiantes));
        }

        public async Task<RespuestaDTO<EstudianteDTO?>> GetById(int idEstudiante)
        {
            var estudiante = await _estudianteRepository.GetById(idEstudiante);

            if (estudiante == null)
                return RespuestaDTO<EstudianteDTO?>.Error("Estudiante no encontrado", 404);

            return RespuestaDTO<EstudianteDTO?>.Exito(_mapper.Map<EstudianteDTO>(estudiante));
        }

        public async Task<RespuestaDTO<EstudianteDTO?>> GetMiExpediente(int idUsuario)
        {
            var estudiante = await _estudianteRepository.GetByUsuario(idUsuario);

            if (estudiante == null)
                return RespuestaDTO<EstudianteDTO?>.Error("No hay un expediente de estudiante asociado a su cuenta", 404);

            var dto = _mapper.Map<EstudianteDTO>(estudiante);

            // MESF-01-11: el estudiante consulta información personal y académica;
            // los datos médicos y de atención no se exponen en esta vista.
            dto.Alergias = null;
            dto.ObservacionesMedicas = null;
            dto.AdecuacionesEducativas = null;

            return RespuestaDTO<EstudianteDTO?>.Exito(dto);
        }

        public async Task<RespuestaDTO<EstudianteDTO>> Crear(EstudianteCrearDTO dto)
        {
            Normalizar(dto);

            string? error = await ValidarDatos(dto, null, null, null);

            if (error != null)
                return RespuestaDTO<EstudianteDTO>.Error(error, 4201);

            var rol = await _rolRepository.GetRolByNombre(RolEstudiante);

            if (rol == null)
                return RespuestaDTO<EstudianteDTO>.Error("No existe el rol Estudiante en el sistema", 4202);

            var usuario = new DAL.Entities.Usuario
            {
                IdRol = rol.IdRol,
                Nombre = dto.Nombre,
                Apellido = dto.Apellidos,
                Correo = dto.Correo,
                PasswordHash = _passwordHashService.HashPassword(dto.Password),
                Estado = true
            };

            var estudiante = new DAL.Entities.Estudiante
            {
                Identificacion = dto.Identificacion,
                FechaNacimiento = dto.FechaNacimiento,
                FechaIngreso = dto.FechaIngreso!.Value,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                CorreoEmergencia = dto.CorreoEmergencia,
                IdGrado = dto.IdGrado,
                EstadoAcademico = dto.EstadoAcademico,
                Alergias = dto.Alergias,
                ObservacionesMedicas = dto.ObservacionesMedicas,
                AdecuacionesEducativas = dto.AdecuacionesEducativas,
                Estado = true
            };

            string? carnet = await _estudianteRepository.CrearConUsuario(estudiante, usuario);

            if (carnet == null)
                return RespuestaDTO<EstudianteDTO>.Error("No se pudo registrar el estudiante", 4203);

            var creado = await _estudianteRepository.GetById(estudiante.IdEstudiante);

            return RespuestaDTO<EstudianteDTO>.Exito(
                _mapper.Map<EstudianteDTO>(creado),
                $"Estudiante registrado correctamente. Carné asignado: {carnet}");
        }

        public async Task<RespuestaDTO<EstudianteDTO>> Actualizar(EstudianteDTO dto)
        {
            var actual = await _estudianteRepository.GetById(dto.IdEstudiante);

            if (actual == null)
                return RespuestaDTO<EstudianteDTO>.Error("Estudiante no encontrado", 404);

            Normalizar(dto);

            string? error = await ValidarDatos(dto, actual.IdEstudiante, actual.IdUsuario, actual.IdGrado);

            if (error != null)
                return RespuestaDTO<EstudianteDTO>.Error(error, 4204);

            var datos = new DAL.Entities.Estudiante
            {
                IdEstudiante = actual.IdEstudiante,
                Identificacion = dto.Identificacion,
                FechaNacimiento = dto.FechaNacimiento,
                FechaIngreso = dto.FechaIngreso!.Value,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                CorreoEmergencia = dto.CorreoEmergencia,
                IdGrado = dto.IdGrado,
                EstadoAcademico = dto.EstadoAcademico,
                Alergias = dto.Alergias,
                ObservacionesMedicas = dto.ObservacionesMedicas,
                AdecuacionesEducativas = dto.AdecuacionesEducativas
            };

            if (!await _estudianteRepository.Actualizar(datos, dto.Nombre, dto.Apellidos, dto.Correo))
                return RespuestaDTO<EstudianteDTO>.Error("No se pudo actualizar el estudiante", 4205);

            var actualizado = await _estudianteRepository.GetById(actual.IdEstudiante);

            return RespuestaDTO<EstudianteDTO>.Exito(
                _mapper.Map<EstudianteDTO>(actualizado),
                "Estudiante actualizado correctamente");
        }

        public async Task<RespuestaDTO<bool>> CambiarEstado(int idEstudiante, bool estado)
        {
            if (await _estudianteRepository.GetById(idEstudiante) == null)
                return RespuestaDTO<bool>.Error("Estudiante no encontrado", 404);

            if (!await _estudianteRepository.CambiarEstado(idEstudiante, estado))
                return RespuestaDTO<bool>.Error("No se pudo cambiar el estado del estudiante", 4206);

            return RespuestaDTO<bool>.Exito(
                true,
                estado ? "Estudiante activado correctamente" : "Estudiante inactivado correctamente");
        }

        private async Task<string?> ValidarDatos(
            EstudianteDTO dto, int? idEstudiante, int? idUsuarioActual, int? idGradoActual)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Apellidos) ||
                string.IsNullOrWhiteSpace(dto.Identificacion) ||
                string.IsNullOrWhiteSpace(dto.Correo))
            {
                return "Nombre, apellidos, identificación y correo son requeridos";
            }

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

            if (!dto.FechaNacimiento.HasValue)
                return "La fecha de nacimiento es requerida";

            if (dto.FechaNacimiento.Value >= hoy)
                return "La fecha de nacimiento debe ser anterior a hoy";

            if (dto.FechaNacimiento.Value < hoy.AddYears(-100))
                return "La fecha de nacimiento no es válida";

            if (!dto.FechaIngreso.HasValue)
                return "La fecha de ingreso es requerida";

            if (dto.FechaIngreso.Value > hoy)
                return "La fecha de ingreso no puede ser futura";

            if (dto.FechaIngreso.Value < dto.FechaNacimiento.Value)
                return "La fecha de ingreso no puede ser anterior a la fecha de nacimiento";

            if (!EstadosAcademicos.Contains(dto.EstadoAcademico))
                return "El estado académico no es válido";

            if (!dto.IdGrado.HasValue)
                return "El grado de referencia es requerido";

            var grado = await _gradoRepository.GetGradoById(dto.IdGrado.Value);

            // Al editar se permite conservar un grado que se inactivó después del registro
            bool gradoSinCambio = idGradoActual.HasValue && idGradoActual.Value == dto.IdGrado.Value;

            if (grado == null || (!grado.Estado && !gradoSinCambio))
                return "El grado seleccionado no existe o está inactivo";

            if (await _estudianteRepository.ExisteIdentificacion(dto.Identificacion, idEstudiante))
                return "Ya existe un estudiante registrado con esa identificación";

            var usuarioConCorreo = await _usuarioRepository.GetUsuarioByCorreo(dto.Correo);

            if (usuarioConCorreo != null && usuarioConCorreo.IdUsuario != idUsuarioActual)
                return "Ya existe una cuenta de usuario con ese correo";

            return null;
        }

        private static void Normalizar(EstudianteDTO dto)
        {
            dto.Nombre = dto.Nombre?.Trim() ?? string.Empty;
            dto.Apellidos = dto.Apellidos?.Trim() ?? string.Empty;
            dto.Identificacion = dto.Identificacion?.Trim().ToUpperInvariant() ?? string.Empty;
            dto.Correo = dto.Correo?.Trim() ?? string.Empty;
            dto.EstadoAcademico = dto.EstadoAcademico?.Trim() ?? string.Empty;
            dto.Telefono = Limpiar(dto.Telefono);
            dto.Direccion = Limpiar(dto.Direccion);
            dto.CorreoEmergencia = Limpiar(dto.CorreoEmergencia);
            dto.Alergias = Limpiar(dto.Alergias);
            dto.ObservacionesMedicas = Limpiar(dto.ObservacionesMedicas);
            dto.AdecuacionesEducativas = Limpiar(dto.AdecuacionesEducativas);
        }

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
