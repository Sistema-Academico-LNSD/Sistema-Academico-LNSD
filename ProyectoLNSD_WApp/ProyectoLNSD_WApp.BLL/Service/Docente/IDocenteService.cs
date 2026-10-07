using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public interface IDocenteService
    {
        Task<RespuestaDTO<List<DocenteDTO>>> Buscar(string? texto, int? idArea, bool? estado);
        Task<RespuestaDTO<DocenteDTO?>> GetById(int idDocente);
        Task<RespuestaDTO<DocenteDTO>> Crear(DocenteDTO docente);
        Task<RespuestaDTO<DocenteDTO>> Actualizar(DocenteDTO docente);
        Task<RespuestaDTO<bool>> CambiarEstado(int idDocente, bool estado);
    }
}