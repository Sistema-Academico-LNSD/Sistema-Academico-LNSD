using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public class DocenteService : IDocenteService
    {
        private readonly IDocenteRepository _docenteRepository;
        private readonly IMapper _mapper;

        public DocenteService(
            IDocenteRepository docenteRepository,
            IMapper mapper)
        {
            _docenteRepository = docenteRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<DocenteDTO>>> Buscar(string? texto, int? idArea, bool? estado)
        {
            var docentes = await _docenteRepository.Buscar(texto, idArea, estado);

            return RespuestaDTO<List<DocenteDTO>>.Exito(_mapper.Map<List<DocenteDTO>>(docentes));
        }

        public async Task<RespuestaDTO<DocenteDTO?>> GetById(int idDocente)
        {
            var docente = await _docenteRepository.GetById(idDocente);

            if (docente == null)
                return RespuestaDTO<DocenteDTO?>.Error("Docente no encontrado", 404);

            return RespuestaDTO<DocenteDTO?>.Exito(_mapper.Map<DocenteDTO>(docente));
        }

        public async Task<RespuestaDTO<DocenteDTO>> Crear(DocenteDTO docente)
        {
            Normalizar(docente);

            string? error = await ValidarDatos(docente, excluirIdDocente: null);

            if (error != null)
                return RespuestaDTO<DocenteDTO>.Error(error, 4001);

            var entidad = _mapper.Map<DAL.Entities.Docente>(docente);

            entidad.IdDocente = 0;
            entidad.Estado = true;
            entidad.IdUsuario = null;

            if (!await _docenteRepository.Crear(entidad))
                return RespuestaDTO<DocenteDTO>.Error("No se pudo registrar el docente", 4002);

            return RespuestaDTO<DocenteDTO>.Exito(docente, "Docente registrado correctamente");
        }

        public async Task<RespuestaDTO<DocenteDTO>> Actualizar(DocenteDTO docente)
        {
            if (await _docenteRepository.GetById(docente.IdDocente) == null)
                return RespuestaDTO<DocenteDTO>.Error("Docente no encontrado", 404);

            Normalizar(docente);

            string? error = await ValidarDatos(docente, excluirIdDocente: docente.IdDocente);

            if (error != null)
                return RespuestaDTO<DocenteDTO>.Error(error, 4001);

            if (!await _docenteRepository.Actualizar(_mapper.Map<DAL.Entities.Docente>(docente)))
                return RespuestaDTO<DocenteDTO>.Error("No se pudo actualizar el docente", 4003);

            return RespuestaDTO<DocenteDTO>.Exito(docente, "Docente actualizado correctamente");
        }

        public async Task<RespuestaDTO<bool>> CambiarEstado(int idDocente, bool estado)
        {
            if (await _docenteRepository.GetById(idDocente) == null)
                return RespuestaDTO<bool>.Error("Docente no encontrado", 404);

            if (!await _docenteRepository.CambiarEstado(idDocente, estado))
                return RespuestaDTO<bool>.Error("No se pudo cambiar el estado del docente", 4004);

            return RespuestaDTO<bool>.Exito(
                true,
                estado ? "Docente activado correctamente" : "Docente inactivado correctamente");
        }

        private async Task<string?> ValidarDatos(DocenteDTO docente, int? excluirIdDocente)
        {
            if (string.IsNullOrWhiteSpace(docente.Nombre) ||
                string.IsNullOrWhiteSpace(docente.Apellidos) ||
                string.IsNullOrWhiteSpace(docente.Identificacion) ||
                string.IsNullOrWhiteSpace(docente.Correo))
            {
                return "Nombre, apellidos, identificación y correo son requeridos";
            }

            if (await _docenteRepository.ExisteIdentificacion(docente.Identificacion, excluirIdDocente))
                return "Ya existe un docente registrado con esa identificación";

            if (await _docenteRepository.ExisteCorreo(docente.Correo, excluirIdDocente))
                return "Ya existe un docente registrado con ese correo";

            return null;
        }
        private static void Normalizar(DocenteDTO docente)
        {
            docente.Nombre = docente.Nombre?.Trim() ?? string.Empty;
            docente.Apellidos = docente.Apellidos?.Trim() ?? string.Empty;
            docente.Identificacion = docente.Identificacion?.Trim().ToUpperInvariant() ?? string.Empty;
            docente.Correo = docente.Correo?.Trim() ?? string.Empty;
            docente.Telefono = Limpiar(docente.Telefono);
            docente.Direccion = Limpiar(docente.Direccion);
            docente.Titulos = Limpiar(docente.Titulos);
            docente.Especialidad = Limpiar(docente.Especialidad);
        }
        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}