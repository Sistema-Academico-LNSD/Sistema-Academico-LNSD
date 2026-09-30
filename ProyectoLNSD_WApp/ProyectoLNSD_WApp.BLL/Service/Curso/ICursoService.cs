using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.Curso;

namespace ProyectoLNSD_WApp.BLL.Service.Curso
{
    public interface ICursoService
    {
        Task<RespuestaDTO<List<CursoListaDTO>>> GetCursos(
            string? texto,
            int? idGrado,
            int? idArea,
            bool? estado);

        Task<RespuestaDTO<List<AreaAcademicaDTO>>> GetAreasActivas();

        Task<RespuestaDTO<List<GradoDTO>>> GetGradosActivos();

        Task<RespuestaDTO<CursoDTO?>> GetCursoById(int id);

        Task<RespuestaDTO<CursoDetalleDTO?>> GetDetalleCurso(int id);

        Task<RespuestaDTO<CursoDTO>> CreateCurso(CursoDTO curso);

        Task<RespuestaDTO<CursoDTO>> UpdateCurso(CursoDTO curso);

        Task<RespuestaDTO<CursoDTO>> CambiarEstadoCurso(
            int id,
            bool estado);

        Task<RespuestaDTO<bool>> EliminarCurso(int id);
    }
}