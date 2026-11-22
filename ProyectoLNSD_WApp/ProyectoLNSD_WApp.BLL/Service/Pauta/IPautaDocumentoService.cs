using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    /// Arma los documentos imprimibles de las pautas (MPPAF-01-08 y 01-09).
    /// Cada impresión queda en la bitácora del expediente.
    public interface IPautaDocumentoService
    {
        Task<RespuestaDTO<DocumentoPautaDTO>> GetFormulario(int idUsuario, string rol, int idFormulario);
        Task<RespuestaDTO<DocumentoPautaDTO>> GetExpediente(int idUsuario, string rol, int idExpediente);
    }
}
