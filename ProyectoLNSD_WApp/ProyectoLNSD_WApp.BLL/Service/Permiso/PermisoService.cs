using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.PermisoActions;
using ProyectoLNSD_WApp.DAL.Repositories.Modulo;
using ProyectoLNSD_WApp.DAL.Repositories.RolPermiso;

namespace ProyectoLNSD_WApp.BLL.Service.Permiso
{
    public class PermisoService : IPermisoService
    {
        private readonly IModuloRepository _moduloRepository;
        private readonly IRolPermisoRepository _rolPermisoRepository;
        private readonly IMapper _mapper;

        public PermisoService(
            IModuloRepository moduloRepository,
            IRolPermisoRepository rolPermisoRepository,
            IMapper mapper)
        {
            _moduloRepository = moduloRepository;
            _rolPermisoRepository = rolPermisoRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<ModuloDTO>>> GetModulos()
        {
            var respuesta = new RespuestaDTO<List<ModuloDTO>>();

            var modulos = await _moduloRepository.GetModulos();

            respuesta.Dato = _mapper.Map<List<ModuloDTO>>(modulos);

            return respuesta;
        }

        public async Task<RespuestaDTO<List<ModuloPermisoDTO>>> GetMatrizPermisos(int idRol)
        {
            var respuesta = new RespuestaDTO<List<ModuloPermisoDTO>>();

            if (idRol <= 0)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Rol inválido";
                respuesta.Codigo = 1018;

                return respuesta;
            }

            var modulos = await _moduloRepository.GetModulos();
            var permisosDelRol = await _rolPermisoRepository.GetPermisosPorRol(idRol);

            respuesta.Dato = modulos
                .Select(modulo =>
                {
                    var permiso = permisosDelRol
                        .FirstOrDefault(p => p.IdModulo == modulo.IdModulo);

                    return new ModuloPermisoDTO
                    {
                        IdModulo = modulo.IdModulo,
                        NombreModulo = modulo.Nombre,
                        PuedeVer = permiso?.PuedeVer ?? false,
                        PuedeCrear = permiso?.PuedeCrear ?? false,
                        PuedeEditar = permiso?.PuedeEditar ?? false,
                        PuedeEliminar = permiso?.PuedeEliminar ?? false
                    };
                })
                .ToList();

            return respuesta;
        }

        public async Task<RespuestaDTO<bool>> GuardarPermisos(GuardarPermisosDTO dto)
        {
            var respuesta = new RespuestaDTO<bool>();

            if (dto.IdRol <= 0)
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "Rol inválido";
                respuesta.Codigo = 1019;

                return respuesta;
            }

            // Solo se guardan filas donde al menos una acción está habilitada;
            // un módulo sin ninguna marca simplemente no genera fila (equivale
            // a "sin permiso" cuando se relee con GetMatrizPermisos).
            var entidades = dto.Permisos
                .Where(p => p.PuedeVer || p.PuedeCrear || p.PuedeEditar || p.PuedeEliminar)
                .Select(p => new DAL.Entities.RolPermiso
                {
                    IdRol = dto.IdRol,
                    IdModulo = p.IdModulo,
                    PuedeVer = p.PuedeVer,
                    PuedeCrear = p.PuedeCrear,
                    PuedeEditar = p.PuedeEditar,
                    PuedeEliminar = p.PuedeEliminar
                })
                .ToList();

            if (!await _rolPermisoRepository.GuardarPermisos(dto.IdRol, entidades))
            {
                respuesta.EsCorrecto = false;
                respuesta.Mensaje = "No se pudieron guardar los permisos";
                respuesta.Codigo = 1020;

                return respuesta;
            }

            respuesta.Dato = true;
            respuesta.Mensaje = "Permisos actualizados correctamente";

            return respuesta;
        }

        public async Task<List<string>> ObtenerPermisosComoClaims(int idRol)
        {
            var permisos = await _rolPermisoRepository.GetPermisosPorRol(idRol);

            var claims = new List<string>();

            foreach (var permiso in permisos)
            {
                if (permiso.Modulo == null)
                    continue;

                if (permiso.PuedeVer)
                    claims.Add($"{permiso.Modulo.Nombre}:Ver");

                if (permiso.PuedeCrear)
                    claims.Add($"{permiso.Modulo.Nombre}:Crear");

                if (permiso.PuedeEditar)
                    claims.Add($"{permiso.Modulo.Nombre}:Editar");

                if (permiso.PuedeEliminar)
                    claims.Add($"{permiso.Modulo.Nombre}:Eliminar");
            }

            return claims;
        }
    }
}