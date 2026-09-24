namespace ProyectoLNSD_WApp.DAL.Repositories.UsuarioTokenReset
{
    public interface IUsuarioTokenResetRepository
    {
        Task<bool> CrearToken(Entities.UsuarioTokenReset token);
        Task<Entities.UsuarioTokenReset?> ObtenerTokenValidoPorHash(byte[] tokenHash);
        Task<bool> MarcarComoUsado(int idToken);
        Task<bool> InvalidarTokensActivos(int idUsuario);
    }
}