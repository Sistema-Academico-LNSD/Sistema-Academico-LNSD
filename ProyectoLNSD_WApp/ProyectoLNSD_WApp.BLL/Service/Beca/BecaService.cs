using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Entities;
using ProyectoLNSD_WApp.DAL.Repositories.Beca;
using ProyectoLNSD_WApp.DAL.Repositories.Estudiante;
using ProyectoLNSD_WApp.DAL.Repositories.Grado;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using BecaEntity = ProyectoLNSD_WApp.DAL.Entities.Beca;
using EstudianteEntity = ProyectoLNSD_WApp.DAL.Entities.Estudiante;
using PeriodoEntity = ProyectoLNSD_WApp.DAL.Entities.PeriodoLectivo;
using UsuarioEntity = ProyectoLNSD_WApp.DAL.Entities.Usuario;

namespace ProyectoLNSD_WApp.BLL.Service.Beca
{
    public class BecaService : IBecaService
    {
        private const int LimiteLista = 500;

        private const int LimiteEstudiantes = 20;

        private readonly IBecaRepository _becaRepository;
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IGradoRepository _gradoRepository;

        public BecaService(
            IBecaRepository becaRepository,
            IEstudianteRepository estudianteRepository,
            IPeriodoLectivoRepository periodoRepository,
            IGradoRepository gradoRepository)
        {
            _becaRepository = becaRepository;
            _estudianteRepository = estudianteRepository;
            _periodoRepository = periodoRepository;
            _gradoRepository = gradoRepository;
        }

        public async Task<RespuestaDTO<List<RequisitoBecaDTO>>> GetRequisitos(bool incluirInactivos)
        {
            try
            {
                var lista = (await _becaRepository.GetRequisitos(!incluirInactivos))
                    .Select(ToDTO)
                    .ToList();

                return RespuestaDTO<List<RequisitoBecaDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No hay requisitos registrados" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<RequisitoBecaDTO>>.Error("No fue posible cargar los requisitos", 1510);
            }
        }

        public async Task<RespuestaDTO<RequisitoBecaDTO>> GuardarRequisito(RequisitoBecaGuardarDTO dto)
        {
            try
            {
                string nombre = BecaReglas.Limpiar(dto.Nombre) ?? string.Empty;

                if (nombre.Length == 0)
                    return FallaRequisito("Indique el nombre del requisito", 1501);

                string? verificacion = BecaReglas.Limpiar(dto.Verificacion);

                if (verificacion != null && !VerificacionRequisito.Todas.Contains(verificacion))
                    return FallaRequisito("El tipo de verificación no es válido", 1501);

                if (await _becaRepository.ExisteNombreRequisito(nombre, dto.IdRequisito > 0 ? dto.IdRequisito : null))
                    return FallaRequisito("Ya existe un requisito con ese nombre", 1501);

                if (verificacion != null)
                {
                    var repetido = (await _becaRepository.GetRequisitos(true))
                        .FirstOrDefault(r => r.Verificacion == verificacion && r.IdRequisito != dto.IdRequisito);

                    if (repetido != null)
                        return FallaRequisito($"El requisito \"{repetido.Nombre}\" ya se verifica de esa forma", 1501);
                }

                BecaRequisito requisito;

                if (dto.IdRequisito > 0)
                {
                    requisito = await _becaRepository.GetRequisito(dto.IdRequisito)
                        ?? throw new KeyNotFoundException();

                    requisito.Nombre = nombre;
                    requisito.Descripcion = BecaReglas.Limpiar(dto.Descripcion);
                    requisito.Obligatorio = dto.Obligatorio;
                    requisito.Verificacion = verificacion;

                    await _becaRepository.ActualizarRequisito(requisito);
                }
                else
                {
                    requisito = new BecaRequisito
                    {
                        Nombre = nombre,
                        Descripcion = BecaReglas.Limpiar(dto.Descripcion),
                        Obligatorio = dto.Obligatorio,
                        Verificacion = verificacion,
                        Activo = true
                    };

                    await _becaRepository.CrearRequisito(requisito);
                }

                return RespuestaDTO<RequisitoBecaDTO>.Exito(
                    ToDTO(requisito),
                    dto.IdRequisito > 0 ? "Requisito actualizado" : "Requisito registrado");
            }
            catch (KeyNotFoundException)
            {
                return FallaRequisito("El requisito no existe", 1502);
            }
            catch (Exception)
            {
                return FallaRequisito("No fue posible guardar el requisito", 1510);
            }
        }

        public async Task<RespuestaDTO<RequisitoBecaDTO>> CambiarEstadoRequisito(int idRequisito, bool activo)
        {
            try
            {
                var requisito = await _becaRepository.GetRequisito(idRequisito);

                if (requisito == null)
                    return FallaRequisito("El requisito no existe", 1502);

                if (activo && requisito.Verificacion != null &&
                    (await _becaRepository.GetRequisitos(true))
                        .Any(r => r.Verificacion == requisito.Verificacion && r.IdRequisito != idRequisito))
                    return FallaRequisito("Ya hay otro requisito activo que se verifica de esa forma", 1501);

                requisito.Activo = activo;
                await _becaRepository.ActualizarRequisito(requisito);

                return RespuestaDTO<RequisitoBecaDTO>.Exito(
                    ToDTO(requisito),
                    activo ? "Requisito activado" : "Requisito desactivado; ya no se exige para activar becas");
            }
            catch (Exception)
            {
                return FallaRequisito("No fue posible cambiar el estado del requisito", 1510);
            }
        }

        public async Task<RespuestaDTO<OpcionesBecaDTO>> GetOpciones()
        {
            try
            {
                var hoy = BecaReglas.Hoy();
                var periodos = await _periodoRepository.GetPeriodos();
                var grados = await _gradoRepository.GetGrados();

                return RespuestaDTO<OpcionesBecaDTO>.Exito(new OpcionesBecaDTO
                {
                    IdPeriodoActivo = periodos.FirstOrDefault(p => p.Activo)?.IdPeriodo,

                    Periodos = periodos.Select(p => new PeriodoBecaDTO
                    {
                        Id = p.IdPeriodo,
                        Nombre = p.Nombre,
                        FechaInicio = p.FechaInicio,
                        FechaFin = p.FechaFin,
                        Activo = p.Activo,
                        Finalizado = BecaReglas.PeriodoFinalizado(p, hoy)
                    }).ToList(),

                    Grados = grados
                        .OrderBy(g => g.Nombre)
                        .Select(g => new OpcionFiltroDTO { Id = g.IdGrado, Nombre = g.Nombre })
                        .ToList(),

                    Estados = EstadoVisibleBeca.Todos.ToList()
                });
            }
            catch (Exception)
            {
                return RespuestaDTO<OpcionesBecaDTO>.Error("No fue posible cargar los filtros", 1510);
            }
        }

        public async Task<RespuestaDTO<List<EstudianteBecaDTO>>> BuscarEstudiantes(string? texto)
        {
            try
            {
                string? limpio = BecaReglas.Limpiar(texto);

                if (limpio == null || limpio.Length < 2)
                    return RespuestaDTO<List<EstudianteBecaDTO>>.Exito(
                        new List<EstudianteBecaDTO>(), "Escriba al menos dos letras del nombre, la identificación o el carné");

                var lista = (await _estudianteRepository.Buscar(limpio, null, true))
                    .OrderBy(e => e.Usuario?.Apellido)
                    .ThenBy(e => e.Usuario?.Nombre)
                    .Take(LimiteEstudiantes)
                    .Select(e => new EstudianteBecaDTO
                    {
                        IdEstudiante = e.IdEstudiante,
                        NombreEstudiante = NombreEstudiante(e),
                        Identificacion = e.Identificacion,
                        Carnet = e.Carnet,
                        Grado = e.Grado?.Nombre ?? string.Empty
                    })
                    .ToList();

                return RespuestaDTO<List<EstudianteBecaDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No se encontraron estudiantes activos con ese dato" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<EstudianteBecaDTO>>.Error("No fue posible buscar estudiantes", 1510);
            }
        }
        public async Task<RespuestaDTO<BecaDetalleDTO>> Preparar(int idEstudiante, int idPeriodo)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetById(idEstudiante);

                if (estudiante == null)
                    return FallaDetalle("El estudiante no existe", 1505);

                var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

                if (periodo == null)
                    return FallaDetalle("Seleccione un período lectivo válido", 1506);

                var existente = await _becaRepository.GetDeEstudiante(idEstudiante, idPeriodo);

                if (existente != null)
                {
                    var detalle = await ArmarDetalle(existente.IdBeca);

                    return RespuestaDTO<BecaDetalleDTO>.Exito(
                        detalle,
                        $"El estudiante ya tiene una beca en {periodo.Nombre}; se abrió para actualizarla");
                }

                var hoy = BecaReglas.Hoy();

                if (!estudiante.Estado)
                    return FallaDetalle("El estudiante está inactivo; no se le puede registrar una beca", 1505);

                if (BecaReglas.PeriodoFinalizado(periodo, hoy))
                    return FallaDetalle(MensajePeriodoFinalizado(periodo), 1509);

                string? estadoMatricula = await _becaRepository.GetEstadoMatricula(idEstudiante, idPeriodo);
                var requisitos = await _becaRepository.GetRequisitos(false);
                var evaluados = EvaluarRequisitos(requisitos, new HashSet<int>(), estadoMatricula);
                var (grado, seccion) = await _becaRepository.GetUbicacion(idEstudiante, idPeriodo);

                return RespuestaDTO<BecaDetalleDTO>.Exito(new BecaDetalleDTO
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    NombreEstudiante = NombreEstudiante(estudiante),
                    Identificacion = estudiante.Identificacion,
                    Carnet = estudiante.Carnet,
                    Grado = TextoGrado(grado, seccion),
                    EstadoMatricula = estadoMatricula,
                    IdPeriodo = periodo.IdPeriodo,
                    NombrePeriodo = periodo.Nombre,
                    PeriodoInicio = periodo.FechaInicio,
                    PeriodoFin = periodo.FechaFin,
                    FechaInicio = hoy > periodo.FechaInicio ? hoy : periodo.FechaInicio,
                    FechaFin = periodo.FechaFin,

                    Requisitos = evaluados,
                    CumpleRequisitos = Cumple(evaluados),
                    Editable = true
                });
            }
            catch (Exception)
            {
                return FallaDetalle("No fue posible preparar el registro de la beca", 1510);
            }
        }

        public async Task<RespuestaDTO<BecaDetalleDTO>> GetDetalle(int idBeca)
        {
            try
            {
                var detalle = await ArmarDetalle(idBeca);

                return detalle == null
                    ? FallaDetalle("La beca no existe", 1503)
                    : RespuestaDTO<BecaDetalleDTO>.Exito(detalle);
            }
            catch (Exception)
            {
                return FallaDetalle("No fue posible cargar la beca", 1510);
            }
        }

        public async Task<RespuestaDTO<BecaDetalleDTO>> Guardar(int idUsuario, BecaGuardarDTO dto)
        {
            try
            {
                var hoy = BecaReglas.Hoy();
                bool esNueva = dto.IdBeca <= 0;

                BecaEntity? beca;
                EstudianteEntity? estudiante;
                PeriodoEntity? periodo;

                if (esNueva)
                {
                    estudiante = await _estudianteRepository.GetById(dto.IdEstudiante);

                    if (estudiante == null)
                        return FallaDetalle("Seleccione un estudiante", 1505);

                    if (!estudiante.Estado)
                        return FallaDetalle("El estudiante está inactivo; no se le puede registrar una beca", 1505);

                    periodo = await _periodoRepository.GetPeriodo(dto.IdPeriodo);

                    if (periodo == null)
                        return FallaDetalle("Seleccione un período lectivo válido", 1506);

                    if (await _becaRepository.GetDeEstudiante(dto.IdEstudiante, dto.IdPeriodo) != null)
                        return FallaDetalle(
                            $"El estudiante ya tiene una beca en {periodo.Nombre}. Actualícela desde la lista", 1507);

                    beca = new BecaEntity
                    {
                        IdEstudiante = estudiante.IdEstudiante,
                        IdPeriodo = periodo.IdPeriodo,
                        Estado = EstadoBeca.Pendiente,
                        IdUsuarioRegistra = idUsuario,
                        FechaRegistro = DateTime.Now
                    };
                }
                else
                {
                    beca = await _becaRepository.GetById(dto.IdBeca);

                    if (beca == null)
                        return FallaDetalle("La beca no existe", 1503);

                    estudiante = beca.Estudiante;
                    periodo = beca.Periodo;

                    if (estudiante == null || periodo == null)
                        return FallaDetalle("La beca no existe", 1503);
                }

                if (BecaReglas.PeriodoFinalizado(periodo, hoy))
                    return FallaDetalle(MensajePeriodoFinalizado(periodo), 1509);

                string? error = BecaReglas.ValidarMonto(dto.Monto)
                    ?? BecaReglas.ValidarVigencia(dto.FechaInicio, dto.FechaFin, periodo);

                string? observacion = BecaReglas.Limpiar(dto.Observacion);

                if (error == null && observacion?.Length > BecaReglas.MaxObservacion)
                    error = $"La observación no puede superar los {BecaReglas.MaxObservacion} caracteres";

                if (error != null)
                    return FallaDetalle(error, 1504);

                var requisitos = await _becaRepository.GetRequisitos(false);
                var marcados = (dto.IdsRequisitosCumplidos ?? new List<int>()).ToHashSet();
                var manualesActivos = requisitos.Where(r => r.Activo && r.Verificacion == null)
                    .Select(r => r.IdRequisito).ToHashSet();

                if (marcados.Any(id => !manualesActivos.Contains(id)))
                    return FallaDetalle("Uno de los requisitos marcados no es válido. Recargue la página", 1504);

                var inactivosConservados = beca.RequisitosCumplidos
                    .Select(c => c.IdRequisito)
                    .Where(id => requisitos.Any(r => r.IdRequisito == id && !r.Activo));

                var cumplidos = marcados.Union(inactivosConservados).ToHashSet();

                string? estadoMatricula = await _becaRepository.GetEstadoMatricula(estudiante.IdEstudiante, periodo.IdPeriodo);
                var evaluados = EvaluarRequisitos(requisitos, cumplidos, estadoMatricula);

                bool quedaActiva = beca.Estado == EstadoBeca.Activa || dto.Activar;

                if (quedaActiva)
                {
                    string? motivo = MotivoNoActivable(evaluados, estudiante, dto.FechaFin!.Value, hoy);

                    if (motivo != null)
                        return FallaDetalle(
                            beca.Estado == EstadoBeca.Activa
                                ? $"La beca está activa y con estos cambios no se puede mantener: {motivo}. " +
                                  "Si ya no cumple, desactívela indicando el motivo"
                                : $"No se puede activar la beca: {motivo}. Guárdela sin activar",
                            1508);
                }

                beca.Monto = dto.Monto!.Value;
                beca.FechaInicio = dto.FechaInicio!.Value;
                beca.FechaFin = dto.FechaFin!.Value;
                beca.Observacion = observacion;

                if (quedaActiva && beca.Estado != EstadoBeca.Activa)
                    MarcarActiva(beca);

                if (esNueva)
                {
                    await _becaRepository.Crear(beca, cumplidos);
                }
                else
                {
                    beca.IdUsuarioModifica = idUsuario;
                    beca.FechaModificacion = DateTime.Now;
                    await _becaRepository.Actualizar(beca, cumplidos);
                }

                string mensaje = esNueva
                    ? (beca.Estado == EstadoBeca.Activa ? "Beca registrada y activada" : "Beca registrada como pendiente")
                    : "Beca actualizada";

                return RespuestaDTO<BecaDetalleDTO>.Exito(await ArmarDetalle(beca.IdBeca), mensaje);
            }
            catch (Exception)
            {
                return FallaDetalle("No fue posible guardar la beca", 1510);
            }
        }

        public async Task<RespuestaDTO<BecaDetalleDTO>> Activar(int idUsuario, int idBeca)
        {
            try
            {
                var (beca, error) = await GetModificable(idBeca);

                if (error != null)
                    return error;

                if (beca!.Estado == EstadoBeca.Activa)
                    return FallaDetalle("La beca ya está activa", 1504);

                var requisitos = await _becaRepository.GetRequisitos(false);
                string? estadoMatricula = await _becaRepository.GetEstadoMatricula(beca.IdEstudiante, beca.IdPeriodo);
                var evaluados = EvaluarRequisitos(
                    requisitos,
                    beca.RequisitosCumplidos.Select(c => c.IdRequisito).ToHashSet(),
                    estadoMatricula);

                string? motivo = MotivoNoActivable(evaluados, beca.Estudiante!, beca.FechaFin, BecaReglas.Hoy());

                if (motivo != null)
                    return FallaDetalle($"No se puede activar la beca: {motivo}", 1508);

                MarcarActiva(beca);
                beca.IdUsuarioModifica = idUsuario;
                beca.FechaModificacion = DateTime.Now;

                await _becaRepository.Actualizar(beca, null);

                return RespuestaDTO<BecaDetalleDTO>.Exito(await ArmarDetalle(idBeca), "Beca activada");
            }
            catch (Exception)
            {
                return FallaDetalle("No fue posible activar la beca", 1510);
            }
        }
        public async Task<RespuestaDTO<BecaDetalleDTO>> Desactivar(int idUsuario, int idBeca, string? motivo)
        {
            try
            {
                string? limpio = BecaReglas.Limpiar(motivo);

                if (limpio == null)
                    return FallaDetalle("Indique el motivo por el que se desactiva la beca", 1504);

                if (limpio.Length > BecaReglas.MaxMotivo)
                    return FallaDetalle($"El motivo no puede superar los {BecaReglas.MaxMotivo} caracteres", 1504);

                var (beca, error) = await GetModificable(idBeca);

                if (error != null)
                    return error;

                if (beca!.Estado == EstadoBeca.Inactiva)
                    return FallaDetalle("La beca ya está inactiva", 1504);

                beca.Estado = EstadoBeca.Inactiva;
                beca.MotivoDesactivacion = limpio;
                beca.FechaDesactivacion = DateTime.Now;
                beca.IdUsuarioModifica = idUsuario;
                beca.FechaModificacion = DateTime.Now;

                await _becaRepository.Actualizar(beca, null);

                return RespuestaDTO<BecaDetalleDTO>.Exito(await ArmarDetalle(idBeca), "Beca desactivada");
            }
            catch (Exception)
            {
                return FallaDetalle("No fue posible desactivar la beca", 1510);
            }
        }

        public async Task<RespuestaDTO<List<BecaListaDTO>>> Buscar(BecaFiltroDTO filtro)
        {
            try
            {
                string? estado = EstadoVisibleBeca.Todos.FirstOrDefault(e =>
                    string.Equals(e, BecaReglas.Limpiar(filtro.Estado), StringComparison.OrdinalIgnoreCase));

                var filas = await _becaRepository.Buscar(new BecaFiltro
                {
                    IdPeriodo = filtro.IdPeriodo,
                    IdGrado = filtro.IdGrado,
                    Texto = BecaReglas.Limpiar(filtro.Texto),
                    Estado = estado,
                    Hoy = BecaReglas.Hoy()
                }, LimiteLista);

                var lista = await ToListaDTO(filas);

                string? mensaje =
                    lista.Count == 0 ? "No se encontraron becas con esos filtros"
                    : lista.Count >= LimiteLista ? $"Se muestran las primeras {LimiteLista} becas; afine los filtros"
                    : null;

                return RespuestaDTO<List<BecaListaDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<BecaListaDTO>>.Error("No fue posible consultar las becas", 1510);
            }
        }

        public async Task<RespuestaDTO<HistorialBecaDTO>> GetHistorial(int idEstudiante)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetById(idEstudiante);

                if (estudiante == null)
                    return RespuestaDTO<HistorialBecaDTO>.Error("El estudiante no existe", 1505);

                var filas = await _becaRepository.Buscar(
                    new BecaFiltro { IdEstudiante = idEstudiante, Hoy = BecaReglas.Hoy() },
                    LimiteLista);

                var becas = await ToListaDTO(filas);

                return RespuestaDTO<HistorialBecaDTO>.Exito(
                    new HistorialBecaDTO
                    {
                        IdEstudiante = estudiante.IdEstudiante,
                        NombreEstudiante = NombreEstudiante(estudiante),
                        Identificacion = estudiante.Identificacion,
                        Carnet = estudiante.Carnet,
                        MontoTotalActivas = becas.Where(b => b.Estado == EstadoBeca.Activa).Sum(b => b.Monto),
                        Becas = becas
                    },
                    becas.Count == 0 ? "El estudiante no tiene becas registradas" : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<HistorialBecaDTO>.Error("No fue posible consultar el historial de becas", 1510);
            }
        }

        public async Task<RespuestaDTO<List<MisBecasEstudianteDTO>>> GetMisBecas(int idUsuario, bool esEncargado)
        {
            try
            {
                var hoy = BecaReglas.Hoy();

                var filas = (await _becaRepository.Buscar(new BecaFiltro
                {
                    IdUsuarioEstudiante = esEncargado ? null : idUsuario,
                    IdUsuarioEncargado = esEncargado ? idUsuario : null,
                    Hoy = hoy
                }, LimiteLista))
                    .Where(b => b.Estado == EstadoBeca.Activa)
                    .ToList();

                var lista = filas
                    .GroupBy(b => b.IdEstudiante)
                    .Select(g => new MisBecasEstudianteDTO
                    {
                        NombreEstudiante = $"{g.First().Nombre} {g.First().Apellido}".Trim(),
                        Carnet = g.First().Carnet,
                        Grado = TextoGrado(g.First().NombreGrado, g.First().NombreSeccion),
                        Becas = g
                            .OrderByDescending(b => b.FechaInicio)
                            .Select(b => new MiBecaDTO
                            {
                                NombrePeriodo = b.NombrePeriodo,
                                Monto = b.Monto,
                                FechaInicio = b.FechaInicio,
                                FechaFin = b.FechaFin,
                                Vigencia = BecaReglas.Vigencia(b.FechaInicio, b.FechaFin, hoy),
                                DiasRestantes = hoy >= b.FechaInicio && hoy <= b.FechaFin
                                    ? b.FechaFin.DayNumber - hoy.DayNumber
                                    : null
                            })
                            .ToList()
                    })
                    .OrderBy(e => e.NombreEstudiante)
                    .ToList();

                return RespuestaDTO<List<MisBecasEstudianteDTO>>.Exito(
                    lista,
                    lista.Count == 0
                        ? (esEncargado
                            ? "Sus estudiantes no tienen becas activas registradas"
                            : "No tiene becas activas registradas")
                        : null);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<MisBecasEstudianteDTO>>.Error("No fue posible consultar las becas", 1510);
            }
        }

        private async Task<(BecaEntity? Beca, RespuestaDTO<BecaDetalleDTO>? Error)> GetModificable(int idBeca)
        {
            var beca = await _becaRepository.GetById(idBeca);

            if (beca == null || beca.Periodo == null || beca.Estudiante == null)
                return (null, FallaDetalle("La beca no existe", 1503));

            if (BecaReglas.PeriodoFinalizado(beca.Periodo, BecaReglas.Hoy()))
                return (null, FallaDetalle(MensajePeriodoFinalizado(beca.Periodo), 1509));

            return (beca, null);
        }

        private static void MarcarActiva(BecaEntity beca)
        {
            beca.Estado = EstadoBeca.Activa;
            beca.FechaActivacion = DateTime.Now;
            beca.MotivoDesactivacion = null;
            beca.FechaDesactivacion = null;
        }

        private static string? MotivoNoActivable(
            List<RequisitoCumplimientoDTO> evaluados, EstudianteEntity estudiante, DateOnly fechaFin, DateOnly hoy)
        {
            if (!estudiante.Estado)
                return "el estudiante está inactivo";

            if (fechaFin < hoy)
                return "su vigencia ya terminó; actualice las fechas";

            var faltantes = Faltantes(evaluados);

            return faltantes.Count == 0
                ? null
                : "faltan requisitos (" + string.Join(", ", faltantes) + ")";
        }

        private static List<RequisitoCumplimientoDTO> EvaluarRequisitos(
            List<BecaRequisito> requisitos, HashSet<int> cumplidos, string? estadoMatricula)
        {
            return requisitos
                .Where(r => r.Activo || (r.Verificacion == null && cumplidos.Contains(r.IdRequisito)))
                .Select(r => new RequisitoCumplimientoDTO
                {
                    IdRequisito = r.IdRequisito,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    Obligatorio = r.Obligatorio,
                    Automatico = r.Verificacion != null,
                    Activo = r.Activo,
                    Cumplido = r.Verificacion switch
                    {
                        VerificacionRequisito.MatriculaActiva => estadoMatricula == BecaReglas.MatriculaActiva,
                        _ => cumplidos.Contains(r.IdRequisito)
                    }
                })
                .ToList();
        }

        private static List<string> Faltantes(IEnumerable<RequisitoCumplimientoDTO> evaluados) =>
            evaluados
                .Where(r => r.Activo && r.Obligatorio && !r.Cumplido)
                .Select(r => r.Nombre)
                .ToList();

        private static bool Cumple(IEnumerable<RequisitoCumplimientoDTO> evaluados) =>
            Faltantes(evaluados).Count == 0;

        private async Task<BecaDetalleDTO?> ArmarDetalle(int idBeca)
        {
            var beca = await _becaRepository.GetById(idBeca);

            if (beca == null || beca.Periodo == null || beca.Estudiante == null)
                return null;

            var hoy = BecaReglas.Hoy();
            var requisitos = await _becaRepository.GetRequisitos(false);
            string? estadoMatricula = await _becaRepository.GetEstadoMatricula(beca.IdEstudiante, beca.IdPeriodo);
            var (grado, seccion) = await _becaRepository.GetUbicacion(beca.IdEstudiante, beca.IdPeriodo);

            var evaluados = EvaluarRequisitos(
                requisitos,
                beca.RequisitosCumplidos.Select(c => c.IdRequisito).ToHashSet(),
                estadoMatricula);

            bool finalizado = BecaReglas.PeriodoFinalizado(beca.Periodo, hoy);

            return new BecaDetalleDTO
            {
                IdBeca = beca.IdBeca,
                IdEstudiante = beca.IdEstudiante,
                NombreEstudiante = NombreEstudiante(beca.Estudiante),
                Identificacion = beca.Estudiante.Identificacion,
                Carnet = beca.Estudiante.Carnet,
                Grado = TextoGrado(grado, seccion),
                EstadoMatricula = estadoMatricula,
                IdPeriodo = beca.IdPeriodo,
                NombrePeriodo = beca.Periodo.Nombre,
                PeriodoInicio = beca.Periodo.FechaInicio,
                PeriodoFin = beca.Periodo.FechaFin,
                Monto = beca.Monto,
                FechaInicio = beca.FechaInicio,
                FechaFin = beca.FechaFin,
                Estado = beca.Estado,
                EstadoVisible = BecaReglas.EstadoVisible(beca.Estado, beca.FechaFin, hoy),
                Vigencia = BecaReglas.Vigencia(beca.FechaInicio, beca.FechaFin, hoy),
                Observacion = beca.Observacion,
                MotivoDesactivacion = beca.MotivoDesactivacion,
                Requisitos = evaluados,
                CumpleRequisitos = Cumple(evaluados),
                Editable = !finalizado,
                MotivoNoEditable = finalizado ? MensajePeriodoFinalizado(beca.Periodo) : null,
                RegistradaPor = NombreUsuario(beca.UsuarioRegistra),
                FechaRegistro = beca.FechaRegistro,
                ModificadaPor = NombreUsuario(beca.UsuarioModifica),
                FechaModificacion = beca.FechaModificacion,
                FechaActivacion = beca.FechaActivacion,
                FechaDesactivacion = beca.FechaDesactivacion
            };
        }

        private async Task<List<BecaListaDTO>> ToListaDTO(List<BecaListado> filas)
        {
            if (filas.Count == 0)
                return new List<BecaListaDTO>();

            var hoy = BecaReglas.Hoy();
            var requisitos = await _becaRepository.GetRequisitos(false);
            var periodosFinalizados = (await _periodoRepository.GetPeriodos())
                .Where(p => BecaReglas.PeriodoFinalizado(p, hoy))
                .Select(p => p.IdPeriodo)
                .ToHashSet();

            return filas.Select(b =>
            {
                var evaluados = EvaluarRequisitos(requisitos, b.IdsRequisitosCumplidos.ToHashSet(), b.EstadoMatricula);
                var faltantes = Faltantes(evaluados);

                return new BecaListaDTO
                {
                    IdBeca = b.IdBeca,
                    IdEstudiante = b.IdEstudiante,
                    NombreEstudiante = $"{b.Nombre} {b.Apellido}".Trim(),
                    Identificacion = b.Identificacion,
                    Carnet = b.Carnet,
                    IdPeriodo = b.IdPeriodo,
                    NombrePeriodo = b.NombrePeriodo,
                    Grado = TextoGrado(b.NombreGrado, b.NombreSeccion),
                    Monto = b.Monto,
                    FechaInicio = b.FechaInicio,
                    FechaFin = b.FechaFin,
                    Estado = b.Estado,
                    EstadoVisible = BecaReglas.EstadoVisible(b.Estado, b.FechaFin, hoy),
                    Vigencia = BecaReglas.Vigencia(b.FechaInicio, b.FechaFin, hoy),
                    CumpleRequisitos = faltantes.Count == 0 && b.EstudianteActivo,
                    RequisitosFaltantes = b.EstudianteActivo
                        ? faltantes
                        : faltantes.Prepend("Estudiante activo").ToList(),
                    MotivoDesactivacion = b.MotivoDesactivacion,
                    Editable = !periodosFinalizados.Contains(b.IdPeriodo)
                };
            }).ToList();
        }

        private static RequisitoBecaDTO ToDTO(BecaRequisito r) => new()
        {
            IdRequisito = r.IdRequisito,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            Obligatorio = r.Obligatorio,
            Verificacion = r.Verificacion,
            Activo = r.Activo
        };

        private static string NombreEstudiante(EstudianteEntity? e) =>
            e?.Usuario == null ? string.Empty : $"{e.Usuario.Nombre} {e.Usuario.Apellido}".Trim();

        private static string? NombreUsuario(UsuarioEntity? u) =>
            u == null ? null : $"{u.Nombre} {u.Apellido}".Trim();

        private static string TextoGrado(string? grado, string? seccion) =>
            $"{grado} {seccion}".Trim();

        private static string MensajePeriodoFinalizado(PeriodoEntity periodo) =>
            $"El período {periodo.Nombre} ya finalizó; sus becas solo se pueden consultar";

        private static RespuestaDTO<BecaDetalleDTO> FallaDetalle(string mensaje, int codigo) =>
            RespuestaDTO<BecaDetalleDTO>.Error(mensaje, codigo);

        private static RespuestaDTO<RequisitoBecaDTO> FallaRequisito(string mensaje, int codigo) =>
            RespuestaDTO<RequisitoBecaDTO>.Error(mensaje, codigo);
    }
}
