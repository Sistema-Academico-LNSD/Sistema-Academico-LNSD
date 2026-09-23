namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public RolDTO? Rol { get; set; }
    }
}