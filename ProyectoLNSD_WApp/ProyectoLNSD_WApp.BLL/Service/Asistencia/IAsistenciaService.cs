using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Asistencia
{
    /// Control de asistencia (MCAF-01).
    /// "idUsuario" sale siempre de la sesión. "soloPropias" = true cuando quien consulta es
    /// un docente: entonces solo ve las clases que él imparte.
    public interface IAsistenciaService
    {
        // ---- Docente: pasar lista ----

        /// MCAF-01-01: clases del docente en el período activo
        Task<RespuestaDTO<List<HorarioDTO>>> GetMisClases(int idUsuario);

        /// MCAF-01-02: estudiantes matriculados en la clase, con lo ya registrado en esa fecha
        Task<RespuestaDTO<AsistenciaListaDTO>> GetLista(int idUsuario, int idHorario, DateOnly fecha, bool puedeEditar);

        /// MCAF-01-01, 03, 04 y 05: guarda la lista completa (crea o corrige)
        Task<RespuestaDTO<AsistenciaListaDTO>> Guardar(int idUsuario, AsistenciaGuardarDTO dto, bool puedeEditar);

        /// MCAF-01-06: historial de un estudiante en un curso
        Task<RespuestaDTO<List<AsistenciaRegistroDTO>>> GetHistorial(
            int idUsuario, bool soloPropias, int idMatricula, int idCurso);

        // ---- Consulta y reportes ----

        Task<RespuestaDTO<OpcionesAsistenciaDTO>> GetOpciones(int idUsuario, bool soloPropias, int? idPeriodo);

        /// MCAF-01-09, 01-10 y 01-12: porcentaje por estudiante y curso
        Task<RespuestaDTO<List<AsistenciaResumenDTO>>> GetResumen(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro);

        /// MCAF-01-09: registros individuales
        Task<RespuestaDTO<List<AsistenciaRegistroDTO>>> GetRegistros(
            int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro);

        /// MCAF-01-12: el resumen en Excel
        Task<RespuestaDTO<byte[]>> ExportarResumen(int idUsuario, bool soloPropias, AsistenciaFiltroDTO filtro);

        // ---- Estudiante y encargado ----

        /// MCAF-01-07 y 01-08
        Task<RespuestaDTO<MiAsistenciaDTO>> GetMiAsistencia(int idUsuario, bool esEncargado, int? idPeriodo);

        // ---- Cierre (MCAF-01-13) ----

        Task<RespuestaDTO<CierreAsistenciaDTO>> GetCierre(int idPeriodo);

        Task<RespuestaDTO<CierreAsistenciaDTO>> CerrarPeriodo(int idUsuario, int idPeriodo);

        // ---- Para el módulo de Notas (MCAF-01-11) ----

        /// Porcentaje de asistencia de una matrícula en un curso (null si no hay registros).
        /// Es el punto de integración para que Notas lo use en la calificación final.
        Task<RespuestaDTO<decimal?>> ObtenerPorcentaje(int idMatricula, int idCurso);
    }
}
