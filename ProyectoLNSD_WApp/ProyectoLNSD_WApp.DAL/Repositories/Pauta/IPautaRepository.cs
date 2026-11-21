using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Pauta
{
    public interface IPautaRepository
    {
        // ---- Expedientes ----
        /// Crea el expediente con su código (PAU-AAAA-NNNN) y los estudiantes. Devuelve el id, o 0 si falló.
        Task<int> CrearExpediente(PautaExpediente expediente, List<PautaExpedienteEstudiante> estudiantes);
        Task<PautaExpediente?> GetExpediente(int idExpediente);
        Task<List<PautaExpediente>> GetExpedientes(PautaExpedienteFiltro filtro);
        Task<bool> ReemplazarEstudiantes(int idExpediente, List<PautaExpedienteEstudiante> estudiantes);
        /// Guarda estado y datos de cierre del expediente.
        Task<bool> ActualizarCierreExpediente(PautaExpediente expediente);

        // ---- Formularios ----
        Task<PautaFormulario?> GetFormulario(int idFormulario);
        Task<List<PautaFormulario>> GetFormularios(int idExpediente);
        Task<short> SiguienteSecuencia(int idExpediente, byte numeroFormulario);
        /// Devuelve el id nuevo, o 0 si falló (por ejemplo, secuencia repetida).
        Task<int> CrearFormulario(PautaFormulario formulario);
        Task<bool> ActualizarFormulario(int idFormulario, string datos, string estado, int idUsuario, DateTime? fechaFinalizacion);
        Task<bool> EliminarFormularioBorrador(int idFormulario);

        // ---- Bitácora ----
        Task RegistrarBitacora(PautaBitacora registro);
        Task<List<PautaBitacora>> GetBitacora(int? idExpediente, int? idUsuario, string? accion, DateTime? desde, DateTime? hasta, int limite = 200);
    }
}
