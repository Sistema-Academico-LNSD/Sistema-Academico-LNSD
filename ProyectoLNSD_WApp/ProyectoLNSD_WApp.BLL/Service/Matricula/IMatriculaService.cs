using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.Matricula;

namespace ProyectoLNSD_WApp.BLL.Service.Matricula;

public interface IMatriculaService
{
    Task<RespuestaDTO<MatriculaDetalleDTO>> Crear(MatriculaCrearDTO dto, int? idUsuario);
    Task<RespuestaDTO<MatriculaDetalleDTO?>> GetById(int idMatricula);
    Task<RespuestaDTO<List<MatriculaDetalleDTO>>> Buscar(MatriculaFiltroDTO filtro);
    Task<RespuestaDTO<MatriculaDetalleDTO>> Actualizar(int idMatricula, MatriculaActualizarDTO dto, int? idUsuario);
    Task<RespuestaDTO<MatriculaDetalleDTO>> AsignarCursos(int idMatricula, List<int> idsCursos, int? idUsuario);
    Task<RespuestaDTO<MatriculaDetalleDTO>> CambiarEstado(int idMatricula, MatriculaEstadoDTO dto, int? idUsuario);
    Task<RespuestaDTO<MatriculaDetalleDTO>> Trasladar(int idMatricula, MatriculaTrasladoDTO dto, int? idUsuario);
    Task<RespuestaDTO<List<MatriculaDetalleDTO>>> HistorialEstudiante(int idEstudiante);
    Task<RespuestaDTO<List<MatriculaHistorialDTO>>> HistorialCambios(int idMatricula);
    Task<RespuestaDTO<MatriculaObservacionDTO>> AgregarObservacion(int idMatricula, MatriculaObservacionCrearDTO dto);
    Task<RespuestaDTO<List<MatriculaObservacionDTO>>> Observaciones(int idMatricula);
    Task<RespuestaDTO<MatriculaReporteDTO>> Reporte(MatriculaFiltroDTO filtro);
    Task<RespuestaDTO<List<MatriculaDetalleDTO>>> ConsultarPropias(int idUsuario, bool esEncargado);
}
