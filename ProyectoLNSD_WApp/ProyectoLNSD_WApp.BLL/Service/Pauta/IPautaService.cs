using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    /// Reglas del módulo de Pautas de actuación (MPPAF-01).
    /// Todos los métodos reciben el id y el rol de la persona en sesión; el servidor decide qué puede ver o cambiar.
    public interface IPautaService
    {
        // ---- Catálogo (MPPAF-01-01, 01-02) ----
        RespuestaDTO<List<PautaCatalogoDTO>> GetPautas();
        RespuestaDTO<List<PlantillaResumenDTO>> GetPlantillas();
        RespuestaDTO<PlantillaPautaDTO> GetPlantilla(int numero);

        // ---- Apoyo a la captura (MPPAF-01-04) ----
        Task<RespuestaDTO<List<EstudiantePautaBusquedaDTO>>> BuscarEstudiantes(string? texto);

        // ---- Expedientes (MPPAF-01-02, 01-06, 01-10, 01-14) ----
        Task<RespuestaDTO<ExpedienteDetalleDTO>> CrearExpediente(int idUsuario, string rol, ExpedienteCrearDTO dto);
        Task<RespuestaDTO<List<ExpedienteResumenDTO>>> GetExpedientes(int idUsuario, string rol, FiltroExpedientesDTO filtro);
        Task<RespuestaDTO<ExpedienteDetalleDTO>> GetExpediente(int idUsuario, string rol, int idExpediente);
        Task<RespuestaDTO<ExpedienteDetalleDTO>> ActualizarEstudiantes(int idUsuario, string rol, int idExpediente, List<EstudianteInvolucradoDTO> estudiantes);
        Task<RespuestaDTO<ExpedienteDetalleDTO>> CerrarExpediente(int idUsuario, string rol, CierreExpedienteDTO dto);

        // ---- Formularios (MPPAF-01-03, 01-05, 01-07, 01-11, 01-13) ----
        Task<RespuestaDTO<FormularioDetalleDTO>> AgregarFormulario(int idUsuario, string rol, AgregarFormularioDTO dto);
        Task<RespuestaDTO<FormularioDetalleDTO>> GetFormulario(int idUsuario, string rol, int idFormulario);
        Task<RespuestaDTO<FormularioDetalleDTO>> GuardarFormulario(int idUsuario, string rol, FormularioGuardarDTO dto);
        Task<RespuestaDTO<FormularioDetalleDTO>> ReabrirFormulario(int idUsuario, string rol, int idFormulario, string? motivo);
        Task<RespuestaDTO<bool>> EliminarFormularioBorrador(int idUsuario, string rol, int idFormulario);

        // ---- Descarga e impresión (MPPAF-01-09): la Parte 4 llama a este método ----
        Task<RespuestaDTO<bool>> RegistrarSalida(int idUsuario, string rol, int idExpediente, int? idFormulario, string accion);

        // ---- Bitácora (solo Administrador y Director) ----
        Task<RespuestaDTO<List<BitacoraPautaDTO>>> GetBitacora(int idUsuario, string rol, int? idExpediente, int? idUsuarioFiltro, string? accion, DateTime? desde, DateTime? hasta);
    }
}
