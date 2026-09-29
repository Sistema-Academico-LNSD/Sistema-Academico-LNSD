using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Grado;

namespace ProyectoLNSD_WApp.BLL.Service.Grado
{
    public class GradoService : IGradoService
    {
        private static readonly string[] NivelesValidos =
        {
            "Sétimo", "Octavo", "Noveno", "Décimo", "Undécimo"
        };

        private readonly IGradoRepository _gradoRepository;
        private readonly IMapper _mapper;

        public GradoService(IGradoRepository gradoRepository, IMapper mapper)
        {
            _gradoRepository = gradoRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<EstructuraAcademicaDTO>>> GetEstructura(string? texto, string? nivel)
        {
            var grados = await _gradoRepository.GetGrados();
            var filas = new List<EstructuraAcademicaDTO>();

            foreach (var grado in grados)
            {
                if (!grado.Secciones.Any())
                {
                    filas.Add(MapearFila(grado, null));
                    continue;
                }

                foreach (var seccion in grado.Secciones.OrderBy(s => s.Nombre))
                    filas.Add(MapearFila(grado, seccion));
            }

            if (!string.IsNullOrWhiteSpace(nivel))
                filas = filas
                    .Where(f => f.Nivel.Equals(nivel.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                string t = texto.Trim();
                filas = filas.Where(f =>
                    f.CodigoGrado.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    f.NombreGrado.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    f.Nivel.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    (f.NombreSeccion != null && f.NombreSeccion.Contains(t, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            return RespuestaDTO<List<EstructuraAcademicaDTO>>.Exito(filas);
        }

        public async Task<RespuestaDTO<List<GradoDTO>>> GetGradosActivos()
        {
            var grados = await _gradoRepository.GetGradosActivos();
            return RespuestaDTO<List<GradoDTO>>.Exito(_mapper.Map<List<GradoDTO>>(grados));
        }

        public async Task<RespuestaDTO<GradoDTO?>> GetGradoById(int id)
        {
            var grado = await _gradoRepository.GetGradoById(id);
            if (grado == null)
                return RespuestaDTO<GradoDTO?>.Error("Grado no encontrado", 404);

            return RespuestaDTO<GradoDTO?>.Exito(_mapper.Map<GradoDTO>(grado));
        }

        public async Task<RespuestaDTO<GradoDetalleDTO?>> GetDetalleGrado(int id)
        {
            var grado = await _gradoRepository.GetGradoConSecciones(id);
            if (grado == null)
                return RespuestaDTO<GradoDetalleDTO?>.Error("Grado no encontrado", 404);

            var detalle = new GradoDetalleDTO
            {
                Grado = _mapper.Map<GradoDTO>(grado),
                Secciones = grado.Secciones
                    .OrderBy(s => s.Nombre)
                    .Select(s => new SeccionDTO
                    {
                        IdSeccion = s.IdSeccion,
                        IdGrado = s.IdGrado,
                        Nombre = s.Nombre,
                        CapacidadMaxima = s.CapacidadMaxima,
                        CuposDisponibles = s.CapacidadMaxima,
                        Estado = s.Estado,
                        NombreGrado = grado.Nombre,
                        CodigoGrado = grado.Codigo,
                        Nivel = grado.Nivel,
                        EstadoGrado = grado.Estado
                    })
                    .ToList()
            };

            return RespuestaDTO<GradoDetalleDTO?>.Exito(detalle);
        }

        public async Task<RespuestaDTO<GradoDTO>> CreateGrado(GradoDTO grado)
        {
            var validacion = ValidarGrado(grado);
            if (validacion != null)
                return validacion;

            if (await _gradoRepository.ExisteNombre(grado.Nombre))
                return RespuestaDTO<GradoDTO>.Error("Ya existe un grado con ese nombre", 1006);

            if (await _gradoRepository.ExisteCodigo(grado.Codigo))
                return RespuestaDTO<GradoDTO>.Error("Ya existe un grado con ese código", 1006);

            grado.Codigo = grado.Codigo.Trim();
            grado.Nombre = grado.Nombre.Trim();
            grado.Nivel = grado.Nivel.Trim();
            grado.Estado = true;

            if (!await _gradoRepository.CreateGrado(_mapper.Map<DAL.Entities.Grado>(grado)))
                return RespuestaDTO<GradoDTO>.Error("No se pudo registrar el grado", 1002);

            return RespuestaDTO<GradoDTO>.Exito(grado, "Grado académico registrado correctamente");
        }

        public async Task<RespuestaDTO<GradoDTO>> UpdateGrado(GradoDTO grado)
        {
            if (grado.IdGrado <= 0)
                return RespuestaDTO<GradoDTO>.Error("Grado inválido", 1003);

            var existente = await _gradoRepository.GetGradoById(grado.IdGrado);
            if (existente == null)
                return RespuestaDTO<GradoDTO>.Error("Grado no encontrado", 404);

            var validacion = ValidarGrado(grado);
            if (validacion != null)
                return validacion;

            if (await _gradoRepository.ExisteNombre(grado.Nombre, grado.IdGrado))
                return RespuestaDTO<GradoDTO>.Error("Ya existe un grado con ese nombre", 1006);

            if (await _gradoRepository.ExisteCodigo(grado.Codigo, grado.IdGrado))
                return RespuestaDTO<GradoDTO>.Error("Ya existe un grado con ese código", 1006);

            grado.Codigo = grado.Codigo.Trim();
            grado.Nombre = grado.Nombre.Trim();
            grado.Nivel = grado.Nivel.Trim();
            grado.Estado = existente.Estado;

            if (!await _gradoRepository.UpdateGrado(_mapper.Map<DAL.Entities.Grado>(grado)))
                return RespuestaDTO<GradoDTO>.Error("No se pudo actualizar el grado", 1004);

            return RespuestaDTO<GradoDTO>.Exito(grado, "Grado actualizado correctamente");
        }

        public async Task<RespuestaDTO<GradoDTO>> CambiarEstadoGrado(int id, bool estado)
        {
            var existente = await _gradoRepository.GetGradoById(id);
            if (existente == null)
                return RespuestaDTO<GradoDTO>.Error("Grado no encontrado", 404);

            if (!await _gradoRepository.CambiarEstado(id, estado))
                return RespuestaDTO<GradoDTO>.Error("No se pudo cambiar el estado del grado", 1005);

            return RespuestaDTO<GradoDTO>.Exito(null, estado
                ? "Grado activado correctamente"
                : "Grado desactivado correctamente");
        }

        private static RespuestaDTO<GradoDTO>? ValidarGrado(GradoDTO grado)
        {
            if (string.IsNullOrWhiteSpace(grado.Codigo) ||
                string.IsNullOrWhiteSpace(grado.Nombre) ||
                string.IsNullOrWhiteSpace(grado.Nivel))
                return RespuestaDTO<GradoDTO>.Error("Complete los campos obligatorios del grado", 1001);

            if (!NivelesValidos.Contains(grado.Nivel.Trim()))
                return RespuestaDTO<GradoDTO>.Error("El nivel académico no es válido", 1001);

            return null;
        }

        private static EstructuraAcademicaDTO MapearFila(DAL.Entities.Grado grado, DAL.Entities.Seccion? seccion)
        {
            return new EstructuraAcademicaDTO
            {
                IdGrado = grado.IdGrado,
                IdSeccion = seccion?.IdSeccion,
                CodigoGrado = grado.Codigo,
                NombreGrado = grado.Nombre,
                Nivel = grado.Nivel,
                NombreSeccion = seccion?.Nombre,
                CapacidadMaxima = seccion?.CapacidadMaxima,
                CuposDisponibles = seccion?.CapacidadMaxima,
                Estado = seccion?.Estado ?? grado.Estado,
                EstadoGrado = grado.Estado
            };
        }
    }
}