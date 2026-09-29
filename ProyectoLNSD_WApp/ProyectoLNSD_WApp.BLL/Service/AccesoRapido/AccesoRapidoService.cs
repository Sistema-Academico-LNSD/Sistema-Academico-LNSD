using System.Text.RegularExpressions;
using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.AccesoRapido;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;

namespace ProyectoLNSD_WApp.BLL.Service.AccesoRapido
{
    public class AccesoRapidoService : IAccesoRapidoService
    {
        // Códigos de error del módulo Configuración (35xx = accesos rápidos)
        public const int CodigoDatosRequeridos = 3501;
        public const int CodigoEnlaceInvalido = 3502;
        public const int CodigoNoEncontrado = 3503;
        public const int CodigoNoGuardado = 3504;
        public const int CodigoNoEliminado = 3505;
        public const int CodigoRolInvalido = 3506;
        public const int CodigoIconoInvalido = 3507;
        public const int CodigoSinRoles = 3508;

        private static readonly Regex IconoRegex = new(@"^bi-[a-z0-9-]+$", RegexOptions.Compiled);

        private readonly IAccesoRapidoRepository _accesoRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IMapper _mapper;

        public AccesoRapidoService(IAccesoRapidoRepository accesoRepository,
                                   IRolRepository rolRepository,
                                   IMapper mapper)
        {
            _accesoRepository = accesoRepository;
            _rolRepository = rolRepository;
            _mapper = mapper;
        }

        // Enlace válido = ruta interna ("/Usuario/Index") o dirección http/https completa con dominio.
        // No se aceptan otros esquemas (javascript:, data:, etc.) porque el enlace se usa como href.
        public static bool EnlaceEsValido(string? enlace)
        {
            if (string.IsNullOrWhiteSpace(enlace) || enlace.Length > 250 || enlace.Any(char.IsWhiteSpace))
                return false;

            if (enlace.StartsWith('/'))
                return !enlace.StartsWith("//") && !enlace.Contains('\\');

            if (!Uri.TryCreate(enlace, UriKind.Absolute, out var uri))
                return false;

            bool esquemaValido = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            return esquemaValido && (uri.Host.Contains('.') || uri.IsLoopback);
        }

        // Entidades -> DTO, agregando los roles asignados a cada acceso
        private async Task<List<AccesoRapidoDTO>> ConRoles(IEnumerable<DAL.Entities.AccesoRapido> filas)
        {
            var asignaciones = await _accesoRepository.GetAsignaciones();
            var dtos = _mapper.Map<List<AccesoRapidoDTO>>(filas);

            foreach (var dto in dtos)
            {
                var suyas = asignaciones.Where(x => x.IdAcceso == dto.IdAcceso).ToList();

                dto.IdsRoles = suyas.Select(x => x.IdRol).ToList();
                dto.NombresRoles = suyas
                    .Select(x => x.Rol?.Nombre ?? string.Empty)
                    .Where(n => n != string.Empty)
                    .OrderBy(n => n)
                    .ToList();
            }

            return dtos;
        }

        public async Task<RespuestaDTO<List<AccesoRapidoDTO>>> GetAccesos()
        {
            var filas = await _accesoRepository.GetAccesos();
            return RespuestaDTO<List<AccesoRapidoDTO>>.Exito(await ConRoles(filas));
        }

        public async Task<RespuestaDTO<AccesoRapidoDTO?>> GetAcceso(int idAcceso)
        {
            var fila = await _accesoRepository.GetAcceso(idAcceso);

            if (fila == null)
                return RespuestaDTO<AccesoRapidoDTO?>.Error("El acceso rápido no existe", CodigoNoEncontrado);

            var dto = (await ConRoles(new[] { fila })).First();
            return RespuestaDTO<AccesoRapidoDTO?>.Exito(dto);
        }

        public async Task<RespuestaDTO<List<RolOpcionDTO>>> GetRoles()
        {
            var roles = await _rolRepository.GetRoles();

            var opciones = roles
                .Select(r => new RolOpcionDTO { IdRol = r.IdRol, Nombre = r.Nombre })
                .OrderBy(r => r.Nombre)
                .ToList();

            return RespuestaDTO<List<RolOpcionDTO>>.Exito(opciones);
        }

        public async Task<RespuestaDTO<AccesoRapidoDTO>> GuardarAcceso(AccesoRapidoDTO acceso)
        {
            // HU escenario 1: nombre y enlace obligatorios
            if (string.IsNullOrWhiteSpace(acceso.Nombre) || string.IsNullOrWhiteSpace(acceso.Enlace))
                return RespuestaDTO<AccesoRapidoDTO>.Error("El nombre y el enlace son requeridos", CodigoDatosRequeridos);

            // HU escenario 4: URL incorrecta o incompleta
            string enlace = acceso.Enlace.Trim();
            if (!EnlaceEsValido(enlace))
                return RespuestaDTO<AccesoRapidoDTO>.Error(
                    "El enlace no es válido. Use una dirección completa (https://sitio.com) o una ruta interna (/Usuario/Index)",
                    CodigoEnlaceInvalido);

            string? icono = string.IsNullOrWhiteSpace(acceso.Icono) ? null : acceso.Icono.Trim();
            if (icono != null && !IconoRegex.IsMatch(icono))
                return RespuestaDTO<AccesoRapidoDTO>.Error(
                    "El ícono no es válido. Use el nombre de Bootstrap Icons, por ejemplo: bi-people",
                    CodigoIconoInvalido);

            // Cada acceso debe estar asignado al menos a un rol; si no, nadie lo vería
            var idsRoles = acceso.IdsRoles.Distinct().ToList();
            if (idsRoles.Count == 0)
                return RespuestaDTO<AccesoRapidoDTO>.Error("Seleccione al menos un rol que verá este acceso", CodigoSinRoles);

            var rolesExistentes = (await _rolRepository.GetRoles()).Select(r => r.IdRol).ToHashSet();
            if (idsRoles.Any(id => !rolesExistentes.Contains(id)))
                return RespuestaDTO<AccesoRapidoDTO>.Error("Uno de los roles seleccionados no existe", CodigoRolInvalido);

            bool esNuevo = acceso.IdAcceso == 0;

            if (!esNuevo && await _accesoRepository.GetAcceso(acceso.IdAcceso) == null)
                return RespuestaDTO<AccesoRapidoDTO>.Error("El acceso rápido no existe", CodigoNoEncontrado);

            var entidad = new DAL.Entities.AccesoRapido
            {
                IdAcceso = acceso.IdAcceso,
                Nombre = acceso.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(acceso.Descripcion) ? null : acceso.Descripcion.Trim(),
                Icono = icono,
                Enlace = enlace,
                Orden = Math.Max(0, acceso.Orden),
                Activo = acceso.Activo
            };

            bool guardado = esNuevo
                ? await _accesoRepository.CreateAcceso(entidad, idsRoles)
                : await _accesoRepository.UpdateAcceso(entidad, idsRoles);

            if (!guardado)
                return RespuestaDTO<AccesoRapidoDTO>.Error("No se pudo guardar el acceso rápido", CodigoNoGuardado);

            string mensaje = esNuevo
                ? "Acceso rápido registrado correctamente"
                : "Acceso rápido actualizado correctamente";

            var resultado = _mapper.Map<AccesoRapidoDTO>(entidad);
            resultado.IdsRoles = idsRoles;

            return RespuestaDTO<AccesoRapidoDTO>.Exito(resultado, mensaje);
        }

        public async Task<RespuestaDTO<bool>> EliminarAcceso(int idAcceso)
        {
            if (await _accesoRepository.GetAcceso(idAcceso) == null)
                return RespuestaDTO<bool>.Error("El acceso rápido no existe", CodigoNoEncontrado);

            if (!await _accesoRepository.DeleteAcceso(idAcceso))
                return RespuestaDTO<bool>.Error("No se pudo eliminar el acceso rápido", CodigoNoEliminado);

            return RespuestaDTO<bool>.Exito(true, "Acceso rápido eliminado correctamente");
        }

        // HU escenario 5: los inactivos y los de otros roles no llegan a Home. También se descartan
        // enlaces inválidos (por ejemplo, si alguien editó la BD a mano) para no generar un href peligroso.
        public async Task<RespuestaDTO<List<AccesoRapidoDTO>>> GetActivosParaRoles(IEnumerable<string> roles)
        {
            var lista = roles.ToList();

            if (lista.Count == 0)
                return RespuestaDTO<List<AccesoRapidoDTO>>.Exito(new List<AccesoRapidoDTO>());

            var filas = (await _accesoRepository.GetActivosPorRol(lista))
                .Where(a => EnlaceEsValido(a.Enlace))
                .ToList();

            return RespuestaDTO<List<AccesoRapidoDTO>>.Exito(_mapper.Map<List<AccesoRapidoDTO>>(filas));
        }
    }
}
