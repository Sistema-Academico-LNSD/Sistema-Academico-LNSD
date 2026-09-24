using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.LogAcceso;

namespace ProyectoLNSD_WApp.BLL.Service.Auditoria
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogAccesoRepository _logAccesoRepository;

        public AuditoriaService(ILogAccesoRepository logAccesoRepository)
        {
            _logAccesoRepository = logAccesoRepository;
        }

        public async Task RegistrarLogin(int? idUsuario, string correo, bool exitoso, string? mensaje)
        {
            var log = new DAL.Entities.LogAcceso
            {
                IdUsuario = idUsuario,
                Correo = correo,
                TipoEvento = "Login",
                Exitoso = exitoso,
                Mensaje = mensaje,
                Fecha = DateTime.UtcNow
            };

            try
            {
                await _logAccesoRepository.Registrar(log);
            }
            catch
            {

            }
        }

        public async Task RegistrarLogout(int idUsuario, string correo)
        {
            var log = new DAL.Entities.LogAcceso
            {
                IdUsuario = idUsuario,
                Correo = correo,
                TipoEvento = "Logout",
                Exitoso = true,
                Mensaje = null,
                Fecha = DateTime.UtcNow
            };

            try
            {
                await _logAccesoRepository.Registrar(log);
            }
            catch
            {

            }
        }

        public async Task<RespuestaDTO<List<LogAccesoDTO>>> GetHistorial(
            string? correo,
            DateTime? desde,
            DateTime? hasta,
            bool? soloFallidos)
        {
            var respuesta = new RespuestaDTO<List<LogAccesoDTO>>();

            var logs = await _logAccesoRepository.GetHistorial(correo, desde, hasta, soloFallidos);

            respuesta.Dato = logs
                .Select(l => new LogAccesoDTO
                {
                    IdLog = l.IdLog,
                    NombreUsuario = l.Usuario != null
                        ? $"{l.Usuario.Nombre} {l.Usuario.Apellido}"
                        : null,
                    Correo = l.Correo,
                    TipoEvento = l.TipoEvento,
                    Exitoso = l.Exitoso,
                    Mensaje = l.Mensaje,
                    Fecha = l.Fecha
                })
                .ToList();

            return respuesta;
        }
    }
}