using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.UsuarioTokenReset
{
    public class UsuarioTokenResetRepository : IUsuarioTokenResetRepository
    {
        private readonly AppDbContext _context;

        public UsuarioTokenResetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CrearToken(Entities.UsuarioTokenReset token)
        {
            if (token == null)
                return false;

            await _context.UsuariosTokensReset.AddAsync(token);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Entities.UsuarioTokenReset?> ObtenerTokenValidoPorHash(byte[] tokenHash)
        {
            var ahora = DateTime.UtcNow;

            return await _context.UsuariosTokensReset
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    !t.Usado &&
                    t.FechaExpiracion > ahora);
        }

        public async Task<bool> MarcarComoUsado(int idToken)
        {
            var token = await _context.UsuariosTokensReset.FindAsync(idToken);

            if (token == null)
                return false;

            token.Usado = true;

            _context.UsuariosTokensReset.Update(token);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> InvalidarTokensActivos(int idUsuario)
        {
            var tokensActivos = await _context.UsuariosTokensReset
                .Where(t => t.IdUsuario == idUsuario && !t.Usado)
                .ToListAsync();

            if (tokensActivos.Count == 0)
                return true;

            foreach (var token in tokensActivos)
            {
                token.Usado = true;
            }

            _context.UsuariosTokensReset.UpdateRange(tokensActivos);

            return await _context.SaveChangesAsync() >= 0;
        }
    }
}