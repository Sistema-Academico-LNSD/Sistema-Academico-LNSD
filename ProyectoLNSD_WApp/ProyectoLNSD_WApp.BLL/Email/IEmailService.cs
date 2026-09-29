namespace ProyectoLNSD_WApp.BLL.Email
{
    public interface IEmailService
    {
        Task EnviarCorreoRecuperacion(
            string correoDestino,
            string nombreDestino,
            string enlaceRecuperacion);
    }
}