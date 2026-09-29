namespace ProyectoLNSD_WApp.DAL.Repositories.Modulo
{
    public interface IModuloRepository
    {
        Task<List<Entities.Modulo>> GetModulos();
    }
}