using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Grado;
using ProyectoLNSD_WApp.DAL.Repositories.Seccion;

namespace ProyectoLNSD_WApp.BLL.Service.Seccion
{
    public class SeccionService : ISeccionService
    {
        private readonly ISeccionRepository _seccionRepository;
        private readonly IGradoRepository _gradoRepository;
        private readonly IMapper _mapper;

        public SeccionService(
            ISeccionRepository seccionRepository,
            IGradoRepository gradoRepository,
            IMapper mapper)
        {
            _seccionRepository = seccionRepository;
            _gradoRepository = gradoRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<SeccionDTO?>> GetSeccionById(int id)
        {
            var seccion = await _seccionRepository.GetSeccionById(id);
            if (seccion == null)
                return RespuestaDTO<SeccionDTO?>.Error("Sección no encontrada", 404);

            return RespuestaDTO<SeccionDTO?>.Exito(Mapear(seccion));
        }

        public async Task<RespuestaDTO<SeccionDTO>> CreateSeccion(SeccionDTO seccion)
        {
            var validacion = await ValidarSeccion(seccion);
            if (validacion != null)
                return validacion;

            seccion.Nombre = seccion.Nombre.Trim();
            seccion.Estado = true;

            if (!await _seccionRepository.CreateSeccion(_mapper.Map<DAL.Entities.Seccion>(seccion)))
                return RespuestaDTO<SeccionDTO>.Error("No se pudo registrar la sección", 1002);

            return RespuestaDTO<SeccionDTO>.Exito(seccion, "Grupo o sección registrado correctamente");
        }

        public async Task<RespuestaDTO<SeccionDTO>> UpdateSeccion(SeccionDTO seccion)
        {
            if (seccion.IdSeccion <= 0)
                return RespuestaDTO<SeccionDTO>.Error("Sección inválida", 1003);

            var existente = await _seccionRepository.GetSeccionById(seccion.IdSeccion);
            if (existente == null)
                return RespuestaDTO<SeccionDTO>.Error("Sección no encontrada", 404);

            var validacion = await ValidarSeccion(seccion, seccion.IdSeccion);
            if (validacion != null)
                return validacion;

            seccion.Nombre = seccion.Nombre.Trim();
            seccion.Estado = existente.Estado;

            if (!await _seccionRepository.UpdateSeccion(_mapper.Map<DAL.Entities.Seccion>(seccion)))
                return RespuestaDTO<SeccionDTO>.Error("No se pudo actualizar la sección", 1004);

            return RespuestaDTO<SeccionDTO>.Exito(seccion, "Sección actualizada correctamente");
        }

        public async Task<RespuestaDTO<SeccionDTO>> CambiarEstadoSeccion(int id, bool estado)
        {
            var existente = await _seccionRepository.GetSeccionById(id);
            if (existente == null)
                return RespuestaDTO<SeccionDTO>.Error("Sección no encontrada", 404);

            if (estado && existente.Grado != null && !existente.Grado.Estado)
                return RespuestaDTO<SeccionDTO>.Error(
                    "No se puede activar la sección porque el grado asociado está inactivo", 1010);

            if (!await _seccionRepository.CambiarEstado(id, estado))
                return RespuestaDTO<SeccionDTO>.Error("No se pudo cambiar el estado de la sección", 1005);

            return RespuestaDTO<SeccionDTO>.Exito(null, estado
                ? "Sección activada correctamente"
                : "Sección desactivada correctamente");
        }

        private async Task<RespuestaDTO<SeccionDTO>?> ValidarSeccion(SeccionDTO seccion, int? excluirId = null)
        {
            if (seccion.IdGrado <= 0)
                return RespuestaDTO<SeccionDTO>.Error("Debe seleccionar un grado antes de continuar", 1001);

            if (string.IsNullOrWhiteSpace(seccion.Nombre))
                return RespuestaDTO<SeccionDTO>.Error("Complete los campos obligatorios de la sección", 1001);

            if (seccion.CapacidadMaxima <= 0)
                return RespuestaDTO<SeccionDTO>.Error("La capacidad máxima debe ser mayor a 0", 1001);

            var grado = await _gradoRepository.GetGradoById(seccion.IdGrado);
            if (grado == null || !grado.Estado)
                return RespuestaDTO<SeccionDTO>.Error(
                    "El grado seleccionado no existe o se encuentra inactivo", 1003);

            if (await _seccionRepository.ExisteNombreEnGrado(seccion.IdGrado, seccion.Nombre, excluirId))
                return RespuestaDTO<SeccionDTO>.Error(
                    "La sección ya existe para ese grado", 1006);

            return null;
        }

        private static SeccionDTO Mapear(DAL.Entities.Seccion seccion)
        {
            return new SeccionDTO
            {
                IdSeccion = seccion.IdSeccion,
                IdGrado = seccion.IdGrado,
                Nombre = seccion.Nombre,
                CapacidadMaxima = seccion.CapacidadMaxima,
                CuposDisponibles = seccion.CapacidadMaxima,
                Estado = seccion.Estado,
                CodigoGrado = seccion.Grado?.Codigo,
                NombreGrado = seccion.Grado?.Nombre,
                Nivel = seccion.Grado?.Nivel,
                EstadoGrado = seccion.Grado?.Estado ?? false
            };
        }
    }
}