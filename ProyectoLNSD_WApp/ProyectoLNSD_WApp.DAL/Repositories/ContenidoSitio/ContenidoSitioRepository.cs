using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.ContenidoSitio
{
    public class ContenidoSitioRepository : IContenidoSitioRepository
    {
        private readonly AppDbContext _context;

        public ContenidoSitioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Entities.ContenidoSitio?> GetPorTipo(string tipo)
        {
            return await _context.ContenidosSitio
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Tipo == tipo);
        }

        public async Task<List<Entities.ContenidoSitio>> GetPorTipos(IEnumerable<string> tipos)
        {
            var lista = tipos.ToList();

            return await _context.ContenidosSitio
                .AsNoTracking()
                .Where(c => lista.Contains(c.Tipo))
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.IdContenido)
                .ToListAsync();
        }

        public async Task<List<Entities.ContenidoSitio>> GetListaPorTipo(string tipo)
        {
            return await _context.ContenidosSitio
                .AsNoTracking()
                .Where(c => c.Tipo == tipo)
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.IdContenido)
                .ToListAsync();
        }

        public async Task<Entities.ContenidoSitio?> GetPorId(int idContenido)
        {
            return await _context.ContenidosSitio
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdContenido == idContenido);
        }

        public async Task<bool> CreateContenido(Entities.ContenidoSitio contenido)
        {
            if (contenido == null)
                return false;

            contenido.FechaActualizacion = DateTime.UtcNow;
            await _context.ContenidosSitio.AddAsync(contenido);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateContenido(Entities.ContenidoSitio contenido)
        {
            if (contenido == null)
                return false;

            var existing = await _context.ContenidosSitio.FindAsync(contenido.IdContenido);
            if (existing == null)
                return false;

            existing.Titulo = contenido.Titulo;
            existing.Descripcion = contenido.Descripcion;
            existing.RutaImagen = contenido.RutaImagen;
            existing.Orden = contenido.Orden;
            existing.Estado = contenido.Estado;
            existing.FechaPublicacion = contenido.FechaPublicacion;
            existing.IdUsuarioModifica = contenido.IdUsuarioModifica;
            existing.FechaActualizacion = DateTime.UtcNow;   // siempre cambia, así SaveChanges devuelve > 0

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteContenido(int idContenido)
        {
            var existing = await _context.ContenidosSitio.FindAsync(idContenido);
            if (existing == null)
                return false;

            _context.ContenidosSitio.Remove(existing);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
