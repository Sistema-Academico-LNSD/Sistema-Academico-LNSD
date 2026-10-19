using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Recurso
{
    public interface IRecursoService
    {
        // Cursos asignados al docente en el período activo, con sus grados (para el formulario)
        Task<RespuestaDTO<List<CursoRecursoDTO>>> GetMisCursos(int idUsuario);

        // MRAF-01-01, 01-02 y 01-03: publica un archivo o un enlace
        Task<RespuestaDTO<RecursoDTO>> Publicar(int idUsuario, RecursoGuardarDTO dto, ArchivoRecursoDTO? archivo);

        // MRAF-01-06 y 01-07: recursos del docente, con filtros y orden
        Task<RespuestaDTO<List<RecursoDTO>>> GetMisRecursos(int idUsuario, RecursoFiltroDTO filtro);

        // Para cargar el formulario de edición
        Task<RespuestaDTO<RecursoDTO>> GetRecurso(int idUsuario, int idRecurso);

        // MRAF-01-04
        Task<RespuestaDTO<RecursoDTO>> Actualizar(int idUsuario, RecursoGuardarDTO dto, ArchivoRecursoDTO? archivo);

        // MRAF-01-05
        Task<RespuestaDTO<bool>> Eliminar(int idUsuario, int idRecurso);

        // MRAF-01-08: filtros del estudiante (su grado y los cursos de ese grado)
        Task<RespuestaDTO<FiltrosMaterialDTO>> GetFiltrosMaterial(int idUsuario);

        // MRAF-01-08: material disponible para el estudiante, con filtro opcional por curso
        Task<RespuestaDTO<List<RecursoDTO>>> GetMaterialEstudiante(int idUsuario, int? idCurso);

        // MRAF-01-09: descarga con control de acceso (estudiante, docente o supervisor)
        Task<RespuestaDTO<RecursoDescargaDTO>> Descargar(int idUsuario, int idRecurso, bool esSupervisor);

        // MRAF-01-10: supervisión de administrador y director
        Task<RespuestaDTO<FiltrosSupervisionDTO>> GetFiltrosSupervision();

        Task<RespuestaDTO<List<RecursoDTO>>> GetRecursosSupervision(RecursoFiltroDTO filtro);

        Task<RespuestaDTO<byte[]>> ExportarReporte(RecursoFiltroDTO filtro);
    }
}