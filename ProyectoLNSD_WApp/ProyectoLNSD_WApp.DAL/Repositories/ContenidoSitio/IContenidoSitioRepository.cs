namespace ProyectoLNSD_WApp.DAL.Repositories.ContenidoSitio
{
    public interface IContenidoSitioRepository
    {
        // Misión / Visión / Historia: una sola fila por tipo.
        Task<Entities.ContenidoSitio?> GetPorTipo(string tipo);
        Task<List<Entities.ContenidoSitio>> GetPorTipos(IEnumerable<string> tipos);
        // Contenido con varias filas por tipo (Bloque, luego Banner), ordenado por Orden.
        Task<List<Entities.ContenidoSitio>> GetListaPorTipo(string tipo);
        Task<Entities.ContenidoSitio?> GetPorId(int idContenido);
        Task<bool> CreateContenido(Entities.ContenidoSitio contenido);
        Task<bool> UpdateContenido(Entities.ContenidoSitio contenido);
        Task<bool> DeleteContenido(int idContenido);
    }
}
