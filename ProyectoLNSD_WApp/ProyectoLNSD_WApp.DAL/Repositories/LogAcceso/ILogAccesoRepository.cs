namespace ProyectoLNSD_WApp.DAL.Repositories.LogAcceso
{
    public interface ILogAccesoRepository
    {
        Task<bool> Registrar(Entities.LogAcceso log);

        Task<List<Entities.LogAcceso>> GetHistorial(
            string? correo,
            DateTime? desde,
            DateTime? hasta,
            bool? soloFallidos);
    }
}