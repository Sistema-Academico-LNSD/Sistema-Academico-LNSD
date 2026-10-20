using ProyectoLNSD_WApp.DAL.Entities;
using EstudianteEntity = global::ProyectoLNSD_WApp.DAL.Entities.Estudiante;
using PeriodoLectivoEntity = global::ProyectoLNSD_WApp.DAL.Entities.PeriodoLectivo;
using SeccionEntity = global::ProyectoLNSD_WApp.DAL.Entities.Seccion;
using CursoEntity = global::ProyectoLNSD_WApp.DAL.Entities.Curso;
using MatriculaEntity = global::ProyectoLNSD_WApp.DAL.Entities.Matricula;

namespace ProyectoLNSD_WApp.DAL.Repositories.Matricula;

public interface IMatriculaRepository
{
    Task<EstudianteEntity?> GetEstudiante(int id);
    Task<PeriodoLectivoEntity?> GetPeriodo(int id);
    Task<Grupo?> GetGrupo(int id);
    Task<SeccionEntity?> GetSeccion(int id);
    Task<bool> ExisteMatriculaPeriodo(int idEstudiante, int idPeriodo, int? excluirId = null);
    Task<int> ContarMatriculasActivasEnSeccion(int idSeccion, int idPeriodo, int? excluirId = null);
    Task<List<CursoEntity>> GetCursosValidosParaGrado(int idGrado, IEnumerable<int> idsCursos);
    Task<MatriculaEntity?> GetById(int id);
    Task<List<MatriculaEntity>> Buscar(int? idEstudiante, string? identificacion, int? idGrado, int? idGrupo, int? idSeccion, int? idPeriodo, string? estado, string? texto, int limite);
    Task<List<MatriculaEntity>> HistorialEstudiante(int idEstudiante);
    Task<List<MatriculaHistorial>> HistorialCambios(int idMatricula);
    Task<List<MatriculaObservacion>> Observaciones(int idMatricula);
    Task<List<MatriculaEntity>> MatriculasDeUsuario(int idUsuario, bool esEncargado);
    Task GuardarNueva(MatriculaEntity matricula, IEnumerable<int> idsCursos, MatriculaHistorial historial);
    Task GuardarCambios(MatriculaEntity matricula, IEnumerable<int>? idsCursos, MatriculaHistorial historial);
    Task AgregarObservacion(MatriculaObservacion observacion);
}
