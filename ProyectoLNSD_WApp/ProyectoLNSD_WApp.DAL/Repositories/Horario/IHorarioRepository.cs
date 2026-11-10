using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Horario
{
    public interface IHorarioRepository
    {
        // ---- Consultas ----

        Task<List<Entities.Horario>> Buscar(HorarioFiltro filtro);

        Task<Entities.Horario?> GetById(int idHorario);

        // ---- Conflictos ----
        // Solo cuentan los horarios ACTIVOS del mismo período y día cuyo rango se traslape.
        // Devuelven el horario que choca (para informarlo) o null si no hay conflicto.
        // excluirId es el horario que se está editando, para no compararlo contra sí mismo.

        Task<Entities.Horario?> BuscarConflictoDocente(
            int idPeriodo, int idDocente, string dia, TimeOnly inicio, TimeOnly fin, int? excluirId = null);

        Task<Entities.Horario?> BuscarConflictoGrupo(
            int idPeriodo, int idSeccion, int idGrupo, string dia, TimeOnly inicio, TimeOnly fin, int? excluirId = null);

        Task<Entities.Horario?> BuscarConflictoAula(
            int idPeriodo, string aula, string dia, TimeOnly inicio, TimeOnly fin, int? excluirId = null);

        // ---- Datos para validar y para llenar los formularios ----

        Task<Entities.Seccion?> GetSeccion(int idSeccion);

        Task<Entities.Grupo?> GetGrupo(int idGrupo);

        Task<bool> CursoImpartidoEnGrado(int idCurso, int idGrado);

        Task<List<Entities.Seccion>> GetSeccionesActivas();

        Task<List<Entities.Grupo>> GetGruposPorNivel(string nivel);

        Task<List<Entities.Curso>> GetCursosActivosDeGrado(int idGrado);

        Task<List<Entities.Docente>> GetDocentesDeCurso(int idCurso, int idPeriodo);

        Task<List<Entities.PeriodoLectivo>> GetPeriodos();

        // ---- Escritura ----

        Task<bool> Crear(Entities.Horario horario);

        Task<bool> Actualizar(Entities.Horario horario);

        Task<bool> CambiarEstado(int idHorario, bool estado);
    }
}
