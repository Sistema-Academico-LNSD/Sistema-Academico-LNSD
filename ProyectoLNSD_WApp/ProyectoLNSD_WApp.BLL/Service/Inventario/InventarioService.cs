using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Entities;
using ProyectoLNSD_WApp.DAL.Repositories.Inventario;
using InventarioEntity = ProyectoLNSD_WApp.DAL.Entities.Inventario;

namespace ProyectoLNSD_WApp.BLL.Service.Inventario
{
    public class InventarioService : IInventarioService
    {
        private const int LimiteLista = 500;

        private const int LimiteReporte = 5000;

        private const int MaxMesesEstadisticas = 36;

        private readonly IInventarioRepository _inventarioRepository;

        public InventarioService(IInventarioRepository inventarioRepository)
        {
            _inventarioRepository = inventarioRepository;
        }

        // =========================================================
        // Tipos / categorías (MIVF-01-02 y 01-03)
        // =========================================================

        public async Task<RespuestaDTO<List<CategoriaInventarioDTO>>> GetCategorias(bool incluirInactivas)
        {
            try
            {
                var categorias = await _inventarioRepository.GetCategorias(!incluirInactivas);
                var conteo = await _inventarioRepository.ContarArticulosPorCategoria();

                var lista = categorias
                    .Select(c => ToDTO(c, conteo.GetValueOrDefault(c.IdCategoria)))
                    .ToList();

                return RespuestaDTO<List<CategoriaInventarioDTO>>.Exito(
                    lista,
                    lista.Count == 0 ? "No hay tipos de inventario registrados" : "Tipos de inventario cargados");
            }
            catch (Exception)
            {
                return RespuestaDTO<List<CategoriaInventarioDTO>>.Error("No fue posible cargar los tipos de inventario", 1610);
            }
        }

        public async Task<RespuestaDTO<CategoriaInventarioDTO>> GuardarCategoria(CategoriaInventarioGuardarDTO dto)
        {
            try
            {
                string nombre = InventarioReglas.Limpiar(dto.Nombre) ?? string.Empty;

                if (nombre.Length == 0)
                    return FallaCategoria("Indique el nombre del tipo de inventario", 1601);

                if (await _inventarioRepository.ExisteNombreCategoria(nombre, dto.IdCategoria > 0 ? dto.IdCategoria : null))
                    return FallaCategoria("Ya existe un tipo de inventario con ese nombre", 1601);

                CategoriaInventario categoria;

                if (dto.IdCategoria > 0)
                {
                    categoria = await _inventarioRepository.GetCategoria(dto.IdCategoria)
                        ?? throw new KeyNotFoundException();

                    categoria.Nombre = nombre;
                    await _inventarioRepository.ActualizarCategoria(categoria);
                }
                else
                {
                    categoria = new CategoriaInventario { Nombre = nombre, Activo = true };
                    await _inventarioRepository.CrearCategoria(categoria);
                }

                return RespuestaDTO<CategoriaInventarioDTO>.Exito(
                    ToDTO(categoria, 0),
                    dto.IdCategoria > 0 ? "Tipo de inventario actualizado" : "Tipo de inventario registrado");
            }
            catch (KeyNotFoundException)
            {
                return FallaCategoria("El tipo de inventario no existe", 1602);
            }
            catch (Exception)
            {
                return FallaCategoria("No fue posible guardar el tipo de inventario", 1610);
            }
        }

        public async Task<RespuestaDTO<CategoriaInventarioDTO>> CambiarEstadoCategoria(int idCategoria, bool activo)
        {
            try
            {
                var categoria = await _inventarioRepository.GetCategoria(idCategoria);

                if (categoria == null)
                    return FallaCategoria("El tipo de inventario no existe", 1602);

                if (categoria.Activo == activo)
                    return FallaCategoria(activo ? "El tipo ya está activo" : "El tipo ya está inactivo", 1603);

                categoria.Activo = activo;
                await _inventarioRepository.ActualizarCategoria(categoria);

                return RespuestaDTO<CategoriaInventarioDTO>.Exito(
                    ToDTO(categoria, 0),
                    activo
                        ? "Tipo de inventario activado"
                        : "Tipo de inventario desactivado; no se podrán registrar artículos nuevos en él");
            }
            catch (Exception)
            {
                return FallaCategoria("No fue posible cambiar el estado del tipo de inventario", 1610);
            }
        }

        // =========================================================
        // Artículos (MIVF-01-01 y 01-04 a 01-08)
        // =========================================================

        public async Task<RespuestaDTO<List<ArticuloInventarioDTO>>> BuscarArticulos(InventarioFiltroDTO filtro)
        {
            try
            {
                var criterio = new InventarioFiltro
                {
                    Texto = InventarioReglas.Limpiar(filtro.Texto),
                    IdCategoria = filtro.IdCategoria > 0 ? filtro.IdCategoria : null,
                    Activo = filtro.Estado switch
                    {
                        InventarioReglas.EstadoActivo => true,
                        InventarioReglas.EstadoInactivo => false,
                        _ => null
                    },
                    SoloBajoMinimo = filtro.SoloBajoMinimo
                };

                bool sinFiltros = criterio.Texto == null && criterio.IdCategoria == null
                                  && criterio.Activo == null && !criterio.SoloBajoMinimo; ;

                var hoy = InventarioReglas.Hoy();
                var lista = (await _inventarioRepository.BuscarArticulos(criterio, LimiteLista))
                    .Select(a => ToDTO(a, hoy))
                    .ToList();

                string mensaje = lista.Count > 0
                    ? "Artículos cargados"
                     : criterio.SoloBajoMinimo
                        ? "No hay artículos por debajo de la cantidad mínima"
                        : sinFiltros
                            ? "No hay recursos en inventario"
                            : "No se encontraron artículos con esos criterios";

                return RespuestaDTO<List<ArticuloInventarioDTO>>.Exito(lista, mensaje);
            }
            catch (Exception)
            {
                return RespuestaDTO<List<ArticuloInventarioDTO>>.Error("No fue posible cargar el inventario", 1619);
            }
        }

        public async Task<RespuestaDTO<ArticuloInventarioDTO>> GuardarArticulo(int idUsuario, ArticuloInventarioGuardarDTO dto)
        {
            try
            {
                bool esNuevo = dto.IdInventario <= 0;
                var hoy = InventarioReglas.Hoy();

                // Tipo obligatorio y disponible (MIVF-01-01)
                if (dto.IdCategoria <= 0)
                    return FallaArticulo("Seleccione el tipo de inventario antes de continuar", 1611);

                var categoria = await _inventarioRepository.GetCategoria(dto.IdCategoria);

                if (categoria == null)
                    return FallaArticulo("El tipo de inventario no existe", 1611);

                InventarioEntity? existente = null;

                if (!esNuevo)
                {
                    existente = await _inventarioRepository.GetArticulo(dto.IdInventario)
                        ?? throw new KeyNotFoundException();
                }

                bool cambioDeTipo = esNuevo || existente!.IdCategoria != dto.IdCategoria;

                if (!categoria.Activo && cambioDeTipo)
                    return FallaArticulo("El tipo seleccionado está desactivado; la clasificación no está disponible", 1612);

                // Datos del artículo (MIVF-01-04)
                string nombre = InventarioReglas.Limpiar(dto.Nombre) ?? string.Empty;
                string unidad = InventarioReglas.Limpiar(dto.UnidadMedida) ?? string.Empty;

                if (nombre.Length == 0)
                    return FallaArticulo("Indique el nombre del artículo", 1613);

                if (unidad.Length == 0)
                    return FallaArticulo("Indique la unidad de medida", 1613);

                if (dto.CantidadMinima < 0 || dto.CantidadMinima > InventarioReglas.MaxCantidad)
                    return FallaArticulo("La cantidad mínima no es válida", 1613);

                if (esNuevo && (dto.CantidadInicial < 0 || dto.CantidadInicial > InventarioReglas.MaxCantidad))
                    return FallaArticulo("La cantidad inicial no es válida", 1613);

                bool venceCambio = esNuevo || existente!.FechaVencimiento != dto.FechaVencimiento;

                if (dto.FechaVencimiento.HasValue && venceCambio && dto.FechaVencimiento.Value < hoy)
                    return FallaArticulo("La fecha de vencimiento no puede ser anterior a hoy", 1613);

                if (await _inventarioRepository.ExisteNombreArticulo(
                        dto.IdCategoria, nombre, esNuevo ? null : dto.IdInventario))
                    return FallaArticulo("Ya existe un artículo con ese nombre en el tipo seleccionado", 1614);

                InventarioEntity articulo;

                if (esNuevo)
                {
                    articulo = new InventarioEntity
                    {
                        IdCategoria = dto.IdCategoria,
                        Nombre = nombre,
                        Descripcion = InventarioReglas.Limpiar(dto.Descripcion),
                        Cantidad = dto.CantidadInicial,
                        UnidadMedida = unidad,
                        FechaRegistro = DateTime.Now,
                        FechaVencimiento = dto.FechaVencimiento,
                        CantidadMinima = dto.CantidadMinima,
                        Activo = true
                    };

                    await _inventarioRepository.CrearArticulo(articulo, idUsuario);
                }
                else
                {
                    articulo = existente!;
                    articulo.IdCategoria = dto.IdCategoria;
                    articulo.Nombre = nombre;
                    articulo.Descripcion = InventarioReglas.Limpiar(dto.Descripcion);
                    articulo.UnidadMedida = unidad;
                    articulo.FechaVencimiento = dto.FechaVencimiento;
                    articulo.CantidadMinima = dto.CantidadMinima;
                    // La cantidad no se toca: solo cambia con movimientos

                    await _inventarioRepository.ActualizarArticulo(articulo);
                }

                var guardado = await _inventarioRepository.GetArticulo(articulo.IdInventario)
                    ?? throw new KeyNotFoundException();

                return RespuestaDTO<ArticuloInventarioDTO>.Exito(
                    ToDTO(guardado, hoy),
                    esNuevo ? "Artículo registrado" : "Artículo actualizado");
            }
            catch (KeyNotFoundException)
            {
                return FallaArticulo("El artículo no existe", 1615);
            }
            catch (Exception)
            {
                return FallaArticulo("No fue posible guardar el artículo", 1619);
            }
        }

        public async Task<RespuestaDTO<ArticuloInventarioDTO>> CambiarEstadoArticulo(int idInventario, bool activo)
        {
            try
            {
                var articulo = await _inventarioRepository.GetArticulo(idInventario);

                if (articulo == null)
                    return FallaArticulo("El artículo no existe", 1615);

                if (articulo.Activo == activo)
                    return FallaArticulo(activo ? "El artículo ya está activo" : "El artículo ya está inactivo", 1616);

                if (activo && articulo.Categoria is { Activo: false })
                    return FallaArticulo("No se puede activar: su tipo de inventario está desactivado", 1612);

                articulo.Activo = activo;
                await _inventarioRepository.ActualizarArticulo(articulo);

                return RespuestaDTO<ArticuloInventarioDTO>.Exito(
                    ToDTO(articulo, InventarioReglas.Hoy()),
                    activo ? "Artículo activado" : "Artículo desactivado; ya no se podrá usar en movimientos");
            }
            catch (Exception)
            {
                return FallaArticulo("No fue posible cambiar el estado del artículo", 1619);
            }
        }


        // =========================================================
        // Movimientos (MIVF-01-09, 01-10 y 01-11)
        // =========================================================

        public async Task<RespuestaDTO<MovimientoInventarioDTO>> RegistrarMovimiento(
            int idUsuario, MovimientoInventarioGuardarDTO dto)
        {
            try
            {
                string tipo = dto.Tipo?.Trim() ?? string.Empty;

                if (tipo != TipoMovimientoInventario.Entrada && tipo != TipoMovimientoInventario.Salida)
                    return FallaMovimiento("El tipo de movimiento no es válido", 1621);

                bool esSalida = tipo == TipoMovimientoInventario.Salida;

                var articulo = await _inventarioRepository.GetArticulo(dto.IdInventario);

                if (articulo == null)
                    return FallaMovimiento("El artículo no existe", 1615);

                if (!articulo.Activo)
                    return FallaMovimiento("El artículo está inactivo; no se pueden registrar movimientos", 1622);

                if (dto.Cantidad <= 0 || dto.Cantidad > InventarioReglas.MaxCantidad)
                    return FallaMovimiento("Indique una cantidad válida, mayor a cero", 1623);

                string? motivo = InventarioReglas.Limpiar(dto.Motivo);
                string? nombre = null;
                string? cedula = null;

                if (esSalida)
                {
                    nombre = InventarioReglas.Limpiar(dto.NombreResponsable);
                    cedula = InventarioReglas.Limpiar(dto.CedulaResponsable)?.Replace(" ", "");

                    if (nombre == null)
                        return FallaMovimiento("Indique el nombre de la persona que retira", 1624);

                    if (cedula == null || !InventarioReglas.CedulaValida(cedula))
                        return FallaMovimiento("Indique una cédula válida (letras, números y guiones)", 1624);

                    if (motivo == null)
                        return FallaMovimiento("Indique el motivo de la salida", 1624);

                    if (dto.Cantidad > articulo.Cantidad)
                        return FallaMovimiento(
                            $"Existencia insuficiente: solo hay {articulo.Cantidad} {articulo.UnidadMedida} disponibles", 1625);
                }
                else
                {
                    // Entrada: el motivo es opcional (compra, donación...); no hay responsable
                    if ((long)articulo.Cantidad + dto.Cantidad > InventarioReglas.MaxExistencia)
                        return FallaMovimiento("La entrada supera la existencia máxima permitida", 1623);
                }

                var movimiento = await _inventarioRepository.RegistrarMovimiento(new InventarioMovimiento
                {
                    IdInventario = articulo.IdInventario,
                    Tipo = tipo,
                    Cantidad = dto.Cantidad,
                    Fecha = DateTime.Now,
                    Motivo = motivo,
                    NombreResponsable = nombre,
                    CedulaResponsable = cedula,
                    IdUsuarioRegistra = idUsuario
                });

                if (movimiento == null)
                    return FallaMovimiento(
                        "La existencia del artículo cambió mientras registraba el movimiento. Actualice e intente de nuevo", 1625);

                return RespuestaDTO<MovimientoInventarioDTO>.Exito(
                    new MovimientoInventarioDTO
                    {
                        IdMovimiento = movimiento.IdMovimiento,
                        IdInventario = articulo.IdInventario,
                        Articulo = articulo.Nombre,
                        Tipo = movimiento.Tipo,
                        Cantidad = movimiento.Cantidad,
                        ExistenciaResultante = movimiento.ExistenciaResultante,
                        Fecha = movimiento.Fecha,
                        Motivo = movimiento.Motivo,
                        NombreResponsable = movimiento.NombreResponsable,
                        CedulaResponsable = movimiento.CedulaResponsable
                    },
                    esSalida
                        ? $"Salida registrada. Quedan {movimiento.ExistenciaResultante} {articulo.UnidadMedida}"
                        : $"Entrada registrada. Existencia actual: {movimiento.ExistenciaResultante} {articulo.UnidadMedida}");
            }
            catch (Exception)
            {
                return FallaMovimiento("No fue posible registrar el movimiento", 1629);
            }
        }


        // =========================================================
        // Historial de movimientos (MIVF-01-12)
        // =========================================================

        public async Task<RespuestaDTO<HistorialArticuloDTO>> GetHistorial(int idInventario)
        {
            try
            {
                var articulo = await _inventarioRepository.GetArticulo(idInventario);

                if (articulo == null)
                    return RespuestaDTO<HistorialArticuloDTO>.Error("El artículo no existe", 1615);

                var movimientos = await _inventarioRepository.GetMovimientos(idInventario, LimiteLista);

                var historial = new HistorialArticuloDTO
                {
                    Articulo = ToDTO(articulo, InventarioReglas.Hoy()),
                    TotalEntradas = movimientos
                        .Where(m => m.Tipo == TipoMovimientoInventario.Entrada).Sum(m => m.Cantidad),
                    TotalSalidas = movimientos
                        .Where(m => m.Tipo == TipoMovimientoInventario.Salida).Sum(m => m.Cantidad),
                    Movimientos = movimientos.Select(m => new MovimientoInventarioDTO
                    {
                        IdMovimiento = m.IdMovimiento,
                        IdInventario = m.IdInventario,
                        Articulo = articulo.Nombre,
                        Tipo = m.Tipo,
                        Cantidad = m.Cantidad,
                        ExistenciaResultante = m.ExistenciaResultante,
                        Fecha = m.Fecha,
                        Motivo = m.Motivo,
                        NombreResponsable = m.NombreResponsable,
                        CedulaResponsable = m.CedulaResponsable,
                        UsuarioRegistra = m.UsuarioRegistra == null
                            ? null
                            : $"{m.UsuarioRegistra.Nombre} {m.UsuarioRegistra.Apellido}".Trim()
                    }).ToList()
                };

                return RespuestaDTO<HistorialArticuloDTO>.Exito(
                    historial,
                    movimientos.Count == 0
                        ? "El artículo todavía no tiene movimientos"
                        : "Historial cargado");
            }
            catch (Exception)
            {
                return RespuestaDTO<HistorialArticuloDTO>.Error("No fue posible cargar el historial", 1629);
            }
        }


        // =========================================================
        // Reportes y estadísticas (MIVF-01-16, 01-17 y 01-18)
        // =========================================================

        public async Task<RespuestaDTO<ReporteExistenciasDTO>> GetReporteExistencias(ReporteExistenciasFiltroDTO filtro)
        {
            try
            {
                int? idCategoria = filtro.IdCategoria > 0 ? filtro.IdCategoria : null;
                string nombreCategoria = "Todos los tipos";

                if (idCategoria.HasValue)
                {
                    var categoria = await _inventarioRepository.GetCategoria(idCategoria.Value);

                    if (categoria == null)
                        return RespuestaDTO<ReporteExistenciasDTO>.Error("El tipo de inventario no existe", 1602);

                    nombreCategoria = categoria.Nombre;
                }

                var hoy = InventarioReglas.Hoy();

                var articulos = (await _inventarioRepository.BuscarArticulos(
                        new InventarioFiltro
                        {
                            IdCategoria = idCategoria,
                            Activo = filtro.IncluirInactivos ? null : true
                        },
                        LimiteReporte))
                    .Select(a => ToDTO(a, hoy))
                    .OrderBy(a => a.Categoria)
                    .ThenBy(a => a.Nombre)
                    .ToList();

                var reporte = new ReporteExistenciasDTO
                {
                    Categoria = nombreCategoria,
                    GeneradoEl = DateTime.Now,
                    TotalArticulos = articulos.Count,
                    BajoMinimo = articulos.Count(a => a.Activo && a.BajoMinimo),
                    Vencidos = articulos.Count(a => a.Vencido),
                    Articulos = articulos
                };

                return RespuestaDTO<ReporteExistenciasDTO>.Exito(
                    reporte,
                    articulos.Count == 0
                        ? "No hay artículos registrados para generar el reporte"
                        : "Reporte generado");
            }
            catch (Exception)
            {
                return RespuestaDTO<ReporteExistenciasDTO>.Error("No fue posible generar el reporte de existencias", 1639);
            }
        }

        public async Task<RespuestaDTO<byte[]>> ExportarExistencias(ReporteExistenciasFiltroDTO filtro)
        {
            var respuesta = await GetReporteExistencias(filtro);

            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return RespuestaDTO<byte[]>.Error(respuesta.Mensaje, respuesta.Codigo);

            if (respuesta.Dato.TotalArticulos == 0)
                return RespuestaDTO<byte[]>.Error("No hay artículos para exportar", 1631);

            try
            {
                return RespuestaDTO<byte[]>.Exito(
                    InventarioReporteExcel.Existencias(respuesta.Dato), "Reporte exportado");
            }
            catch (Exception)
            {
                return RespuestaDTO<byte[]>.Error("No fue posible exportar el reporte", 1639);
            }
        }

        public async Task<RespuestaDTO<ReporteMovimientosDTO>> GetReporteMovimientos(ReporteMovimientosFiltroDTO filtro)
        {
            try
            {
                var rango = ValidarRango(filtro.Desde, filtro.Hasta);

                if (rango.Error != null)
                    return RespuestaDTO<ReporteMovimientosDTO>.Error(rango.Error, 1632);

                string? tipo = InventarioReglas.Limpiar(filtro.Tipo);

                if (tipo != null && tipo != TipoMovimientoInventario.Entrada && tipo != TipoMovimientoInventario.Salida)
                    return RespuestaDTO<ReporteMovimientosDTO>.Error("El tipo de movimiento no es válido", 1621);

                int? idCategoria = filtro.IdCategoria > 0 ? filtro.IdCategoria : null;
                string nombreCategoria = "Todos los tipos";

                if (idCategoria.HasValue)
                {
                    var categoria = await _inventarioRepository.GetCategoria(idCategoria.Value);

                    if (categoria == null)
                        return RespuestaDTO<ReporteMovimientosDTO>.Error("El tipo de inventario no existe", 1602);

                    nombreCategoria = categoria.Nombre;
                }

                var movimientos = await _inventarioRepository.GetMovimientosReporte(
                    rango.Desde, rango.HastaExclusivo, idCategoria, tipo, LimiteReporte + 1);

                bool truncado = movimientos.Count > LimiteReporte;

                if (truncado)
                    movimientos = movimientos.Take(LimiteReporte).ToList();

                var reporte = new ReporteMovimientosDTO
                {
                    Desde = filtro.Desde!.Value,
                    Hasta = filtro.Hasta!.Value,
                    Categoria = nombreCategoria,
                    Tipo = tipo == null ? "Entradas y salidas" : tipo == TipoMovimientoInventario.Entrada ? "Solo entradas" : "Solo salidas",
                    TotalEntradas = movimientos.Count(m => m.Tipo == TipoMovimientoInventario.Entrada),
                    TotalSalidas = movimientos.Count(m => m.Tipo == TipoMovimientoInventario.Salida),
                    Truncado = truncado,
                    Movimientos = movimientos.Select(m => new MovimientoReporteDTO
                    {
                        IdMovimiento = m.IdMovimiento,
                        IdInventario = m.IdInventario,
                        Articulo = m.Inventario?.Nombre ?? string.Empty,
                        Categoria = m.Inventario?.Categoria?.Nombre ?? string.Empty,
                        UnidadMedida = m.Inventario?.UnidadMedida ?? string.Empty,
                        Tipo = m.Tipo,
                        Cantidad = m.Cantidad,
                        ExistenciaResultante = m.ExistenciaResultante,
                        Fecha = m.Fecha,
                        Motivo = m.Motivo,
                        NombreResponsable = m.NombreResponsable,
                        CedulaResponsable = m.CedulaResponsable,
                        UsuarioRegistra = m.UsuarioRegistra == null
                            ? null
                            : $"{m.UsuarioRegistra.Nombre} {m.UsuarioRegistra.Apellido}".Trim()
                    }).ToList()
                };

                return RespuestaDTO<ReporteMovimientosDTO>.Exito(
                    reporte,
                    movimientos.Count == 0
                        ? "No hay movimientos en el período seleccionado"
                        : truncado
                            ? $"Se muestran los {LimiteReporte} movimientos más recientes; acote el rango de fechas para ver el resto"
                            : "Reporte generado");
            }
            catch (Exception)
            {
                return RespuestaDTO<ReporteMovimientosDTO>.Error("No fue posible generar el reporte de movimientos", 1639);
            }
        }

        public async Task<RespuestaDTO<byte[]>> ExportarMovimientos(ReporteMovimientosFiltroDTO filtro)
        {
            var respuesta = await GetReporteMovimientos(filtro);

            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return RespuestaDTO<byte[]>.Error(respuesta.Mensaje, respuesta.Codigo);

            if (respuesta.Dato.Movimientos.Count == 0)
                return RespuestaDTO<byte[]>.Error("No hay movimientos para exportar", 1631);

            try
            {
                return RespuestaDTO<byte[]>.Exito(
                    InventarioReporteExcel.Movimientos(respuesta.Dato), "Reporte exportado");
            }
            catch (Exception)
            {
                return RespuestaDTO<byte[]>.Error("No fue posible exportar el reporte", 1639);
            }
        }

        public async Task<RespuestaDTO<EstadisticasInventarioDTO>> GetEstadisticas(ReporteMovimientosFiltroDTO filtro)
        {
            try
            {
                var hoy = InventarioReglas.Hoy();

                // Sin fechas: los últimos 6 meses, incluido el actual
                DateOnly hasta = filtro.Hasta ?? hoy;
                DateOnly desde = filtro.Desde ?? new DateOnly(hasta.Year, hasta.Month, 1).AddMonths(-5);

                var rango = ValidarRango(desde, hasta);

                if (rango.Error != null)
                    return RespuestaDTO<EstadisticasInventarioDTO>.Error(rango.Error, 1632);

                int meses = (hasta.Year - desde.Year) * 12 + hasta.Month - desde.Month + 1;

                if (meses > MaxMesesEstadisticas)
                    return RespuestaDTO<EstadisticasInventarioDTO>.Error(
                        $"El rango máximo para las estadísticas es de {MaxMesesEstadisticas} meses", 1632);

                int? idCategoria = filtro.IdCategoria > 0 ? filtro.IdCategoria : null;

                var resumen = await _inventarioRepository.GetResumenMensual(rango.Desde, rango.HastaExclusivo, idCategoria);

                var porMes = new List<PuntoMensualInventarioDTO>();
                var cursor = new DateOnly(desde.Year, desde.Month, 1);

                for (int i = 0; i < meses; i++)
                {
                    var mes = cursor.AddMonths(i);

                    porMes.Add(new PuntoMensualInventarioDTO
                    {
                        Etiqueta = $"{mes.Month:00}/{mes.Year}",
                        Entradas = resumen
                            .Where(r => r.Anio == mes.Year && r.Mes == mes.Month && r.Tipo == TipoMovimientoInventario.Entrada)
                            .Sum(r => r.Movimientos),
                        Salidas = resumen
                            .Where(r => r.Anio == mes.Year && r.Mes == mes.Month && r.Tipo == TipoMovimientoInventario.Salida)
                            .Sum(r => r.Movimientos)
                    });
                }

                var topSalidas = await _inventarioRepository.GetTopArticulos(
                    rango.Desde, rango.HastaExclusivo, idCategoria, TipoMovimientoInventario.Salida, 5);
                var topEntradas = await _inventarioRepository.GetTopArticulos(
                    rango.Desde, rango.HastaExclusivo, idCategoria, TipoMovimientoInventario.Entrada, 5);

                var estadisticas = new EstadisticasInventarioDTO
                {
                    Desde = desde,
                    Hasta = hasta,
                    TotalEntradas = porMes.Sum(p => p.Entradas),
                    TotalSalidas = porMes.Sum(p => p.Salidas),
                    PorMes = porMes,
                    TopSalidas = topSalidas.Select(ToDTO).ToList(),
                    TopEntradas = topEntradas.Select(ToDTO).ToList()
                };

                return RespuestaDTO<EstadisticasInventarioDTO>.Exito(
                    estadisticas,
                    estadisticas.TotalEntradas + estadisticas.TotalSalidas == 0
                        ? "No hay movimientos registrados en el período"
                        : "Estadísticas cargadas");
            }
            catch (Exception)
            {
                return RespuestaDTO<EstadisticasInventarioDTO>.Error("No fue posible cargar las estadísticas", 1639);
            }
        }



        // ---- Auxiliares ----

        private static CategoriaInventarioDTO ToDTO(CategoriaInventario c, int totalArticulos) => new()
        {
            IdCategoria = c.IdCategoria,
            Nombre = c.Nombre,
            Activo = c.Activo,
            TotalArticulos = totalArticulos
        };

        private static ArticuloInventarioDTO ToDTO(InventarioEntity a, DateOnly hoy) => new()
        {
            IdInventario = a.IdInventario,
            IdCategoria = a.IdCategoria,
            Categoria = a.Categoria?.Nombre ?? string.Empty,
            CategoriaActiva = a.Categoria?.Activo ?? false,
            Nombre = a.Nombre,
            Descripcion = a.Descripcion,
            Cantidad = a.Cantidad,
            UnidadMedida = a.UnidadMedida,
            FechaRegistro = a.FechaRegistro,
            FechaVencimiento = a.FechaVencimiento,
            CantidadMinima = a.CantidadMinima,
            Activo = a.Activo,
            BajoMinimo = a.CantidadMinima > 0 && a.Cantidad < a.CantidadMinima,
            Vencido = a.FechaVencimiento.HasValue && a.FechaVencimiento.Value < hoy
        };

        private static RespuestaDTO<CategoriaInventarioDTO> FallaCategoria(string mensaje, int codigo) =>
            RespuestaDTO<CategoriaInventarioDTO>.Error(mensaje, codigo);

        private static RespuestaDTO<ArticuloInventarioDTO> FallaArticulo(string mensaje, int codigo) =>
            RespuestaDTO<ArticuloInventarioDTO>.Error(mensaje, codigo);

        private static RespuestaDTO<MovimientoInventarioDTO> FallaMovimiento(string mensaje, int codigo) =>
            RespuestaDTO<MovimientoInventarioDTO>.Error(mensaje, codigo);

        private static (DateTime Desde, DateTime HastaExclusivo, string? Error) ValidarRango(DateOnly? desde, DateOnly? hasta)
        {
            if (!desde.HasValue || !hasta.HasValue)
                return (default, default, "Indique la fecha inicial y la fecha final del período");

            if (desde.Value > hasta.Value)
                return (default, default, "La fecha inicial no puede ser posterior a la fecha final");

            return (desde.Value.ToDateTime(TimeOnly.MinValue),
                    hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue),
                    null);
        }

        private static ArticuloMovimientoResumenDTO ToDTO(ResumenArticuloMovimiento r) => new()
        {
            IdInventario = r.IdInventario,
            Articulo = r.Articulo,
            Categoria = r.Categoria,
            UnidadMedida = r.UnidadMedida,
            Movimientos = r.Movimientos,
            Unidades = r.Unidades
        };
    }
}