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

        // Página pública: solo lo publicado.
        Task<RespuestaDTO<LandingDTO>> GetLandingPublica();
    }
}

