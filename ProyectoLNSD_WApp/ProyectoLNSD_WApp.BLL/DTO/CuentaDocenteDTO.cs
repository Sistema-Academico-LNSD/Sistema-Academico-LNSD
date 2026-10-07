namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class CuentaDocenteDTO
    {
        public int IdDocente { get; set; }
        public string NombreDocente { get; set; } = string.Empty;
        public string CorreoDocente { get; set; } = string.Empty;
        public bool TieneCuenta { get; set; }
        public int? IdUsuario { get; set; }
        public string? CorreoCuenta { get; set; }
        public bool CuentaActiva { get; set; }
    }
}