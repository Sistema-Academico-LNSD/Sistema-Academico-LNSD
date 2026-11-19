using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Asistencia
{

    public interface IAsistenciaService
    {
        Task<RespuestaDTO<List<HorarioDTO>>> GetMisClases(int idUsuario);

        Task<RespuestaDTO<AsistenciaListaDTO>> GetLista(int idUsuario, int idHorario, DateOnly fecha, bool puedeEditar);

        Task<RespuestaDTO<AsistenciaListaDTO>> Guardar(int idUsuario, AsistenciaGuardarDTO dto, bool puedeEditar);

        Task<RespuestaDTO<List<AsistenciaRegistroDTO>>> GetHistorial(
            int idUsuario, bool soloPropias, int idMatricula, int idCurso);

        Task<RespuestaDTO<OpcionesAsistenciaDTO>> GetOpciones(int idUsuario, bool soloPropias, int? idPeriodo);

        Task<RespuestaDTO<List<AsistenciaResumenDTO>>> GetResumen(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro);

        Task<RespuestaDTO<List<AsistenciaRegistroDTO>>> GetRegistros(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro);

        Task<RespuestaDTO<byte[]>> ExportarResumen(int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro);

        Task<RespuestaDTO<MiAsistenciaDTO>> GetMiAsistencia(int idUsuario, bool esEncargado, int? idPeriodo);

        Task<RespuestaDTO<CierreAsistenciaDTO>> GetCierre(int idPeriodo);

        Task<RespuestaDTO<CierreAsistenciaDTO>> CerrarPeriodo(int idUsuario, int idPeriodo);

        Task<RespuestaDTO<decimal?>> ObtenerPorcentaje(int idMatricula, int idCurso);
    }
}
