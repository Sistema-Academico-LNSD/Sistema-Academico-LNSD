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

        // 32xx = banners e imágenes (3204 lo usa el controller cuando el archivo no es imagen válida)
        public const int CodigoBannerSinTitulo = 3201;
        public const int CodigoBannerSinDescripcion = 3202;
        public const int CodigoBannerSinImagen = 3203;
        public const int CodigoBannerNoEncontrado = 3205;
        public const int CodigoBannerNoGuardado = 3206;
        public const int CodigoBannerNoEliminado = 3207;

        private const string TipoBanner = "Banner";

        // 34xx = publicar / despublicar
        public const int CodigoEstadoNoEncontrado = 3401;
        public const int CodigoEstadoIncompleto = 3402;
        public const int CodigoEstadoNoGuardado = 3403;
        public const int CodigoSinPendientes = 3404;

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

        // ---------- HU 03: banners e imágenes ----------

        public async Task<RespuestaDTO<List<ContenidoSitioDTO>>> GetBanners()
        {
            var filas = await _contenidoRepository.GetListaPorTipo(TipoBanner);
            return RespuestaDTO<List<ContenidoSitioDTO>>.Exito(_mapper.Map<List<ContenidoSitioDTO>>(filas));
        }

        public async Task<RespuestaDTO<ContenidoSitioDTO?>> GetBanner(int idContenido)
        {
            var fila = await _contenidoRepository.GetPorId(idContenido);

            if (fila == null || fila.Tipo != TipoBanner)
                return RespuestaDTO<ContenidoSitioDTO?>.Error("El banner no existe", CodigoBannerNoEncontrado);

            return RespuestaDTO<ContenidoSitioDTO?>.Exito(_mapper.Map<ContenidoSitioDTO>(fila));
        }

        public async Task<RespuestaDTO<ContenidoSitioDTO>> GuardarBanner(ContenidoSitioDTO contenido, bool publicar, string? nuevaRutaImagen, int? idUsuario)
        {
            // El título es obligatorio siempre (la columna no admite vacío)
            if (string.IsNullOrWhiteSpace(contenido.Titulo))
                return RespuestaDTO<ContenidoSitioDTO>.Error("El título es requerido", CodigoBannerSinTitulo);

            // HU escenario 5: para publicar se exige también la descripción
            if (publicar && string.IsNullOrWhiteSpace(contenido.Descripcion))
                return RespuestaDTO<ContenidoSitioDTO>.Error("Para publicar debe completar el título y la descripción", CodigoBannerSinDescripcion);

            bool esNuevo = contenido.IdContenido == 0;
            DAL.Entities.ContenidoSitio? existente = null;

            if (esNuevo)
            {
                if (string.IsNullOrWhiteSpace(nuevaRutaImagen))
                    return RespuestaDTO<ContenidoSitioDTO>.Error("Seleccione una imagen para el banner", CodigoBannerSinImagen);
            }
            else
            {
                existente = await _contenidoRepository.GetPorId(contenido.IdContenido);

                if (existente == null || existente.Tipo != TipoBanner)
                    return RespuestaDTO<ContenidoSitioDTO>.Error("El banner no existe", CodigoBannerNoEncontrado);
            }

            var entidad = new DAL.Entities.ContenidoSitio
            {
                IdContenido = existente?.IdContenido ?? 0,
                Tipo = TipoBanner,
                Titulo = contenido.Titulo.Trim(),
                Descripcion = contenido.Descripcion?.Trim(),
                // La ruta nunca viene del cliente: solo cambia si el controller guardó un archivo nuevo
                RutaImagen = nuevaRutaImagen ?? existente?.RutaImagen,
                Orden = Math.Max(0, contenido.Orden),
                Estado = publicar ? EstadoPublicado : EstadoBorrador,
                FechaPublicacion = publicar ? DateTime.UtcNow : existente?.FechaPublicacion,
                IdUsuarioModifica = idUsuario
            };

            bool guardado = esNuevo
                ? await _contenidoRepository.CreateContenido(entidad)
                : await _contenidoRepository.UpdateContenido(entidad);

            if (!guardado)
                return RespuestaDTO<ContenidoSitioDTO>.Error("No se pudo guardar el banner", CodigoBannerNoGuardado);

            string mensaje = publicar
                ? "Banner publicado correctamente"
                : "Banner guardado como borrador";

            return RespuestaDTO<ContenidoSitioDTO>.Exito(_mapper.Map<ContenidoSitioDTO>(entidad), mensaje);
        }

        public async Task<RespuestaDTO<bool>> EliminarBanner(int idContenido)
        {
            var fila = await _contenidoRepository.GetPorId(idContenido);

            if (fila == null || fila.Tipo != TipoBanner)
                return RespuestaDTO<bool>.Error("El banner no existe", CodigoBannerNoEncontrado);

            if (!await _contenidoRepository.DeleteContenido(idContenido))
                return RespuestaDTO<bool>.Error("No se pudo eliminar el banner", CodigoBannerNoEliminado);

            return RespuestaDTO<bool>.Exito(true, "Banner eliminado correctamente");
        }

        // ---------- HU 08: publicar / despublicar ----------

        public async Task<RespuestaDTO<ContenidoSitioDTO>> CambiarEstado(int idContenido, bool publicar, int? idUsuario)
        {
            var fila = await _contenidoRepository.GetPorId(idContenido);

            if (fila == null)
                return RespuestaDTO<ContenidoSitioDTO>.Error("El contenido no existe", CodigoEstadoNoEncontrado);

            // HU escenario 3: no se publica contenido incompleto y se indican los campos que faltan
            if (publicar)
            {
                var faltantes = CamposFaltantes(fila);
                if (faltantes.Count > 0)
                    return RespuestaDTO<ContenidoSitioDTO>.Error(
                        $"No se puede publicar. Faltan: {string.Join(", ", faltantes)}", CodigoEstadoIncompleto);
            }

            fila.Estado = publicar ? EstadoPublicado : EstadoBorrador;
            if (publicar)
                fila.FechaPublicacion = DateTime.UtcNow;
            fila.IdUsuarioModifica = idUsuario;

            if (!await _contenidoRepository.UpdateContenido(fila))
                return RespuestaDTO<ContenidoSitioDTO>.Error("No se pudo cambiar el estado del contenido", CodigoEstadoNoGuardado);

            string mensaje = publicar
                ? "Contenido publicado correctamente"
                : "Contenido despublicado: ya no se muestra en el sitio web";

            return RespuestaDTO<ContenidoSitioDTO>.Exito(_mapper.Map<ContenidoSitioDTO>(fila), mensaje);
        }

        // Información mínima para publicar según el tipo de contenido
        private static List<string> CamposFaltantes(DAL.Entities.ContenidoSitio c)
        {
            var faltan = new List<string>();

            if (string.IsNullOrWhiteSpace(c.Titulo))
                faltan.Add("título");

            if (string.IsNullOrWhiteSpace(c.Descripcion))
                faltan.Add("descripción");

            if (c.Tipo == TipoBanner && string.IsNullOrWhiteSpace(c.RutaImagen))
                faltan.Add("imagen");

            return faltan;
        }

        // HU escenario 5: visualización pública
        public Task<RespuestaDTO<LandingDTO>> GetLandingPublica() => ConstruirLanding(soloPublicado: true);

        // HU 07: lo mismo que ve el público, pero con los borradores incluidos
        public Task<RespuestaDTO<LandingDTO>> GetVistaPrevia() => ConstruirLanding(soloPublicado: false);

        // HU 07 escenario 5: publica de una vez todos los borradores que estén completos
        public async Task<RespuestaDTO<int>> PublicarPendientes(int? idUsuario)
        {
            var tipos = TiposInstitucionales.Keys.Append(TipoBloque).Append(TipoBanner);
            var pendientes = (await _contenidoRepository.GetPorTipos(tipos))
                .Where(c => c.Estado == EstadoBorrador)
                .ToList();

            if (pendientes.Count == 0)
                return RespuestaDTO<int>.Error("No hay cambios pendientes por publicar", CodigoSinPendientes);

            int publicados = 0;
            var omitidos = new List<string>();

            foreach (var fila in pendientes)
            {
                var faltantes = CamposFaltantes(fila);
                if (faltantes.Count > 0)
                {
                    omitidos.Add($"{fila.Titulo} (faltan: {string.Join(", ", faltantes)})");
                    continue;
                }

                fila.Estado = EstadoPublicado;
                fila.FechaPublicacion = DateTime.UtcNow;
                fila.IdUsuarioModifica = idUsuario;

                if (await _contenidoRepository.UpdateContenido(fila))
                    publicados++;
                else
                    omitidos.Add($"{fila.Titulo} (no se pudo guardar)");
            }

            if (publicados == 0)
                return RespuestaDTO<int>.Error($"No se publicó nada. No se publicaron: {string.Join("; ", omitidos)}", CodigoEstadoIncompleto);

            string mensaje = $"Se publicaron {publicados} contenido(s) correctamente";
            if (omitidos.Count > 0)
                mensaje += $". No se publicaron: {string.Join("; ", omitidos)}";

            return RespuestaDTO<int>.Exito(publicados, mensaje);
        }

        private async Task<RespuestaDTO<LandingDTO>> ConstruirLanding(bool soloPublicado)
        {
            var landing = new LandingDTO();

            var institucion = await _institucionRepository.GetInstitucion();
            if (institucion != null)
                landing.Institucion = _mapper.Map<InstitucionDTO>(institucion);

            var filas = (await _contenidoRepository.GetPorTipos(TiposInstitucionales.Keys))
                .Where(c => !soloPublicado || c.Estado == EstadoPublicado)
                .ToList();

            ContenidoSitioDTO? Buscar(string tipo)
            {
                var fila = filas.FirstOrDefault(c => c.Tipo == tipo);
                return fila == null ? null : _mapper.Map<ContenidoSitioDTO>(fila);
            }

            landing.Mision = Buscar("Mision");
            landing.Vision = Buscar("Vision");
            landing.Historia = Buscar("Historia");

            // HU 04 escenario 5: los bloques sin publicar no se muestran (salvo en la vista previa)
            var bloques = (await _contenidoRepository.GetListaPorTipo(TipoBloque))
                .Where(c => !soloPublicado || c.Estado == EstadoPublicado);
            landing.Bloques = _mapper.Map<List<ContenidoSitioDTO>>(bloques);

            // HU 03: solo banners publicados
            var banners = (await _contenidoRepository.GetListaPorTipo(TipoBanner))
                .Where(c => !soloPublicado || c.Estado == EstadoPublicado);
            landing.Banners = _mapper.Map<List<ContenidoSitioDTO>>(banners);

            return RespuestaDTO<LandingDTO>.Exito(landing);
        }
    }
}
