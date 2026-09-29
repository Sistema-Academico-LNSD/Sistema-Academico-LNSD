using AutoMapper;
using System.Security.Cryptography;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Tiquete;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Tiquete
{
    public class TiqueteService : ITiqueteService
    {
        // Sin 0/O/1/I para que el código sea fácil de leer y dictar
        private const string CaracteresCodigo = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int LongitudSufijo = 6;
        private const int IntentosCodigo = 5;

        private readonly ITiqueteRepository _tiqueteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        public TiqueteService(
            ITiqueteRepository tiqueteRepository,
            IUsuarioRepository usuarioRepository,
            IMapper mapper)
        {
            _tiqueteRepository = tiqueteRepository;
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<TiqueteDTO>> GenerarTiquete(int idUsuario)
        {
            try
            {
                var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);

                if (usuario == null || !usuario.Estado)
                    return RespuestaDTO<TiqueteDTO>.Error("No tiene acceso para generar tiquetes", 1101);

                if (await _tiqueteRepository.GetTiqueteDisponibleByUsuario(idUsuario) != null)
                    return RespuestaDTO<TiqueteDTO>.Error(
                        "Ya existe un tiquete disponible. Debe utilizarlo antes de generar otro.", 1102);

                string? codigo = await GenerarCodigoUnico();

                if (codigo == null)
                    return RespuestaDTO<TiqueteDTO>.Error("No se pudo generar el tiquete", 1103);

                var tiquete = new DAL.Entities.Tiquete
                {
                    IdUsuario = idUsuario,
                    Codigo = codigo,
                    Estado = DAL.Entities.EstadoTiquete.Disponible,
                    FechaGeneracion = DateTime.UtcNow
                };

                if (!await _tiqueteRepository.CreateTiquete(tiquete))
                {
                    // Si la base lo rechazó porque el usuario ya tenía uno disponible
                    // (dos clics seguidos), se informa igual que en la validación normal.
                    if (await _tiqueteRepository.GetTiqueteDisponibleByUsuario(idUsuario) != null)
                        return RespuestaDTO<TiqueteDTO>.Error(
                            "Ya existe un tiquete disponible. Debe utilizarlo antes de generar otro.", 1102);

                    return RespuestaDTO<TiqueteDTO>.Error("No se pudo generar el tiquete", 1103);
                }

                return RespuestaDTO<TiqueteDTO>.Exito(
                    _mapper.Map<TiqueteDTO>(tiquete),
                    "Tiquete generado correctamente");
            }
            catch (Exception)
            {
                return RespuestaDTO<TiqueteDTO>.Error("No se pudo generar el tiquete. Intente nuevamente.", 1103);
            }
        }

        public async Task<RespuestaDTO<List<TiqueteDTO>>> GetMisTiquetes(int idUsuario)
        {
            try
            {
                var tiquetes = await _tiqueteRepository.GetTiquetesByUsuario(idUsuario);

                var lista = _mapper.Map<List<TiqueteDTO>>(tiquetes);

                return RespuestaDTO<List<TiqueteDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No existen tiquetes disponibles" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<TiqueteDTO>>.Error("No fue posible realizar la consulta", 1105);
            }
        }

        public async Task<RespuestaDTO<TiqueteDTO?>> GetTiqueteActual(int idUsuario)
        {
            try
            {
                // La lista viene ordenada del más reciente al más antiguo: el primero es
                // el disponible (si existe) o, si no, el último que fue utilizado.
                var tiquetes = await _tiqueteRepository.GetTiquetesByUsuario(idUsuario);

                var actual = tiquetes.FirstOrDefault();

                if (actual == null)
                    return RespuestaDTO<TiqueteDTO?>.Error("No existe un tiquete disponible", 1104);

                return RespuestaDTO<TiqueteDTO?>.Exito(_mapper.Map<TiqueteDTO>(actual));
            }
            catch (Exception)
            {
                return RespuestaDTO<TiqueteDTO?>.Error("No fue posible realizar la consulta", 1105);
            }
        }

        public async Task<RespuestaDTO<TiqueteDTO>> ValidarTiquete(string codigo, string correo)
        {
            try
            {
                var (tiquete, error) = await VerificarTiquete(codigo, correo);

                if (error != null)
                    return error;

                return RespuestaDTO<TiqueteDTO>.Exito(
                    _mapper.Map<TiqueteDTO>(tiquete),
                    "El tiquete es válido");
            }
            catch (Exception)
            {
                return RespuestaDTO<TiqueteDTO>.Error("No fue posible validar el tiquete", 1111);
            }
        }

        public async Task<RespuestaDTO<TiqueteDTO>> MarcarComoUtilizado(
            string codigo,
            string correo,
            int idUsuarioValidador)
        {
            try
            {
                // Se repiten las validaciones: inválido, no corresponde o ya utilizado
                // rechazan la operación y el estado no cambia.
                var (tiquete, error) = await VerificarTiquete(codigo, correo);

                if (error != null)
                    return error;

                DateTime ahora = DateTime.UtcNow;

                if (!await _tiqueteRepository.MarcarUtilizado(tiquete!.IdTiquete, idUsuarioValidador, ahora))
                    return RespuestaDTO<TiqueteDTO>.Error("El tiquete ya fue utilizado", 1108);

                var dto = _mapper.Map<TiqueteDTO>(tiquete);
                dto.Estado = DAL.Entities.EstadoTiquete.Utilizado;
                dto.FechaUtilizacion = ahora;

                return RespuestaDTO<TiqueteDTO>.Exito(dto, "Tiquete marcado como utilizado");
            }
            catch (Exception)
            {
                return RespuestaDTO<TiqueteDTO>.Error(
                    "No se pudo actualizar el estado. El tiquete conserva su estado anterior.", 1109);
            }
        }

        public async Task<RespuestaDTO<List<TiqueteDTO>>> GetTiquetes(string? estado)
        {
            try
            {
                bool hayFiltro = !string.IsNullOrWhiteSpace(estado);

                if (hayFiltro &&
                    estado != DAL.Entities.EstadoTiquete.Disponible &&
                    estado != DAL.Entities.EstadoTiquete.Utilizado)
                {
                    return RespuestaDTO<List<TiqueteDTO>>.Error("El estado indicado no es válido", 1112);
                }

                var tiquetes = await _tiqueteRepository.GetTiquetes(hayFiltro ? estado : null);

                var lista = _mapper.Map<List<TiqueteDTO>>(tiquetes);

                string? mensaje = null;

                if (lista.Count == 0)
                {
                    mensaje = hayFiltro
                        ? $"No existen tiquetes en estado {estado!.ToLower()}"
                        : "No existen registros de tiquetes";
                }

                return RespuestaDTO<List<TiqueteDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<TiqueteDTO>>.Error("No fue posible realizar la consulta", 1113);
            }
        }

        public async Task<RespuestaDTO<TiqueteResumenDTO>> GetResumen()
        {
            try
            {
                var resumen = new TiqueteResumenDTO
                {
                    Generados = await _tiqueteRepository.ContarTiquetes(),
                    Disponibles = await _tiqueteRepository.ContarTiquetes(DAL.Entities.EstadoTiquete.Disponible),
                    Utilizados = await _tiqueteRepository.ContarTiquetes(DAL.Entities.EstadoTiquete.Utilizado)
                };

                return RespuestaDTO<TiqueteResumenDTO>.Exito(resumen);
            }
            catch (Exception)
            {
                return RespuestaDTO<TiqueteResumenDTO>.Error("No fue posible realizar la consulta", 1113);
            }
        }

        // --- Auxiliares ---

        private async Task<string?> GenerarCodigoUnico()
        {
            
            string fecha = DateTime.Now.ToString("yyyyMMdd");

            for (int i = 0; i < IntentosCodigo; i++)
            {
                string codigo = $"TQ-{fecha}-{SufijoAleatorio()}";

                if (!await _tiqueteRepository.ExisteCodigo(codigo))
                    return codigo;
            }

            return null;
        }

        private static string SufijoAleatorio()
        {
            var caracteres = new char[LongitudSufijo];

            for (int i = 0; i < LongitudSufijo; i++)
                caracteres[i] = CaracteresCodigo[RandomNumberGenerator.GetInt32(CaracteresCodigo.Length)];

            return new string(caracteres);
        }

        /// Reglas comunes de HU 04 y HU 05. Devuelve el tiquete si todo está bien
        /// o la respuesta de error que corresponde.
        private async Task<(DAL.Entities.Tiquete? Tiquete, RespuestaDTO<TiqueteDTO>? Error)> VerificarTiquete(
            string codigo,
            string correo)
        {
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(correo))
                return (null, RespuestaDTO<TiqueteDTO>.Error(
                    "Debe indicar el código del tiquete y el correo de quien lo presenta", 1110));

            var tiquete = await _tiqueteRepository.GetTiqueteByCodigo(codigo.Trim().ToUpperInvariant());

            if (tiquete == null)
                return (null, RespuestaDTO<TiqueteDTO>.Error("El tiquete no es válido", 1106));

            if (!string.Equals(tiquete.Usuario?.Correo, correo.Trim(), StringComparison.OrdinalIgnoreCase))
                return (null, RespuestaDTO<TiqueteDTO>.Error(
                    "El tiquete no corresponde al usuario que lo presenta", 1107));

            if (tiquete.Estado == DAL.Entities.EstadoTiquete.Utilizado)
                return (null, RespuestaDTO<TiqueteDTO>.Error("El tiquete ya fue utilizado", 1108));

            return (tiquete, null);
        }
    }
}
