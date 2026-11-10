using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Beca
{
    public interface IBecaService
    {
        Task<RespuestaDTO<List<RequisitoBecaDTO>>> GetRequisitos(bool incluirInactivos);

        Task<RespuestaDTO<RequisitoBecaDTO>> GuardarRequisito(RequisitoBecaGuardarDTO dto);

        Task<RespuestaDTO<RequisitoBecaDTO>> CambiarEstadoRequisito(int idRequisito, bool activo);
        Task<RespuestaDTO<OpcionesBecaDTO>> GetOpciones();
        Task<RespuestaDTO<List<EstudianteBecaDTO>>> BuscarEstudiantes(string? texto);
        Task<RespuestaDTO<BecaDetalleDTO>> Preparar(int idEstudiante, int idPeriodo);
        Task<RespuestaDTO<BecaDetalleDTO>> GetDetalle(int idBeca);
        Task<RespuestaDTO<BecaDetalleDTO>> Guardar(int idUsuario, BecaGuardarDTO dto);
        Task<RespuestaDTO<BecaDetalleDTO>> Activar(int idUsuario, int idBeca);
        Task<RespuestaDTO<BecaDetalleDTO>> Desactivar(int idUsuario, int idBeca, string? motivo);
        Task<RespuestaDTO<List<BecaListaDTO>>> Buscar(BecaFiltroDTO filtro);
        Task<RespuestaDTO<HistorialBecaDTO>> GetHistorial(int idEstudiante);
        Task<RespuestaDTO<List<MisBecasEstudianteDTO>>> GetMisBecas(int idUsuario, bool esEncargado);
    }
}
