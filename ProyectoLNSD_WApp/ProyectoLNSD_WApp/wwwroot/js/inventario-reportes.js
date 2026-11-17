// Reportes y estadísticas de inventario (MIVF-01-16, 01-17 y 01-18).

$(function () {
    const hoy = new Date();
    const primeroDeMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);

    $('#mDesde').val(fechaIso(primeroDeMes));
    $('#mHasta').val(fechaIso(hoy));

    cargarCategorias().then(function () {
        generarExistencias();
    });

    $('#btnGenerarExistencias').on('click', generarExistencias);
    $('#btnExcelExistencias').on('click', function () {
        descargarExcel('/Inventario/ExportarExistencias?' + $.param(filtroExistencias()),
            `Inventario_Existencias_${fechaIso(new Date())}.xlsx`);
    });

    $('#btnGenerarMovimientos').on('click', generarMovimientos);
    $('#btnExcelMovimientos').on('click', function () {
        descargarExcel('/Inventario/ExportarMovimientos?' + $.param(filtroMovimientos()),
            `Inventario_Movimientos_${fechaIso(new Date())}.xlsx`);
    });

    if (window.inventarioReportes && window.inventarioReportes.estadisticas) {
        $('#btnGenerarEstadisticas').on('click', generarEstadisticas);
        $('#tabEstadisticas').one('shown.bs.tab', generarEstadisticas);
    }
});

function cargarCategorias() {
    return $.get('/Inventario/GetCategorias', { incluirInactivas: true })
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];
            const opciones = lista.map(c =>
                `<option value="${Number(c.idCategoria)}">${escapeHtml(c.nombre)}${c.activo ? '' : ' (inactivo)'}</option>`).join('');

            $('#eCategoria, #mCategoria, #sCategoria').each(function () {
                $(this).html('<option value="">Todos los tipos</option>' + opciones);
            });
        })
        .fail(() => mostrarMensaje('No fue posible cargar los tipos de inventario.', 'danger'));
}

// Descarga el Excel; si el servidor responde con un mensaje (JSON), lo muestra en vez de navegar
async function descargarExcel(url, nombre) {
    try {
        const resp = await fetch(url, { credentials: 'same-origin' });
        const tipo = resp.headers.get('content-type') || '';

        if (tipo.includes('application/json')) {
            const r = await resp.json();
            mostrarMensaje(r.mensaje || 'No hay datos para exportar.', 'warning');
            return;
        }

        if (!resp.ok) {
            mostrarMensaje('No fue posible exportar el reporte.', 'danger');
            return;
        }

        const blob = await resp.blob();
        const enlace = document.createElement('a');
        enlace.href = URL.createObjectURL(blob);
        enlace.download = nombre;
        document.body.appendChild(enlace);
        enlace.click();
        enlace.remove();
        URL.revokeObjectURL(enlace.href);
    } catch (e) {
        mostrarMensaje('No fue posible exportar el reporte.', 'danger');
    }
}

// ---------------- Existencias ----------------

function filtroExistencias() {
    return {
        idCategoria: $('#eCategoria').val(),
        incluirInactivos: $('#eInactivos').is(':checked')
    };
}

function generarExistencias() {
    const $cuerpo = $('#tblExistencias tbody');
    $cuerpo.html('<tr><td colspan="5" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Inventario/GetReporteExistencias', filtroExistencias())
        .done(function (r) {
            if (!r.esCorrecto || !r.dato || !r.dato.articulos.length) {
                $('#eResumen').addClass('d-none');
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="5" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const d = r.dato;

            $('#eResumen').removeClass('d-none').html(`
                <strong>${escapeHtml(d.categoria)}</strong> ·
                ${Number(d.totalArticulos)} artículo(s) ·
                <span class="${d.bajoMinimo ? 'text-warning-emphasis fw-semibold' : ''}">${Number(d.bajoMinimo)} bajo el mínimo</span> ·
                <span class="${d.vencidos ? 'text-danger fw-semibold' : ''}">${Number(d.vencidos)} vencido(s)</span>`);

            let categoriaActual = null;
            let html = '';

            d.articulos.forEach(a => {
                if (a.categoria !== categoriaActual) {
                    categoriaActual = a.categoria;
                    html += `<tr class="table-light"><td colspan="5" class="fw-semibold">${escapeHtml(a.categoria)}</td></tr>`;
                }

                html += `
                    <tr class="${a.activo ? '' : 'text-muted'}">
                        <td>
                            <div>${escapeHtml(a.nombre)}</div>
                            ${a.descripcion ? `<div class="small text-muted">${escapeHtml(a.descripcion)}</div>` : ''}
                        </td>
                        <td class="text-end">
                            ${Number(a.cantidad).toLocaleString('es-CR')} <span class="small text-muted">${escapeHtml(a.unidadMedida)}</span>
                            ${a.activo && a.bajoMinimo ? '<div><span class="badge text-bg-warning">Bajo el mínimo</span></div>' : ''}
                        </td>
                        <td class="text-end">${Number(a.cantidadMinima) > 0 ? Number(a.cantidadMinima).toLocaleString('es-CR') : '<span class="text-muted">—</span>'}</td>
                        <td>${a.fechaVencimiento
                            ? `${fechaCorta(a.fechaVencimiento)}${a.vencido ? ' <span class="badge text-bg-danger">Vencido</span>' : ''}`
                            : '<span class="text-muted">—</span>'}</td>
                        <td>${a.activo
                            ? '<span class="badge text-bg-success">Activo</span>'
                            : '<span class="badge text-bg-secondary">Inactivo</span>'}</td>
                    </tr>`;
            });

            $cuerpo.html(html);
        })
        .fail(() => {
            $('#eResumen').addClass('d-none');
            $cuerpo.html('<tr><td colspan="5" class="text-center text-danger">No fue posible generar el reporte.</td></tr>');
        });
}

// ---------------- Movimientos ----------------

function filtroMovimientos() {
    return {
        desde: $('#mDesde').val(),
        hasta: $('#mHasta').val(),
        idCategoria: $('#mCategoria').val(),
        tipo: $('#mTipoMov').val()
    };
}

function generarMovimientos() {
    const filtro = filtroMovimientos();

    if (!filtro.desde || !filtro.hasta) {
        mostrarMensaje('Indique la fecha inicial y la fecha final del período.', 'warning');
        return;
    }

    if (filtro.desde > filtro.hasta) {
        mostrarMensaje('La fecha inicial no puede ser posterior a la fecha final.', 'warning');
        return;
    }

    const $cuerpo = $('#tblMovimientos tbody');
    $cuerpo.html('<tr><td colspan="8" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Inventario/GetReporteMovimientos', filtro)
        .done(function (r) {
            if (!r.esCorrecto || !r.dato || !r.dato.movimientos.length) {
                $('#mResumenReporte').addClass('d-none');
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="8" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const d = r.dato;

            $('#mResumenReporte').removeClass('d-none').html(`
                ${fechaCorta(d.desde)} al ${fechaCorta(d.hasta)} · ${escapeHtml(d.categoria)} · ${escapeHtml(d.tipo)} —
                <strong>${Number(d.totalEntradas)}</strong> entrada(s) y <strong>${Number(d.totalSalidas)}</strong> salida(s)
                ${d.truncado ? `<div class="text-warning-emphasis mt-1"><i class="bi bi-exclamation-triangle"></i> ${escapeHtml(r.mensaje)}</div>` : ''}`);

            $cuerpo.html(d.movimientos.map(m => `
                <tr>
                    <td class="text-nowrap">${escapeHtml(new Date(m.fecha).toLocaleString('es-CR'))}</td>
                    <td>${m.tipo === 'Entrada'
                        ? '<span class="badge text-bg-success">Entrada</span>'
                        : '<span class="badge text-bg-warning">Salida</span>'}</td>
                    <td>
                        <div>${escapeHtml(m.articulo)}</div>
                        <div class="small text-muted">${escapeHtml(m.categoria)}</div>
                    </td>
                    <td class="text-end">
                        ${m.tipo === 'Entrada' ? '+' : '−'}${Number(m.cantidad).toLocaleString('es-CR')}
                        <span class="small text-muted">${escapeHtml(m.unidadMedida)}</span>
                    </td>
                    <td class="text-end">${Number(m.existenciaResultante).toLocaleString('es-CR')}</td>
                    <td>${m.nombreResponsable
                        ? `${escapeHtml(m.nombreResponsable)}<div class="small text-muted">${escapeHtml(m.cedulaResponsable || '')}</div>`
                        : '<span class="text-muted">—</span>'}</td>
                    <td>${m.motivo ? escapeHtml(m.motivo) : '<span class="text-muted">—</span>'}</td>
                    <td>${escapeHtml(m.usuarioRegistra || '')}</td>
                </tr>`).join(''));
        })
        .fail(() => {
            $('#mResumenReporte').addClass('d-none');
            $cuerpo.html('<tr><td colspan="8" class="text-center text-danger">No fue posible generar el reporte.</td></tr>');
        });
}

// ---------------- Estadísticas ----------------

function generarEstadisticas() {
    const $contenido = $('#sContenido');
    $contenido.html('<div class="text-center text-muted py-4">Cargando...</div>');

    $.get('/Inventario/GetEstadisticas', {
        desde: $('#sDesde').val(),
        hasta: $('#sHasta').val(),
        idCategoria: $('#sCategoria').val()
    })
        .done(function (r) {
            if (!r.esCorrecto || !r.dato) {
                $contenido.html(`<div class="alert alert-danger">${escapeHtml(r.mensaje)}</div>`);
                return;
            }

            const d = r.dato;

            // Si no se eligieron fechas, se muestran las que usó el servidor
            $('#sDesde').val(String(d.desde).substring(0, 10));
            $('#sHasta').val(String(d.hasta).substring(0, 10));

            if (d.totalEntradas + d.totalSalidas === 0) {
                $contenido.html(`<div class="alert alert-light border text-center">${escapeHtml(r.mensaje)}</div>`);
                return;
            }

            const maximo = Math.max(1, ...d.porMes.map(p => Math.max(p.entradas, p.salidas)));

            const barras = d.porMes.map(p => `
                <div class="row align-items-center g-2 mb-2">
                    <div class="col-3 col-md-2 small text-muted">${escapeHtml(p.etiqueta)}</div>
                    <div class="col-9 col-md-10">
                        <div class="d-flex align-items-center mb-1">
                            <div class="bg-success rounded" style="height:14px;width:${(p.entradas / maximo * 100).toFixed(1)}%;min-width:${p.entradas ? 3 : 0}px"></div>
                            <span class="small ms-2">${Number(p.entradas)}</span>
                        </div>
                        <div class="d-flex align-items-center">
                            <div class="bg-warning rounded" style="height:14px;width:${(p.salidas / maximo * 100).toFixed(1)}%;min-width:${p.salidas ? 3 : 0}px"></div>
                            <span class="small ms-2">${Number(p.salidas)}</span>
                        </div>
                    </div>
                </div>`).join('');

            $contenido.html(`
                <div class="row g-3 mb-3">
                    <div class="col-md-6">
                        <div class="card border-success"><div class="card-body text-center">
                            <div class="display-6 text-success">${Number(d.totalEntradas).toLocaleString('es-CR')}</div>
                            <div class="small text-muted">Entradas registradas</div>
                        </div></div>
                    </div>
                    <div class="col-md-6">
                        <div class="card border-warning"><div class="card-body text-center">
                            <div class="display-6 text-warning-emphasis">${Number(d.totalSalidas).toLocaleString('es-CR')}</div>
                            <div class="small text-muted">Salidas registradas</div>
                        </div></div>
                    </div>
                </div>

                <div class="card mb-3">
                    <div class="card-header"><h2 class="h6 mb-0">Movimientos por mes</h2></div>
                    <div class="card-body">
                        <div class="small mb-3">
                            <span class="badge text-bg-success">Entradas</span>
                            <span class="badge text-bg-warning">Salidas</span>
                            <span class="text-muted ms-2">Cantidad de movimientos registrados</span>
                        </div>
                        ${barras}
                    </div>
                </div>

                <div class="row g-3">
                    <div class="col-lg-6">${tablaTop('Artículos con más salidas', d.topSalidas)}</div>
                    <div class="col-lg-6">${tablaTop('Artículos con más entradas', d.topEntradas)}</div>
                </div>`);
        })
        .fail(() => $contenido.html('<div class="alert alert-danger">No fue posible cargar las estadísticas.</div>'));
}

function tablaTop(titulo, lista) {
    const filas = lista.length
        ? lista.map(a => `
            <tr>
                <td>
                    <div>${escapeHtml(a.articulo)}</div>
                    <div class="small text-muted">${escapeHtml(a.categoria)}</div>
                </td>
                <td class="text-end">${Number(a.movimientos)}</td>
                <td class="text-end">${Number(a.unidades).toLocaleString('es-CR')} <span class="small text-muted">${escapeHtml(a.unidadMedida)}</span></td>
            </tr>`).join('')
        : '<tr><td colspan="3" class="text-center text-muted">Sin movimientos de este tipo.</td></tr>';

    return `
        <div class="card h-100">
            <div class="card-header"><h2 class="h6 mb-0">${escapeHtml(titulo)}</h2></div>
            <div class="card-body">
                <table class="table table-sm align-middle mb-0">
                    <thead><tr><th>Artículo</th><th class="text-end">Movimientos</th><th class="text-end">Unidades</th></tr></thead>
                    <tbody>${filas}</tbody>
                </table>
            </div>
        </div>`;
}