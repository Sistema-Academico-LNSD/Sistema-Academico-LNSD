namespace ProyectoLNSD_WApp.BLL.DTO.UsuarioActions
{
    public class ResultadoLoginDTO
    {
        public bool Autenticado { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public UsuarioDTO? Usuario { get; set; }
    }
}