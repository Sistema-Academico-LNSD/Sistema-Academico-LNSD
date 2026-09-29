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

        public PermisoService(
            IModuloRepository moduloRepository,
            IRolPermisoRepository rolPermisoRepository)
        {
            _moduloRepository = moduloRepository;
            _rolPermisoRepository = rolPermisoRepository;
        }

        public async Task<RespuestaDTO<List<ModuloPermisoDTO>>> GetMatrizPermisos(int idRol)
        {
            if (idRol <= 0)
                return RespuestaDTO<List<ModuloPermisoDTO>>.Error("Rol inválido", 1018);

            var modulos = await _moduloRepository.GetModulos();
            var permisosDelRol = await _rolPermisoRepository.GetPermisosPorRol(idRol);

            var matriz = modulos
                .Select(modulo =>
                {
                    var permiso = permisosDelRol.FirstOrDefault(p => p.IdModulo == modulo.IdModulo);

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

            return RespuestaDTO<List<ModuloPermisoDTO>>.Exito(matriz);
        }

        public async Task<RespuestaDTO<bool>> GuardarPermisos(GuardarPermisosDTO dto)
        {
            if (dto.IdRol <= 0)
                return RespuestaDTO<bool>.Error("Rol inválido", 1019);

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
                return RespuestaDTO<bool>.Error("No se pudieron guardar los permisos", 1020);

            return RespuestaDTO<bool>.Exito(true, "Permisos actualizados correctamente");
        }

        public async Task<List<string>> ObtenerPermisosComoClaims(int idRol)
        {
            var permisos = await _rolPermisoRepository.GetPermisosPorRol(idRol);

            var claims = new List<string>();

            foreach (var permiso in permisos)
            {
                if (permiso.Modulo == null)
                    continue;

                string modulo = permiso.Modulo.Nombre;

                if (permiso.PuedeVer) claims.Add($"{modulo}:Ver");
                if (permiso.PuedeCrear) claims.Add($"{modulo}:Crear");
                if (permiso.PuedeEditar) claims.Add($"{modulo}:Editar");
                if (permiso.PuedeEliminar) claims.Add($"{modulo}:Eliminar");
            }

            return claims;
        }
    }
}