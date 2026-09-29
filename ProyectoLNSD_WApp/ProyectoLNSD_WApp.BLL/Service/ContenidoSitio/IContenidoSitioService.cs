using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.ContenidoSitio
{
    public interface IContenidoSitioService
    {
        // HU 05: filas existentes de Misión, Visión e Historia (incluye borradores, es para el administrador).
        Task<RespuestaDTO<List<ContenidoSitioDTO>>> GetInstitucionales();

        // publicar = true -> Estado Publicado; false -> Borrador.
        Task<RespuestaDTO<ContenidoSitioDTO>> GuardarInstitucional(ContenidoSitioDTO contenido, bool publicar, int? idUsuario);

        // HU 04: bloques de contenido de la landing (tipo Bloque)
        Task<RespuestaDTO<List<ContenidoSitioDTO>>> GetBloques();
        Task<RespuestaDTO<ContenidoSitioDTO?>> GetBloque(int idContenido);
        Task<RespuestaDTO<ContenidoSitioDTO>> GuardarBloque(ContenidoSitioDTO contenido, bool publicar, int? idUsuario);
        Task<RespuestaDTO<bool>> EliminarBloque(int idContenido);

        // HU 03: banners e imágenes (tipo Banner). nuevaRutaImagen: solo si se subió una imagen nueva.
        Task<RespuestaDTO<List<ContenidoSitioDTO>>> GetBanners();
        Task<RespuestaDTO<ContenidoSitioDTO?>> GetBanner(int idContenido);
        Task<RespuestaDTO<ContenidoSitioDTO>> GuardarBanner(ContenidoSitioDTO contenido, bool publicar, string? nuevaRutaImagen, int? idUsuario);
        Task<RespuestaDTO<bool>> EliminarBanner(int idContenido);

        // HU 08: publicar / despublicar cualquier contenido del sitio (misión, visión, historia, bloque, banner)
        Task<RespuestaDTO<ContenidoSitioDTO>> CambiarEstado(int idContenido, bool publicar, int? idUsuario);

        // HU 07: vista previa (igual que la página pública, pero incluye borradores) y publicación desde ella
        Task<RespuestaDTO<LandingDTO>> GetVistaPrevia();
        Task<RespuestaDTO<int>> PublicarPendientes(int? idUsuario);

        // Página pública: solo lo publicado.
        Task<RespuestaDTO<LandingDTO>> GetLandingPublica();
    }
}


