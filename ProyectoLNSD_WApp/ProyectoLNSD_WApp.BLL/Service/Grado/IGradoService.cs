using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Grado
{
    public interface IGradoService
    {
        Task<RespuestaDTO<List<EstructuraAcademicaDTO>>> GetEstructura(string? texto, string? nivel);
        Task<RespuestaDTO<List<GradoDTO>>> GetGradosActivos();
        Task<RespuestaDTO<GradoDTO?>> GetGradoById(int id);
        Task<RespuestaDTO<GradoDetalleDTO?>> GetDetalleGrado(int id);
        Task<RespuestaDTO<GradoDTO>> CreateGrado(GradoDTO grado);
        Task<RespuestaDTO<GradoDTO>> UpdateGrado(GradoDTO grado);
        Task<RespuestaDTO<GradoDTO>> CambiarEstadoGrado(int id, bool estado);
    }
}