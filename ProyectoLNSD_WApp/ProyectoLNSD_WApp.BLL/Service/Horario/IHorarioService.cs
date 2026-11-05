using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Horario
{
    public interface IHorarioService
    {
        // Período activo, secciones activas y días lectivos (para arrancar el formulario)
        Task<RespuestaDTO<OpcionesHorarioDTO>> GetOpciones();

        // Listas que dependen de lo elegido en el formulario
        Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetGrupos(int idSeccion);

        Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetCursos(int idSeccion);

        Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetDocentes(int idCurso);

        // MHOF-01-01, esc. 5: consulta de la programación
        Task<RespuestaDTO<List<HorarioDTO>>> GetHorarios(HorarioFiltroDTO filtro);

        // MHOF-01-01 a 01-04
        Task<RespuestaDTO<HorarioDTO>> Registrar(HorarioGuardarDTO dto);
        // MHOF-01-12: detalle de un horario
        Task<RespuestaDTO<HorarioDTO>> GetHorario(int idHorario);

        // MHOF-01-11: períodos para el filtro de búsqueda
        Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetPeriodos();

        // MHOF-01-05: modificar
        Task<RespuestaDTO<HorarioDTO>> Actualizar(HorarioGuardarDTO dto);

        // MHOF-01-06: activar o inactivar
        Task<RespuestaDTO<HorarioDTO>> CambiarEstado(int idHorario, bool activar);
        // MHOF-01-10: horario personal del docente
        Task<RespuestaDTO<List<HorarioDTO>>> GetMiHorarioDocente(int idUsuario, int? idPeriodo);

        // MHOF-01-07: horarios de un docente, para quien administra
        Task<RespuestaDTO<List<HorarioDTO>>> GetHorariosDeDocente(int idDocente, int? idPeriodo);

        Task<RespuestaDTO<List<OpcionFiltroDTO>>> GetDocentesConsulta();

        // MHOF-01-08 y 01-09: horario de las matrículas vigentes del estudiante o del encargado
        Task<RespuestaDTO<List<HorarioMatriculaDTO>>> GetHorariosDeMatriculas(int idUsuario, bool esEncargado);
    }
}