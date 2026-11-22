using System.Text.Json;
using System.Text.Json.Nodes;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Entities;
using ProyectoLNSD_WApp.DAL.Repositories.Estudiante;
using ProyectoLNSD_WApp.DAL.Repositories.Institucion;
using ProyectoLNSD_WApp.DAL.Repositories.Pauta;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    public class PautaService : IPautaService
    {
        private const string RolAdministrador = "Administrador";
        private const string RolDirector = "Director";
        private const string MensajeSinAcceso = "El expediente no existe o no tiene acceso a él.";

        // Formularios que se llenan para una sola persona estudiante.
        private static readonly int[] FormulariosPorEstudiante = { 3, 6, 7, 8, 9, 10 };

        private readonly IPautaRepository _pautaRepository;
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IInstitucionRepository _institucionRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public PautaService(
            IPautaRepository pautaRepository,
            IEstudianteRepository estudianteRepository,
            IInstitucionRepository institucionRepository,
            IUsuarioRepository usuarioRepository)
        {
            _pautaRepository = pautaRepository;
            _estudianteRepository = estudianteRepository;
            _institucionRepository = institucionRepository;
            _usuarioRepository = usuarioRepository;
        }

        // =====================  Catálogo  =====================

        public RespuestaDTO<List<PautaCatalogoDTO>> GetPautas() =>
            RespuestaDTO<List<PautaCatalogoDTO>>.Exito(PautasCatalogo.Pautas.ToList());

        public RespuestaDTO<List<PlantillaResumenDTO>> GetPlantillas()
        {
            var lista = PlantillasPauta.Todas.Select(p => new PlantillaResumenDTO
            {
                Numero = p.Numero,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Repetible = p.Repetible,
                Pautas = PautasCatalogo.Pautas
                    .Where(x => x.Requeridos.Contains(p.Numero) || x.Sugeridos.Contains(p.Numero))
                    .Select(x => x.Numero)
                    .ToList()
            }).ToList();

            return RespuestaDTO<List<PlantillaResumenDTO>>.Exito(lista);
        }

        public RespuestaDTO<PlantillaPautaDTO> GetPlantilla(int numero)
        {
            var plantilla = PlantillasPauta.Obtener(numero);
            return plantilla == null
                ? RespuestaDTO<PlantillaPautaDTO>.Error("El formulario solicitado no existe.", 7001)
                : RespuestaDTO<PlantillaPautaDTO>.Exito(plantilla);
        }

        // =====================  Búsqueda de estudiantes  =====================

        public async Task<RespuestaDTO<List<EstudiantePautaBusquedaDTO>>> BuscarEstudiantes(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 2)
                return RespuestaDTO<List<EstudiantePautaBusquedaDTO>>.Exito(new List<EstudiantePautaBusquedaDTO>());

            var encontrados = await _estudianteRepository.Buscar(texto.Trim(), null, true);

            var lista = encontrados.Take(20).Select(e => new EstudiantePautaBusquedaDTO
            {
                IdEstudiante = e.IdEstudiante,
                NombreCompleto = NombreCompleto(e.Usuario?.Nombre, e.Usuario?.Apellido),
                Identificacion = e.Identificacion,
                Nivel = e.Grado?.Nombre,
                FechaNacimiento = e.FechaNacimiento,
                Edad = CalcularEdad(e.FechaNacimiento)
            }).ToList();

            return RespuestaDTO<List<EstudiantePautaBusquedaDTO>>.Exito(lista);
        }

        // =====================  Expedientes  =====================

        public async Task<RespuestaDTO<ExpedienteDetalleDTO>> CrearExpediente(int idUsuario, string rol, ExpedienteCrearDTO dto)
        {
            var pauta = PautasCatalogo.Obtener(dto.NumeroPauta);
            if (pauta == null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("La pauta seleccionada no es válida.", 7001);

            var (estudiantes, error) = await ValidarEstudiantes(dto.Estudiantes, pauta);
            if (error != null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(error, 7002);

            if (dto.IdExpedienteOrigen.HasValue)
            {
                var origen = await CargarExpediente(idUsuario, rol, dto.IdExpedienteOrigen.Value);
                if (origen == null)
                    return RespuestaDTO<ExpedienteDetalleDTO>.Error(MensajeSinAcceso, 7003);
            }

            var nuevo = new PautaExpediente
            {
                NumeroPauta = (byte)pauta.Numero,
                IdUsuarioResponsable = idUsuario,
                FechaActivacion = DateOnly.FromDateTime(DateTime.Now),
                IdExpedienteOrigen = dto.IdExpedienteOrigen
            };

            int idExpediente = await _pautaRepository.CrearExpediente(nuevo, estudiantes);
            if (idExpediente == 0)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("No se pudo crear el expediente. Intente nuevamente.", 7090);

            var expediente = await _pautaRepository.GetExpediente(idExpediente);
            if (expediente == null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("No se pudo crear el expediente. Intente nuevamente.", 7090);

            // El formulario de inicio (N°1 o N°2) nace con los datos del centro, de la persona usuaria y de las personas estudiantes.
            int idFormulario = await CrearFormularioInterno(idUsuario, rol, expediente, pauta.FormularioInicial, null);

            await Registrar(idUsuario, idExpediente, null, AccionesBitacoraPauta.Crear,
                $"Expediente {expediente.Codigo} (pauta {pauta.Numero})");

            if (idFormulario > 0)
                await Registrar(idUsuario, idExpediente, idFormulario, AccionesBitacoraPauta.Crear,
                    $"Formulario N°{pauta.FormularioInicial}");

            expediente = await _pautaRepository.GetExpediente(idExpediente);
            return RespuestaDTO<ExpedienteDetalleDTO>.Exito(await MapDetalle(expediente!), "Expediente creado correctamente.");
        }

        public async Task<RespuestaDTO<List<ExpedienteResumenDTO>>> GetExpedientes(int idUsuario, string rol, FiltroExpedientesDTO filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Estado)
                && filtro.Estado != EstadosExpedientePauta.Abierto && filtro.Estado != EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<List<ExpedienteResumenDTO>>.Error("El estado indicado no es válido.", 7002);

            var filtroDatos = new PautaExpedienteFiltro
            {
                // Quien no es Administrador ni Director solo ve sus propios expedientes.
                IdUsuarioResponsable = VeTodo(rol) ? null : idUsuario,
                Estado = string.IsNullOrWhiteSpace(filtro.Estado) ? null : filtro.Estado,
                NumeroPauta = filtro.NumeroPauta is >= 1 and <= 9 ? (byte)filtro.NumeroPauta.Value : null,
                Desde = filtro.Desde.HasValue ? DateOnly.FromDateTime(filtro.Desde.Value) : null,
                Hasta = filtro.Hasta.HasValue ? DateOnly.FromDateTime(filtro.Hasta.Value) : null,
                TextoEstudiante = string.IsNullOrWhiteSpace(filtro.TextoEstudiante) ? null : filtro.TextoEstudiante,
                IdEstudiante = filtro.IdEstudiante
            };

            var expedientes = await _pautaRepository.GetExpedientes(filtroDatos);
            return RespuestaDTO<List<ExpedienteResumenDTO>>.Exito(expedientes.Select(MapResumen).ToList());
        }

        public async Task<RespuestaDTO<ExpedienteDetalleDTO>> GetExpediente(int idUsuario, string rol, int idExpediente)
        {
            var expediente = await CargarExpediente(idUsuario, rol, idExpediente);
            if (expediente == null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(MensajeSinAcceso, 7003);

            await Registrar(idUsuario, idExpediente, null, AccionesBitacoraPauta.Ver, null);
            return RespuestaDTO<ExpedienteDetalleDTO>.Exito(await MapDetalle(expediente));
        }

        public async Task<RespuestaDTO<ExpedienteDetalleDTO>> ActualizarEstudiantes(int idUsuario, string rol, int idExpediente, List<EstudianteInvolucradoDTO> estudiantes)
        {
            var expediente = await CargarExpediente(idUsuario, rol, idExpediente);
            if (expediente == null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(MensajeSinAcceso, 7003);

            if (expediente.Estado == EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("El expediente está cerrado y no se puede modificar.", 7011);

            var pauta = PautasCatalogo.Obtener(expediente.NumeroPauta)!;
            var (lista, error) = await ValidarEstudiantes(estudiantes, pauta);
            if (error != null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(error, 7002);

            if (!await _pautaRepository.ReemplazarEstudiantes(idExpediente, lista))
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("No se pudieron guardar las personas estudiantes.", 7090);

            await Registrar(idUsuario, idExpediente, null, AccionesBitacoraPauta.Editar,
                $"Personas estudiantes involucradas: {lista.Count}");

            expediente = await _pautaRepository.GetExpediente(idExpediente);
            return RespuestaDTO<ExpedienteDetalleDTO>.Exito(await MapDetalle(expediente!), "Personas estudiantes guardadas correctamente.");
        }

        public async Task<RespuestaDTO<ExpedienteDetalleDTO>> CerrarExpediente(int idUsuario, string rol, CierreExpedienteDTO dto)
        {
            var expediente = await CargarExpediente(idUsuario, rol, dto.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(MensajeSinAcceso, 7003);

            if (expediente.Estado == EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("El expediente ya está cerrado.", 7011);

            var pendientes = CalcularPendientesCierre(expediente);
            if (pendientes.Count > 0)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(
                    "No se puede cerrar el expediente: " + string.Join(" ", pendientes), 7020);

            bool sinIdentificacion = InformeActuacionSinIdentificacion(expediente);
            if (!sinIdentificacion && !dto.DevolucionRealizada)
                return RespuestaDTO<ExpedienteDetalleDTO>.Error(
                    "Antes de cerrar debe realizar la devolución de las acciones a las personas encargadas legales y registrarla en un acta firmada.", 7021);

            string? vistoBueno = dto.VistoBuenoDirector?.Trim();
            if (string.IsNullOrWhiteSpace(vistoBueno))
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("El visto bueno de la persona directora es requerido para el cierre.", 7022);

            DateTime fechaCierre = dto.FechaCierre ?? DateTime.Now;
            if (fechaCierre > DateTime.Now.AddDays(1))
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("La fecha de cierre no puede estar en el futuro.", 7023);

            string nombreCierre = string.IsNullOrWhiteSpace(dto.NombreCierre)
                ? (await ArmarPrecarga(idUsuario, rol, expediente)).GetValueOrDefault("usuario_nombre", string.Empty)
                : dto.NombreCierre.Trim();

            var cierre = new PautaExpediente
            {
                IdExpediente = expediente.IdExpediente,
                Estado = EstadosExpedientePauta.Cerrado,
                FechaCierre = fechaCierre,
                NombreCierre = nombreCierre,
                VistoBuenoDirector = vistoBueno,
                DevolucionRealizada = dto.DevolucionRealizada,
                ObservacionCierre = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim()
            };

            if (!await _pautaRepository.ActualizarCierreExpediente(cierre))
                return RespuestaDTO<ExpedienteDetalleDTO>.Error("No se pudo cerrar el expediente.", 7090);

            await Registrar(idUsuario, expediente.IdExpediente, null, AccionesBitacoraPauta.Cerrar, $"Expediente {expediente.Codigo}");

            expediente = await _pautaRepository.GetExpediente(expediente.IdExpediente);
            return RespuestaDTO<ExpedienteDetalleDTO>.Exito(await MapDetalle(expediente!), "Expediente cerrado correctamente.");
        }

        // =====================  Formularios  =====================

        public async Task<RespuestaDTO<FormularioDetalleDTO>> AgregarFormulario(int idUsuario, string rol, AgregarFormularioDTO dto)
        {
            var expediente = await CargarExpediente(idUsuario, rol, dto.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error(MensajeSinAcceso, 7003);

            if (expediente.Estado == EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El expediente está cerrado y no admite más formularios.", 7011);

            var plantilla = PlantillasPauta.Obtener(dto.NumeroFormulario);
            if (plantilla == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario seleccionado no es válido.", 7001);

            var pauta = PautasCatalogo.Obtener(expediente.NumeroPauta)!;
            bool delProcedimiento = pauta.Requeridos.Contains(plantilla.Numero) || pauta.Sugeridos.Contains(plantilla.Numero);
            string? justificacion = dto.Justificacion?.Trim();

            if (!delProcedimiento && (justificacion == null || justificacion.Length < 10))
                return RespuestaDTO<FormularioDetalleDTO>.Error(
                    $"El formulario N°{plantilla.Numero} no está indicado en el procedimiento de esta pauta. Escriba una justificación de al menos 10 caracteres.", 7014);

            bool yaExiste = expediente.Formularios.Any(f => f.NumeroFormulario == plantilla.Numero);
            if (yaExiste && !plantilla.Repetible)
                return RespuestaDTO<FormularioDetalleDTO>.Error(
                    $"El formulario N°{plantilla.Numero} ya existe en este expediente. Ábralo para completarlo.", 7013);

            PautaExpedienteEstudiante? involucrado = null;
            if (dto.IdPautaEstudiante.HasValue)
            {
                involucrado = expediente.Estudiantes.FirstOrDefault(e => e.IdPautaEstudiante == dto.IdPautaEstudiante.Value);
                if (involucrado == null)
                    return RespuestaDTO<FormularioDetalleDTO>.Error("La persona estudiante no pertenece a este expediente.", 7002);
            }
            else if (FormulariosPorEstudiante.Contains(plantilla.Numero) && expediente.Estudiantes.Count == 1)
            {
                involucrado = expediente.Estudiantes.First();
            }

            int idFormulario = await CrearFormularioInterno(idUsuario, rol, expediente, plantilla.Numero, involucrado);
            if (idFormulario == 0)
                return RespuestaDTO<FormularioDetalleDTO>.Error("No se pudo crear el formulario. Intente nuevamente.", 7090);

            string detalle = $"Formulario N°{plantilla.Numero}" + (delProcedimiento ? string.Empty : $" | Justificación: {justificacion}");
            await Registrar(idUsuario, expediente.IdExpediente, idFormulario, AccionesBitacoraPauta.Crear, detalle);

            return await DetalleFormulario(idUsuario, rol, idFormulario, "Formulario agregado al expediente.");
        }

        public async Task<RespuestaDTO<FormularioDetalleDTO>> GetFormulario(int idUsuario, string rol, int idFormulario)
        {
            var formulario = await _pautaRepository.GetFormulario(idFormulario);
            if (formulario == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            var expediente = await CargarExpediente(idUsuario, rol, formulario.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            await Registrar(idUsuario, expediente.IdExpediente, idFormulario, AccionesBitacoraPauta.Ver, $"Formulario N°{formulario.NumeroFormulario}");
            return RespuestaDTO<FormularioDetalleDTO>.Exito(MapFormulario(expediente, formulario, rol));
        }

        public async Task<RespuestaDTO<FormularioDetalleDTO>> GuardarFormulario(int idUsuario, string rol, FormularioGuardarDTO dto)
        {
            var formulario = await _pautaRepository.GetFormulario(dto.IdFormulario);
            if (formulario == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            var expediente = await CargarExpediente(idUsuario, rol, formulario.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            if (expediente.Estado == EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El expediente está cerrado y no se puede modificar.", 7011);

            if (formulario.Estado == EstadosFormularioPauta.Finalizado)
                return RespuestaDTO<FormularioDetalleDTO>.Error(
                    "Un formulario finalizado no se puede modificar. Solicite a la dirección que lo reabra.", 7011);

            var plantilla = PlantillasPauta.Obtener(formulario.NumeroFormulario)!;

            var (json, errores) = FormularioPautaHelper.Sanitizar(plantilla, dto.Datos);
            if (errores.Count > 0)
                return RespuestaDTO<FormularioDetalleDTO>.Error(ResumirErrores("Hay datos que corregir:", errores), 7010);

            bool hayDatos = json != "{}";
            string estado = hayDatos ? EstadosFormularioPauta.EnElaboracion : formulario.Estado;
            DateTime? fechaFinalizacion = null;

            List<string> faltantes = new();
            if (dto.Finalizar)
            {
                faltantes = FormularioPautaHelper.Validar(plantilla, json);
                if (faltantes.Count == 0)
                {
                    estado = EstadosFormularioPauta.Finalizado;
                    fechaFinalizacion = DateTime.Now;
                }
            }

            if (!await _pautaRepository.ActualizarFormulario(formulario.IdFormulario, json, estado, idUsuario, fechaFinalizacion))
                return RespuestaDTO<FormularioDetalleDTO>.Error("No se pudo guardar el formulario.", 7090);

            string accion = estado == EstadosFormularioPauta.Finalizado ? AccionesBitacoraPauta.Finalizar : AccionesBitacoraPauta.Editar;
            await Registrar(idUsuario, expediente.IdExpediente, formulario.IdFormulario, accion, $"Formulario N°{formulario.NumeroFormulario}");

            if (faltantes.Count > 0)
            {
                // Se guardó lo escrito, pero no se finaliza hasta completar lo obligatorio.
                var guardado = await DetalleFormulario(idUsuario, rol, formulario.IdFormulario, null, false);
                guardado.EsCorrecto = false;
                guardado.Codigo = 7012;
                guardado.Mensaje = ResumirErrores("Se guardó el borrador, pero faltan datos obligatorios para finalizar:", faltantes);
                return guardado;
            }

            return await DetalleFormulario(idUsuario, rol, formulario.IdFormulario,
                dto.Finalizar ? "Formulario finalizado correctamente." : "Borrador guardado correctamente.", false);
        }

        public async Task<RespuestaDTO<FormularioDetalleDTO>> ReabrirFormulario(int idUsuario, string rol, int idFormulario, string? motivo)
        {
            if (!VeTodo(rol))
                return RespuestaDTO<FormularioDetalleDTO>.Error("Solo la dirección o el administrador pueden reabrir un formulario.", 7003);

            string? texto = motivo?.Trim();
            if (texto == null || texto.Length < 10)
                return RespuestaDTO<FormularioDetalleDTO>.Error("Indique el motivo de la reapertura (mínimo 10 caracteres).", 7002);

            var formulario = await _pautaRepository.GetFormulario(idFormulario);
            if (formulario == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe.", 7003);

            var expediente = await CargarExpediente(idUsuario, rol, formulario.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe.", 7003);

            if (expediente.Estado == EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El expediente está cerrado y no se puede modificar.", 7011);

            if (formulario.Estado != EstadosFormularioPauta.Finalizado)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no está finalizado.", 7011);

            if (!await _pautaRepository.ActualizarFormulario(idFormulario, formulario.Datos, EstadosFormularioPauta.EnElaboracion, idUsuario, null))
                return RespuestaDTO<FormularioDetalleDTO>.Error("No se pudo reabrir el formulario.", 7090);

            await Registrar(idUsuario, expediente.IdExpediente, idFormulario, AccionesBitacoraPauta.Reabrir,
                $"Formulario N°{formulario.NumeroFormulario} | Motivo: {texto}");

            return await DetalleFormulario(idUsuario, rol, idFormulario, "Formulario reabierto para su edición.", false);
        }

        public async Task<RespuestaDTO<bool>> EliminarFormularioBorrador(int idUsuario, string rol, int idFormulario)
        {
            var formulario = await _pautaRepository.GetFormulario(idFormulario);
            if (formulario == null)
                return RespuestaDTO<bool>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            var expediente = await CargarExpediente(idUsuario, rol, formulario.IdExpediente);
            if (expediente == null)
                return RespuestaDTO<bool>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            if (expediente.Estado == EstadosExpedientePauta.Cerrado)
                return RespuestaDTO<bool>.Error("El expediente está cerrado y no se puede modificar.", 7011);

            if (formulario.Estado != EstadosFormularioPauta.Borrador)
                return RespuestaDTO<bool>.Error("Solo se pueden quitar formularios que todavía no tienen datos guardados.", 7011);

            var pauta = PautasCatalogo.Obtener(expediente.NumeroPauta)!;
            int mismos = expediente.Formularios.Count(f => f.NumeroFormulario == formulario.NumeroFormulario);
            if (pauta.Requeridos.Contains(formulario.NumeroFormulario) && mismos <= 1)
                return RespuestaDTO<bool>.Error($"El formulario N°{formulario.NumeroFormulario} es requerido en esta pauta y no se puede quitar.", 7011);

            if (!await _pautaRepository.EliminarFormularioBorrador(idFormulario))
                return RespuestaDTO<bool>.Error("No se pudo quitar el formulario.", 7090);

            await Registrar(idUsuario, expediente.IdExpediente, null, AccionesBitacoraPauta.Eliminar,
                $"Se quitó el borrador del formulario N°{formulario.NumeroFormulario}");

            return RespuestaDTO<bool>.Exito(true, "Formulario quitado del expediente.");
        }

        // =====================  Descarga e impresión  =====================

        public async Task<RespuestaDTO<bool>> RegistrarSalida(int idUsuario, string rol, int idExpediente, int? idFormulario, string accion)
        {
            if (accion != AccionesBitacoraPauta.Descargar && accion != AccionesBitacoraPauta.Imprimir)
                return RespuestaDTO<bool>.Error("La acción indicada no es válida.", 7002);

            var expediente = await CargarExpediente(idUsuario, rol, idExpediente);
            if (expediente == null)
                return RespuestaDTO<bool>.Error(MensajeSinAcceso, 7003);

            string detalle = idFormulario.HasValue
                ? $"Formulario #{idFormulario.Value}"
                : "Expediente completo";

            await Registrar(idUsuario, idExpediente, idFormulario, accion, detalle);
            return RespuestaDTO<bool>.Exito(true);
        }

        // =====================  Bitácora  =====================

        public async Task<RespuestaDTO<List<BitacoraPautaDTO>>> GetBitacora(
            int idUsuario, string rol, int? idExpediente, int? idUsuarioFiltro, string? accion, DateTime? desde, DateTime? hasta)
        {
            if (!VeTodo(rol))
                return RespuestaDTO<List<BitacoraPautaDTO>>.Error("No tiene permiso para consultar la bitácora.", 7003);

            var registros = await _pautaRepository.GetBitacora(
                idExpediente, idUsuarioFiltro, string.IsNullOrWhiteSpace(accion) ? null : accion, desde, hasta?.Date.AddDays(1).AddTicks(-1));

            var lista = registros.Select(b => new BitacoraPautaDTO
            {
                Fecha = b.Fecha,
                Usuario = NombreCompleto(b.Usuario?.Nombre, b.Usuario?.Apellido),
                Accion = b.Accion,
                Detalle = b.Detalle,
                CodigoExpediente = b.Expediente?.Codigo ?? string.Empty,
                IdFormulario = b.IdFormulario
            }).ToList();

            return RespuestaDTO<List<BitacoraPautaDTO>>.Exito(lista);
        }

        // =====================  Auxiliares  =====================

        private static bool VeTodo(string rol) => rol == RolAdministrador || rol == RolDirector;

        private static string NombreCompleto(string? nombre, string? apellido) =>
            $"{nombre} {apellido}".Trim();

        private static int? CalcularEdad(DateOnly? nacimiento)
        {
            if (!nacimiento.HasValue) return null;

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            int edad = hoy.Year - nacimiento.Value.Year;
            if (hoy < nacimiento.Value.AddYears(edad)) edad--;
            return edad < 0 ? null : edad;
        }

        private static string ResumirErrores(string titulo, List<string> errores)
        {
            var mostrar = errores.Take(8).Select(e => "• " + e);
            string texto = titulo + " " + string.Join(" ", mostrar);
            if (errores.Count > 8) texto += $" …y {errores.Count - 8} más.";
            return texto;
        }

        /// Un expediente solo se entrega a quien lo activó o a la dirección y administración.
        private async Task<PautaExpediente?> CargarExpediente(int idUsuario, string rol, int idExpediente)
        {
            var expediente = await _pautaRepository.GetExpediente(idExpediente);
            if (expediente == null) return null;

            return VeTodo(rol) || expediente.IdUsuarioResponsable == idUsuario ? expediente : null;
        }

        private async Task Registrar(int idUsuario, int idExpediente, int? idFormulario, string accion, string? detalle)
        {
            if (detalle != null && detalle.Length > 300)
                detalle = detalle[..300];

            await _pautaRepository.RegistrarBitacora(new PautaBitacora
            {
                IdExpediente = idExpediente,
                IdFormulario = idFormulario,
                IdUsuario = idUsuario,
                Accion = accion,
                Detalle = detalle
            });
        }

        private async Task<(List<PautaExpedienteEstudiante> Lista, string? Error)> ValidarEstudiantes(
            List<EstudianteInvolucradoDTO>? estudiantes, PautaCatalogoDTO pauta)
        {
            var resultado = new List<PautaExpedienteEstudiante>();
            if (estudiantes == null) return (resultado, null);

            if (estudiantes.Count > 30)
                return (resultado, "No se pueden registrar más de 30 personas estudiantes por expediente.");

            foreach (var e in estudiantes)
            {
                if (!pauta.RolesEstudiante.Contains(e.Rol))
                    return (resultado, $"El rol \"{e.Rol}\" no corresponde a esta pauta.");

                string nombre = e.NombreCompleto?.Trim() ?? string.Empty;

                if (e.IdEstudiante.HasValue)
                {
                    var matriculado = await _estudianteRepository.GetById(e.IdEstudiante.Value);
                    if (matriculado == null)
                        return (resultado, "Una de las personas estudiantes seleccionadas no existe.");

                    nombre = NombreCompleto(matriculado.Usuario?.Nombre, matriculado.Usuario?.Apellido);
                }

                if (string.IsNullOrWhiteSpace(nombre))
                    return (resultado, "El nombre de la persona estudiante es requerido.");

                if (nombre.Length > 200)
                    return (resultado, "El nombre de la persona estudiante no puede superar los 200 caracteres.");

                string? seccion = string.IsNullOrWhiteSpace(e.Seccion) ? null : e.Seccion.Trim();
                if (seccion != null && seccion.Length > 50)
                    return (resultado, "La sección no puede superar los 50 caracteres.");

                resultado.Add(new PautaExpedienteEstudiante
                {
                    IdEstudiante = e.IdEstudiante,
                    NombreCompleto = nombre,
                    Seccion = seccion,
                    Rol = e.Rol
                });
            }

            return (resultado, null);
        }

        /// Datos que se precargan al crear un formulario (HU 01-03 y 01-12).
        private async Task<Dictionary<string, string>> ArmarPrecarga(int idUsuario, string rol, PautaExpediente? expediente)
        {
            var datos = new Dictionary<string, string>
            {
                ["fecha_hoy"] = DateTime.Now.ToString("yyyy-MM-dd")
            };

            var institucion = await _institucionRepository.GetInstitucion();
            if (institucion != null)
            {
                datos["centro_nombre"] = institucion.Nombre;
                datos["centro_contacto"] = string.Join(" / ",
                    new[] { institucion.Telefono, institucion.Correo }.Where(x => !string.IsNullOrWhiteSpace(x)));

                if (!string.IsNullOrWhiteSpace(institucion.CodigoPresupuestario)) datos["centro_codigo"] = institucion.CodigoPresupuestario;
                if (!string.IsNullOrWhiteSpace(institucion.CircuitoEscolar)) datos["centro_circuito"] = institucion.CircuitoEscolar;
                if (!string.IsNullOrWhiteSpace(institucion.Dre)) datos["centro_dre"] = institucion.Dre;
                if (!string.IsNullOrWhiteSpace(institucion.NombreDirector)) datos["centro_director"] = institucion.NombreDirector;
            }

            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            if (usuario != null)
            {
                string nombre = NombreCompleto(usuario.Nombre, usuario.Apellido);
                datos["usuario_nombre"] = nombre;
                datos["usuario_cargo"] = rol;
                datos["usuario_nombre_cargo"] = $"{nombre}, {rol}";
            }

            if (expediente != null)
                datos["expediente_fecha"] = expediente.FechaActivacion.ToString("yyyy-MM-dd");

            return datos;
        }

        private async Task AgregarPrecargaEstudiante(Dictionary<string, string> datos, PautaExpedienteEstudiante involucrado)
        {
            datos["est_nombre"] = involucrado.NombreCompleto;
            if (!string.IsNullOrWhiteSpace(involucrado.Seccion))
                datos["est_seccion"] = involucrado.Seccion;

            if (!involucrado.IdEstudiante.HasValue) return;

            var matriculado = await _estudianteRepository.GetById(involucrado.IdEstudiante.Value);
            if (matriculado == null) return;

            if (!string.IsNullOrWhiteSpace(matriculado.Identificacion)) datos["est_cedula"] = matriculado.Identificacion;
            if (matriculado.FechaNacimiento.HasValue)
            {
                datos["est_nacimiento"] = matriculado.FechaNacimiento.Value.ToString("yyyy-MM-dd");
                var edad = CalcularEdad(matriculado.FechaNacimiento);
                if (edad.HasValue) datos["est_edad"] = edad.Value.ToString();
            }
            if (!string.IsNullOrWhiteSpace(matriculado.Grado?.Nombre)) datos["est_nivel"] = matriculado.Grado!.Nombre;
            if (!string.IsNullOrWhiteSpace(matriculado.Direccion)) datos["direccion"] = matriculado.Direccion;
        }

        private async Task<int> CrearFormularioInterno(int idUsuario, string rol, PautaExpediente expediente, int numero, PautaExpedienteEstudiante? involucrado)
        {
            var plantilla = PlantillasPauta.Obtener(numero)!;

            var precarga = await ArmarPrecarga(idUsuario, rol, expediente);
            if (involucrado != null)
                await AgregarPrecargaEstudiante(precarga, involucrado);

            JsonObject? informeActuacion = null;
            if (numero == 12)
            {
                var f11 = expediente.Formularios.FirstOrDefault(f => f.NumeroFormulario == 11);
                if (f11 != null)
                {
                    try { informeActuacion = JsonNode.Parse(f11.Datos) as JsonObject; }
                    catch (JsonException) { informeActuacion = null; }
                }
            }

            var estudiantes = expediente.Estudiantes.Select(MapEstudiante).ToList();
            string datos = FormularioPautaHelper.CrearDatosIniciales(plantilla, precarga, estudiantes, informeActuacion);

            var formulario = new PautaFormulario
            {
                IdExpediente = expediente.IdExpediente,
                NumeroFormulario = (byte)numero,
                Secuencia = await _pautaRepository.SiguienteSecuencia(expediente.IdExpediente, (byte)numero),
                Estado = EstadosFormularioPauta.Borrador,
                Datos = datos,
                IdUsuarioActualiza = idUsuario
            };

            return await _pautaRepository.CrearFormulario(formulario);
        }

        private async Task<RespuestaDTO<FormularioDetalleDTO>> DetalleFormulario(
            int idUsuario, string rol, int idFormulario, string? mensaje, bool registrarVista = false)
        {
            var formulario = await _pautaRepository.GetFormulario(idFormulario);
            var expediente = formulario == null ? null : await CargarExpediente(idUsuario, rol, formulario.IdExpediente);

            if (formulario == null || expediente == null)
                return RespuestaDTO<FormularioDetalleDTO>.Error("El formulario no existe o no tiene acceso a él.", 7003);

            return RespuestaDTO<FormularioDetalleDTO>.Exito(MapFormulario(expediente, formulario, rol), mensaje);
        }

        // =====================  Cierre  =====================

        private static List<string> CalcularPendientesCierre(PautaExpediente expediente)
        {
            var pendientes = new List<string>();
            var pauta = PautasCatalogo.Obtener(expediente.NumeroPauta)!;

            foreach (int requerido in pauta.Requeridos)
            {
                if (!expediente.Formularios.Any(f => f.NumeroFormulario == requerido))
                    pendientes.Add($"Falta crear el formulario N°{requerido}.");
            }

            foreach (var f in expediente.Formularios.OrderBy(f => f.NumeroFormulario).ThenBy(f => f.Secuencia))
            {
                if (f.Estado != EstadosFormularioPauta.Finalizado)
                    pendientes.Add($"El formulario N°{f.NumeroFormulario} (#{f.Secuencia}) está sin finalizar.");
            }

            return pendientes;
        }

        /// El informe de actuación (N°11) permite cerrar directo si no se identificó violencia ni vulnerabilidad.
        private static bool InformeActuacionSinIdentificacion(PautaExpediente expediente)
        {
            var f11 = expediente.Formularios.FirstOrDefault(f => f.NumeroFormulario == 11);
            if (f11 == null) return false;

            try
            {
                var datos = JsonNode.Parse(f11.Datos) as JsonObject;
                return datos?["sin_identificacion"] is JsonArray marcadas && marcadas.Count > 0;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        // =====================  Conversión a DTO  =====================

        private static EstudianteInvolucradoDTO MapEstudiante(PautaExpedienteEstudiante e) => new()
        {
            IdPautaEstudiante = e.IdPautaEstudiante,
            IdEstudiante = e.IdEstudiante,
            NombreCompleto = e.NombreCompleto,
            Seccion = e.Seccion,
            Rol = e.Rol
        };

        private static ExpedienteResumenDTO MapResumen(PautaExpediente e) => new()
        {
            IdExpediente = e.IdExpediente,
            Codigo = e.Codigo,
            NumeroPauta = e.NumeroPauta,
            NombrePauta = PautasCatalogo.Obtener(e.NumeroPauta)?.Nombre ?? string.Empty,
            Estado = e.Estado,
            FechaActivacion = e.FechaActivacion,
            Responsable = NombreCompleto(e.UsuarioResponsable?.Nombre, e.UsuarioResponsable?.Apellido),
            Estudiantes = e.Estudiantes.Select(x => x.NombreCompleto).ToList(),
            TotalFormularios = e.Formularios.Count,
            FormulariosFinalizados = e.Formularios.Count(f => f.Estado == EstadosFormularioPauta.Finalizado)
        };

        private Task<ExpedienteDetalleDTO> MapDetalle(PautaExpediente e)
        {
            var resumen = MapResumen(e);
            var pauta = PautasCatalogo.Obtener(e.NumeroPauta)!;
            var creados = e.Formularios.Select(f => (int)f.NumeroFormulario).ToHashSet();

            var detalle = new ExpedienteDetalleDTO
            {
                IdExpediente = resumen.IdExpediente,
                Codigo = resumen.Codigo,
                NumeroPauta = resumen.NumeroPauta,
                NombrePauta = resumen.NombrePauta,
                Estado = resumen.Estado,
                FechaActivacion = resumen.FechaActivacion,
                Responsable = resumen.Responsable,
                Estudiantes = resumen.Estudiantes,
                TotalFormularios = resumen.TotalFormularios,
                FormulariosFinalizados = resumen.FormulariosFinalizados,
                IdExpedienteOrigen = e.IdExpedienteOrigen,
                EstudiantesInvolucrados = e.Estudiantes.Select(MapEstudiante).ToList(),
                Formularios = e.Formularios
                    .OrderBy(f => f.NumeroFormulario).ThenBy(f => f.Secuencia)
                    .Select(f => new FormularioResumenDTO
                    {
                        IdFormulario = f.IdFormulario,
                        NumeroFormulario = f.NumeroFormulario,
                        Nombre = PlantillasPauta.Obtener(f.NumeroFormulario)?.Nombre ?? string.Empty,
                        Secuencia = f.Secuencia,
                        Estado = f.Estado,
                        Requerido = pauta.Requeridos.Contains(f.NumeroFormulario),
                        FechaActualizacion = f.FechaActualizacion
                    }).ToList(),
                FormulariosFaltantes = pauta.Requeridos.Concat(pauta.Sugeridos)
                    .Distinct()
                    .Where(n => !creados.Contains(n))
                    .Select(n => new FormularioFaltanteDTO
                    {
                        Numero = n,
                        Nombre = PlantillasPauta.Obtener(n)?.Nombre ?? string.Empty,
                        Requerido = pauta.Requeridos.Contains(n)
                    }).ToList(),
                FechaCierre = e.FechaCierre,
                NombreCierre = e.NombreCierre,
                VistoBuenoDirector = e.VistoBuenoDirector,
                DevolucionRealizada = e.DevolucionRealizada,
                ObservacionCierre = e.ObservacionCierre,
                PuedeEditar = e.Estado == EstadosExpedientePauta.Abierto,
                PendientesCierre = e.Estado == EstadosExpedientePauta.Abierto ? CalcularPendientesCierre(e) : new List<string>()
            };

            return Task.FromResult(detalle);
        }

        private static FormularioDetalleDTO MapFormulario(PautaExpediente expediente, PautaFormulario f, string rol)
        {
            bool abierto = expediente.Estado == EstadosExpedientePauta.Abierto;

            return new FormularioDetalleDTO
            {
                IdFormulario = f.IdFormulario,
                IdExpediente = f.IdExpediente,
                CodigoExpediente = expediente.Codigo,
                NumeroFormulario = f.NumeroFormulario,
                Secuencia = f.Secuencia,
                Estado = f.Estado,
                Plantilla = PlantillasPauta.Obtener(f.NumeroFormulario)!,
                Datos = f.Datos,
                PuedeEditar = abierto && f.Estado != EstadosFormularioPauta.Finalizado,
                PuedeReabrir = abierto && VeTodo(rol) && f.Estado == EstadosFormularioPauta.Finalizado,
                FechaActualizacion = f.FechaActualizacion
            };
        }
    }
}
