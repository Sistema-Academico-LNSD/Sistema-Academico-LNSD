namespace ProyectoLNSD_WApp.BLL.Security
{
    /// <summary>
    /// Servicio encargado de generar y verificar el hash de contraseñas de usuario.
    /// Se abstrae detrás de una interfaz para no acoplar la capa BLL a una
    /// implementación concreta (PasswordHasher de ASP.NET Core, BCrypt, etc.).
    /// </summary>
    public interface IPasswordHashService
    {
        /// <summary>
        /// Genera el hash (con sal e iteraciones) de una contraseña en texto plano,
        /// listo para almacenarse en la columna password_hash (VARBINARY).
        /// </summary>
        byte[] HashPassword(string password);

        /// <summary>
        /// Verifica que una contraseña en texto plano corresponda al hash almacenado.
        /// </summary>
        /// <returns>true si la contraseña es correcta.</returns>
        bool VerifyPassword(byte[] hashAlmacenado, string password);
    }
}
