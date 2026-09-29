namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class LogAccesoDTO
    {
        public int IdLog { get; set; }

        public string? NombreUsuario { get; set; }

        public string Correo { get; set; } = string.Empty;

        public string TipoEvento { get; set; } = string.Empty;

        public bool Exitoso { get; set; }

        public string? Mensaje { get; set; }

        public DateTime Fecha { get; set; }
    }
}