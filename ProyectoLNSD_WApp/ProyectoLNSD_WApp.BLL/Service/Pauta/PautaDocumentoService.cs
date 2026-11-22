using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Entities;
using ProyectoLNSD_WApp.DAL.Repositories.Institucion;
using ProyectoLNSD_WApp.DAL.Repositories.Pauta;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    public class PautaDocumentoService : IPautaDocumentoService
    {
        private const string RolAdministrador = "Administrador";
        private const string RolDirector = "Director";

        private readonly IPautaRepository _pautaRepository;
        private readonly IInstitucionRepository _institucionRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public PautaDocumentoService(
            IPautaRepository pautaRepository,
            IInstitucionRepository institucionRepository,
            IUsuarioRepository usuarioRepository)
        {
            _pautaRepository = pautaRepository;
            _institucionRepository = institucionRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<RespuestaDTO<DocumentoPautaDTO>> GetFormulario(int idUsuario, string rol, int idFormulario)
        {
            var formulario = await _pautaRepository.GetFormulario(idFormulario);
            if (formulario == null)
                return RespuestaDTO<DocumentoPautaDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            var expediente = await CargarExpediente(idUsuario, rol, formulario.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<DocumentoPautaDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            var documento = await ArmarDocumento(idUsuario, expediente, new[] { formulario }, false);
            if (documento.Formularios.Count == 0)
                return RespuestaDTO<DocumentoPautaDTO>.Error("No se encontró la plantilla del formulario.", 7001);

            await Registrar(idUsuario, expediente.IdExpediente, idFormulario, $"Formulario N°{formulario.NumeroFormulario}");
            return RespuestaDTO<DocumentoPautaDTO>.Exito(documento);
        }

        public async Task<RespuestaDTO<DocumentoPautaDTO>> GetExpediente(int idUsuario, string rol, int idExpediente)
        {
            var expediente = await CargarExpediente(idUsuario, rol, idExpediente);
            if (expediente == null)
                return RespuestaDTO<DocumentoPautaDTO>.Error("El expediente no existe o no tiene acceso a él.", 7003);

            if (expediente.Formularios.Count == 0)
                return RespuestaDTO<DocumentoPautaDTO>.Error("El expediente todavía no tiene formularios para imprimir.", 7030);

            var documento = await ArmarDocumento(idUsuario, expediente, expediente.Formularios, true);

            await Registrar(idUsuario, expediente.IdExpediente, null, "Expediente completo");
            return RespuestaDTO<DocumentoPautaDTO>.Exito(documento);
        }

        // =====================  Auxiliares  =====================

        /// Un expediente solo se entrega a quien lo activó o a la dirección y administración.
        private async Task<PautaExpediente?> CargarExpediente(int idUsuario, string rol, int idExpediente)
        {
            var expediente = await _pautaRepository.GetExpediente(idExpediente);
            if (expediente == null) return null;

            bool veTodo = rol == RolAdministrador || rol == RolDirector;
            return veTodo || expediente.IdUsuarioResponsable == idUsuario ? expediente : null;
        }

        private async Task Registrar(int idUsuario, int idExpediente, int? idFormulario, string detalle)
        {
            await _pautaRepository.RegistrarBitacora(new PautaBitacora
            {
                IdExpediente = idExpediente,
                IdFormulario = idFormulario,
                IdUsuario = idUsuario,
                Accion = AccionesBitacoraPauta.Imprimir,
                Detalle = detalle.Length > 300 ? detalle[..300] : detalle
            });
        }

        private async Task<DocumentoPautaDTO> ArmarDocumento(
            int idUsuario, PautaExpediente expediente, IEnumerable<PautaFormulario> formularios, bool completo)
        {
            var institucion = await _institucionRepository.GetInstitucion();
            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            var pauta = PautasCatalogo.Obtener(expediente.NumeroPauta);

            var documento = new DocumentoPautaDTO
            {
                Institucion = new InstitucionDocumentoDTO
                {
                    Nombre = string.IsNullOrWhiteSpace(institucion?.Nombre) ? "Centro educativo" : institucion!.Nombre,
                    Direccion = institucion?.Direccion,
                    Telefono = institucion?.Telefono,
                    Correo = institucion?.Correo,
                    RutaLogo = RutaLogoSegura(institucion?.RutaLogo),
                    CodigoPresupuestario = institucion?.CodigoPresupuestario,
                    CircuitoEscolar = institucion?.CircuitoEscolar,
                    Dre = institucion?.Dre,
                    NombreDirector = institucion?.NombreDirector
                },
                CodigoExpediente = expediente.Codigo,
                NumeroPauta = expediente.NumeroPauta,
                NombrePauta = pauta?.Nombre ?? string.Empty,
                EstadoExpediente = expediente.Estado,
                FechaActivacion = expediente.FechaActivacion,
                Responsable = $"{expediente.UsuarioResponsable?.Nombre} {expediente.UsuarioResponsable?.Apellido}".Trim(),
                GeneradoPor = $"{usuario?.Nombre} {usuario?.Apellido}".Trim(),
                FechaGeneracion = DateTime.Now,
                EsExpedienteCompleto = completo
            };

            foreach (var f in formularios.OrderBy(x => x.NumeroFormulario).ThenBy(x => x.Secuencia))
            {
                var plantilla = PlantillasPauta.Obtener(f.NumeroFormulario);
                if (plantilla == null) continue;

                documento.Formularios.Add(new FormularioDocumentoDTO
                {
                    IdFormulario = f.IdFormulario,
                    NumeroFormulario = f.NumeroFormulario,
                    Secuencia = f.Secuencia,
                    Estado = f.Estado,
                    Plantilla = plantilla,
                    Datos = f.Datos
                });
            }

            return documento;
        }

        /// El logo solo se acepta si es un archivo subido por el sistema (/uploads/...).
        private static string? RutaLogoSegura(string? ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return null;
            if (!ruta.StartsWith("/uploads/", StringComparison.Ordinal)) return null;
            if (ruta.Contains("..") || ruta.Contains('"') || ruta.Contains('\'') || ruta.Contains('<') || ruta.Contains('>')) return null;
            return ruta;
        }
    }
}
