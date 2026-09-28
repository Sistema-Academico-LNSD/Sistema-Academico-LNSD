using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.ContenidoSitio;
using ProyectoLNSD_WApp.DAL.Repositories.Institucion;

namespace ProyectoLNSD_WApp.BLL.Service.ContenidoSitio
{
    public class ContenidoSitioService : IContenidoSitioService
    {
        // Códigos de error del módulo Configuración (33xx = contenido del sitio)
        public const int CodigoDatosRequeridos = 3301;
        public const int CodigoTipoInvalido = 3302;
        public const int CodigoNoGuardado = 3303;
        public const int CodigoBloqueIncompleto = 3304;
        public const int CodigoBloqueNoEncontrado = 3305;
        public const int CodigoBloqueNoGuardado = 3306;
        public const int CodigoBloqueNoEliminado = 3307;

        private const string TipoBloque = "Bloque";

        public const string EstadoBorrador = "Borrador";
        public const string EstadoPublicado = "Publicado";

        // tipo (BD) -> título que se muestra
        private static readonly Dictionary<string, string> TiposInstitucionales = new()
        {
            ["Mision"] = "Misión",
            ["Vision"] = "Visión",
            ["Historia"] = "Historia"
        };

        private readonly IContenidoSitioRepository _contenidoRepository;
        private readonly IInstitucionRepository _institucionRepository;
        private readonly IMapper _mapper;

        public ContenidoSitioService(IContenidoSitioRepository contenidoRepository,
                                     IInstitucionRepository institucionRepository,
                                     IMapper mapper)
        {
            _contenidoRepository = contenidoRepository;
            _institucionRepository = institucionRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<ContenidoSitioDTO>>> GetInstitucionales()
        {
            var filas = await _contenidoRepository.GetPorTipos(TiposInstitucionales.Keys);
            return RespuestaDTO<List<ContenidoSitioDTO>>.Exito(_mapper.Map<List<ContenidoSitioDTO>>(filas));
        }

        public async Task<RespuestaDTO<ContenidoSitioDTO>> GuardarInstitucional(ContenidoSitioDTO contenido, bool publicar, int? idUsuario)
        {
            if (!TiposInstitucionales.TryGetValue(contenido.Tipo, out var titulo))
                return RespuestaDTO<ContenidoSitioDTO>.Error("El tipo de contenido no es válido", CodigoTipoInvalido);

            // Sin texto no hay nada que guardar ni publicar
            if (string.IsNullOrWhiteSpace(contenido.Descripcion))
                return RespuestaDTO<ContenidoSitioDTO>.Error($"El texto de {titulo.ToLowerInvariant()} es requerido", CodigoDatosRequeridos);

            var existente = await _contenidoRepository.GetPorTipo(contenido.Tipo);

            var entidad = new DAL.Entities.ContenidoSitio
            {
                IdContenido = existente?.IdContenido ?? 0,
                Tipo = contenido.Tipo,
                Titulo = titulo,                                   // fijo, no viene del cliente
                Descripcion = contenido.Descripcion.Trim(),
                RutaImagen = existente?.RutaImagen,
                Orden = existente?.Orden ?? 0,
                Estado = publicar ? EstadoPublicado : EstadoBorrador,
                FechaPublicacion = publicar ? DateTime.UtcNow : existente?.FechaPublicacion,
                IdUsuarioModifica = idUsuario
            };

            bool guardado = existente == null
                ? await _contenidoRepository.CreateContenido(entidad)
                : await _contenidoRepository.UpdateContenido(entidad);

            if (!guardado)
                return RespuestaDTO<ContenidoSitioDTO>.Error($"No se pudo guardar {titulo.ToLowerInvariant()}", CodigoNoGuardado);

            string mensaje = publicar
                ? $"{titulo} publicada correctamente"
                : $"{titulo} guardada como borrador";

            return RespuestaDTO<ContenidoSitioDTO>.Exito(_mapper.Map<ContenidoSitioDTO>(entidad), mensaje);
        }

        // ---------- HU 04: bloques de la landing ----------

        public async Task<RespuestaDTO<List<ContenidoSitioDTO>>> GetBloques()
        {
            var filas = await _contenidoRepository.GetListaPorTipo(TipoBloque);
            return RespuestaDTO<List<ContenidoSitioDTO>>.Exito(_mapper.Map<List<ContenidoSitioDTO>>(filas));
        }

        public async Task<RespuestaDTO<ContenidoSitioDTO?>> GetBloque(int idContenido)
        {
            var fila = await _contenidoRepository.GetPorId(idContenido);

            if (fila == null || fila.Tipo != TipoBloque)
                return RespuestaDTO<ContenidoSitioDTO?>.Error("El contenido no existe", CodigoBloqueNoEncontrado);

            return RespuestaDTO<ContenidoSitioDTO?>.Exito(_mapper.Map<ContenidoSitioDTO>(fila));
        }

        public async Task<RespuestaDTO<ContenidoSitioDTO>> GuardarBloque(ContenidoSitioDTO contenido, bool publicar, int? idUsuario)
        {
            // HU escenario 4: contenido incompleto (aplica también al borrador)
            if (string.IsNullOrWhiteSpace(contenido.Titulo) || string.IsNullOrWhiteSpace(contenido.Descripcion))
                return RespuestaDTO<ContenidoSitioDTO>.Error("El título y el texto son requeridos", CodigoBloqueIncompleto);

            bool esNuevo = contenido.IdContenido == 0;
            DAL.Entities.ContenidoSitio? existente = null;

            if (!esNuevo)
            {
                existente = await _contenidoRepository.GetPorId(contenido.IdContenido);

                // Este endpoint solo toca bloques: no permite sobrescribir misión, visión, etc.
                if (existente == null || existente.Tipo != TipoBloque)
                    return RespuestaDTO<ContenidoSitioDTO>.Error("El contenido no existe", CodigoBloqueNoEncontrado);
            }

            var entidad = new DAL.Entities.ContenidoSitio
            {
                IdContenido = existente?.IdContenido ?? 0,
                Tipo = TipoBloque,                                 // siempre lo fija el servidor
                Titulo = contenido.Titulo.Trim(),
                Descripcion = contenido.Descripcion.Trim(),
                RutaImagen = existente?.RutaImagen,
                Orden = Math.Max(0, contenido.Orden),
                Estado = publicar ? EstadoPublicado : EstadoBorrador,
                FechaPublicacion = publicar ? DateTime.UtcNow : existente?.FechaPublicacion,
                IdUsuarioModifica = idUsuario
            };

            bool guardado = esNuevo
                ? await _contenidoRepository.CreateContenido(entidad)
                : await _contenidoRepository.UpdateContenido(entidad);

            if (!guardado)
                return RespuestaDTO<ContenidoSitioDTO>.Error("No se pudo guardar el contenido", CodigoBloqueNoGuardado);

            string mensaje = publicar
                ? "Contenido publicado correctamente"
                : "Contenido guardado como borrador";

            return RespuestaDTO<ContenidoSitioDTO>.Exito(_mapper.Map<ContenidoSitioDTO>(entidad), mensaje);
        }

        public async Task<RespuestaDTO<bool>> EliminarBloque(int idContenido)
        {
            var fila = await _contenidoRepository.GetPorId(idContenido);

            if (fila == null || fila.Tipo != TipoBloque)
                return RespuestaDTO<bool>.Error("El contenido no existe", CodigoBloqueNoEncontrado);

            if (!await _contenidoRepository.DeleteContenido(idContenido))
                return RespuestaDTO<bool>.Error("No se pudo eliminar el contenido", CodigoBloqueNoEliminado);

            return RespuestaDTO<bool>.Exito(true, "Contenido eliminado correctamente");
        }

        // HU escenario 5: visualización pública
        public async Task<RespuestaDTO<LandingDTO>> GetLandingPublica()
        {
            var landing = new LandingDTO();

            var institucion = await _institucionRepository.GetInstitucion();
            if (institucion != null)
                landing.Institucion = _mapper.Map<InstitucionDTO>(institucion);

            var filas = (await _contenidoRepository.GetPorTipos(TiposInstitucionales.Keys))
                .Where(c => c.Estado == EstadoPublicado)
                .ToList();

            ContenidoSitioDTO? Buscar(string tipo)
            {
                var fila = filas.FirstOrDefault(c => c.Tipo == tipo);
                return fila == null ? null : _mapper.Map<ContenidoSitioDTO>(fila);
            }

            landing.Mision = Buscar("Mision");
            landing.Vision = Buscar("Vision");
            landing.Historia = Buscar("Historia");

            // HU 04 escenario 5: los bloques sin publicar no se muestran
            var bloques = (await _contenidoRepository.GetListaPorTipo(TipoBloque))
                .Where(c => c.Estado == EstadoPublicado);
            landing.Bloques = _mapper.Map<List<ContenidoSitioDTO>>(bloques);

            return RespuestaDTO<LandingDTO>.Exito(landing);
        }
    }
}
