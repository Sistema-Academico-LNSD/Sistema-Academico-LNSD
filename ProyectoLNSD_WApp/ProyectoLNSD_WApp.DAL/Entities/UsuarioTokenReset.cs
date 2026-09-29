namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class UsuarioTokenReset
    {
        public int IdToken { get; set; }

        public int IdUsuario { get; set; }

        public byte[] TokenHash { get; set; } = Array.Empty<byte>();

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; }

        public virtual Usuario? Usuario { get; set; }
    }
}