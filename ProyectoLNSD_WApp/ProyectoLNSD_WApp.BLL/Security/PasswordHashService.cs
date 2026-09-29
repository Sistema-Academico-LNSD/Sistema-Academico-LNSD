
using Microsoft.AspNetCore.Identity;

namespace ProyectoLNSD_WApp.BLL.Security
{
    public class PasswordHashService : IPasswordHashService
    {
        private readonly PasswordHasher<object> _hasher = new();

        public byte[] HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(
                    "La contraseña no puede estar vacía.",
                    nameof(password));

            string hashBase64 =
                _hasher.HashPassword(null!, password);

            return Convert.FromBase64String(hashBase64);
        }

        public bool VerifyPassword(
            byte[] hashAlmacenado,
            string password)
        {
            if (hashAlmacenado == null ||
                hashAlmacenado.Length == 0)
                return false;

            if (string.IsNullOrWhiteSpace(password))
                return false;

            string hashBase64 =
                Convert.ToBase64String(hashAlmacenado);

            PasswordVerificationResult resultado =
                _hasher.VerifyHashedPassword(
                    null!,
                    hashBase64,
                    password);

            return resultado == PasswordVerificationResult.Success
                || resultado == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
