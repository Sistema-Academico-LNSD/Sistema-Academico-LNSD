namespace ProyectoLNSD_WApp.DAL.Repositories.Asistencia
{
    public interface IAsistenciaRepository
    {
        // ---- Lista de clase (MCAF-01-02) ----

        /// Matrículas ACTIVAS del período, sección y grupo de la clase que tienen matriculado el curso
        Task<List<Entities.Matricula>> GetMatriculasDeClase(int idPeriodo, int idSeccion, int idGrupo, int idCurso);

        /// Registros ya guardados de una clase en una fecha (para editarlos, MCAF-01-05)
        Task<List<Entities.Asistencia>> GetDeSesion(int idHorario, DateOnly fecha);

        // ---- Escritura (MCAF-01-01, 03, 04 y 05) ----

        /// Crea los registros que no existen y actualiza los que cambiaron, en un solo guardado.
        /// Devuelve cuántos se crearon y cuántos se modificaron.
        Task<(int Nuevos, int Modificados)> GuardarSesion(
            int idHorario, DateOnly fecha, IReadOnlyList<Entities.Asistencia> registros, int idUsuario, DateTime ahora);

        // ---- Consultas (MCAF-01-06 a 01-12) ----

        /// Registros con su clase y su estudiante, más recientes primero
        Task<List<Entities.Asistencia>> Buscar(AsistenciaFiltro filtro, int limite);

        /// Conteo por estudiante y curso, calculado en la base de datos
        Task<List<ConteoAsistencia>> Contar(AsistenciaFiltro filtro);

        /// Matrículas con estudiante, sección y grupo (para nombrar las filas de un resumen)
        Task<List<Entities.Matricula>> GetMatriculas(IEnumerable<int> idsMatriculas);

        Task<List<Entities.Curso>> GetCursos(IEnumerable<int> idsCursos);

        // ---- Cierre del período (MCAF-01-13) ----

        Task<Entities.AsistenciaCierre?> GetCierre(int idPeriodo);

        Task<bool> Cerrar(Entities.AsistenciaCierre cierre);
    }
}
