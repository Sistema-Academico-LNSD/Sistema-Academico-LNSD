namespace ProyectoLNSD_WApp.DAL.Repositories.Asistencia
{
    public interface IAsistenciaRepository
    {

        /// Matrículas ACTIVAS del período, sección y grupo de la clase que tienen matriculado el curso
        Task<List<Entities.Matricula>> GetMatriculasDeClase(int idPeriodo, int idSeccion, int idGrupo, int idCurso);

        /// Registros ya guardados de una clase en una fecha
        Task<List<Entities.Asistencia>> GetDeSesion(int idHorario, DateOnly fecha);

        /// Crea los registros que no existen y actualiza los que cambiaron, en un solo guardado.
        /// Devuelve cuántos se crearon y cuántos se modificaron.
        Task<(int Nuevos, int Modificados)> GuardarSesion(
            int idHorario, DateOnly fecha, IReadOnlyList<Entities.Asistencia> registros, int idUsuario, DateTime ahora); 

        /// Registros con su clase y su estudiante, más recientes primero
        Task<List<Entities.Asistencia>> Buscar(AsistenciaFiltro filtro, int limite);

        /// Conteo por estudiante y curso, calculado en la base de datos
        Task<List<ConteoAsistencia>> Contar(AsistenciaFiltro filtro);

        /// Matrículas con estudiante, sección y grupo
        Task<List<Entities.Matricula>> GetMatriculas(IEnumerable<int> idsMatriculas);

        Task<List<Entities.Curso>> GetCursos(IEnumerable<int> idsCursos);

        Task<Entities.AsistenciaCierre?> GetCierre(int idPeriodo);

        Task<bool> Cerrar(Entities.AsistenciaCierre cierre);
    }
}
