using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Encargado
{
    public interface IEncargadoService
    {
        Task<RespuestaDTO<List<EncargadoVinculoDTO>>> GetPorEstudiante(int idEstudiante);
        Task<RespuestaDTO<List<EstudianteEncargadoDTO>>> GetMisEstudiantes(int idUsuario);
        Task<RespuestaDTO<List<EncargadoBusquedaDTO>>> Buscar(string? texto, int idEstudiante);
        Task<RespuestaDTO<bool>> Vincular(VincularEncargadoDTO dto);
        Task<RespuestaDTO<bool>> CrearYVincular(CrearEncargadoDTO dto);
        Task<RespuestaDTO<bool>> DefinirPrincipal(int idEstudiante, int idEncargado);
        Task<RespuestaDTO<bool>> Desvincular(int idEstudiante, int idEncargado);
    }
}
