using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Beca
{
    public interface IBecaRepository
    {
        Task<List<BecaRequisito>> GetRequisitos(bool soloActivos);

        Task<BecaRequisito?> GetRequisito(int idRequisito);

        Task<bool> ExisteNombreRequisito(string nombre, int? excluirIdRequisito = null);

        Task CrearRequisito(BecaRequisito requisito);

        Task ActualizarRequisito(BecaRequisito requisito);

        Task<Entities.Beca?> GetById(int idBeca);

        Task<Entities.Beca?> GetDeEstudiante(int idEstudiante, int idPeriodo);

        Task<List<BecaListado>> Buscar(BecaFiltro filtro, int limite);

        Task<string?> GetEstadoMatricula(int idEstudiante, int idPeriodo);

        Task<(string? Grado, string? Seccion)> GetUbicacion(int idEstudiante, int idPeriodo);

        Task Crear(Entities.Beca beca, IEnumerable<int> idsRequisitosCumplidos);

        Task Actualizar(Entities.Beca beca, IEnumerable<int>? idsRequisitosCumplidos);
    }
}
