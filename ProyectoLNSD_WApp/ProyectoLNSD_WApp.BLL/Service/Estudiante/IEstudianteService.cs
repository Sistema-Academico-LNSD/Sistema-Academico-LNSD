using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Estudiante
{
    public interface IEstudianteService
    {
        Task<RespuestaDTO<List<EstudianteDTO>>> Buscar(string? texto, int? idGrado, bool? estado);
        Task<RespuestaDTO<EstudianteDTO?>> GetById(int idEstudiante);
        Task<RespuestaDTO<EstudianteDTO>> Crear(EstudianteCrearDTO estudiante);
        Task<RespuestaDTO<EstudianteDTO>> Actualizar(EstudianteDTO estudiante);
        Task<RespuestaDTO<bool>> CambiarEstado(int idEstudiante, bool estado);
    }
}