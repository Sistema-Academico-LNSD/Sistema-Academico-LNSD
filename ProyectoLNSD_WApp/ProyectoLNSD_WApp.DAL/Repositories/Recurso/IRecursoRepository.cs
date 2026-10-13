using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Recurso
{
    public interface IRecursoRepository
    {
        // Consulta única para docente, estudiante y administrador: cada uno la usa con sus filtros
        Task<List<Entities.RecursoAcademico>> Buscar(
            int? idDocente,
            int? idCurso,
            int? idGrado,
            string? titulo,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string? orden);

        Task<Entities.RecursoAcademico?> GetById(int idRecurso);

        Task<bool> ExisteHash(int idCurso, string hash, int? excluirIdRecurso = null);

        Task<bool> ExisteUrl(int idCurso, string url, int? excluirIdRecurso = null);

        // ¿El curso se imparte en ese grado? (tabla Curso_Grado)
        Task<bool> CursoImpartidoEnGrado(int idCurso, int idGrado);

        // Para llenar la lista de grados al elegir un curso
        Task<List<Entities.Grado>> GetGradosActivosDeCurso(int idCurso);

        Task<bool> Crear(Entities.RecursoAcademico recurso);

        Task<bool> Actualizar(Entities.RecursoAcademico recurso);

        Task<bool> Eliminar(int idRecurso);

        // Cursos activos que se imparten en un grado (filtro del estudiante)
        Task<List<Entities.Curso>> GetCursosActivosDeGrado(int idGrado);

        // Opciones de los filtros de supervisión: solo docentes y cursos que tienen recursos
        Task<List<(int Id, string Nombre)>> GetDocentesConRecursos();

        Task<List<(int Id, string Nombre)>> GetCursosConRecursos();
    }
}
