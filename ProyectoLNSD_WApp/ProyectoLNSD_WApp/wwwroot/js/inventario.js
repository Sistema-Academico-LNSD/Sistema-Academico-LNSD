// Artículos de inventario (MIVF-01-01 y 01-04 a 01-08), movimientos (01-09 a 01-11),
// historial (01-12) y alertas de mínimo (01-14 y 01-15).

let categorias = [];
let articulos = [];
let temporizador = null;

$(function () {
    cargarCategorias().then(buscar);

    $('#fTexto').on('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(buscar, 300);
    });
    $('#fCategoria, #fEstado, #fBajoMinimo').on('change', buscar);
    $('#btnVerBajoMinimo').on('click', function () {
        $('#fBajoMinimo').prop('checked', true);
        buscar();
    });
    $('#btnLimpiar').on('click', function () {
        $('#fTexto').val('');
        $('#fCategoria').val('');
        $('#fEstado').val('Activo');
        $('#fBajoMinimo').prop('checked', false);
        buscar();
    });

    $('#btnNuevoArticulo').on('click', () => abrirArticulo(null));
    $('#btnGuardarArticulo').on('click', guardarArticulo);
    $('#formArticulo').on('submit', function (e) {
        e.preventDefault();
        guardarArticulo();
    });

    $('#btnGuardarMovimiento').on('click', guardarMovimiento);
    $('#formMovimiento').on('submit', function (e) {
        e.preventDefault();
        guardarMovimiento();
    });

    $(document).on('click', '[data-accion-articulo]', function () {
        const d = $(this).data();
        const articulo = articulos.find(a => a.idInventario === Number(d.idInventario));

        if (!articulo) return;

        if (d.accionArticulo === 'editar') abrirArticulo(articulo);
        else if (d.accionArticulo === 'entrada') abrirMovimiento(articulo, 'Entrada');
        else if (d.accionArticulo === 'salida') abrirMovimiento(articulo, 'Salida');
        else if (d.accionArticulo === 'historial') abrirHistorial(articulo);
        else cambiarEstado(articulo);
    });
});

function cargarCategorias() {
    return $.get('/Inventario/GetCategorias', { incluirInactivas: true })
        .done(function (r) {
            categorias = r.esCorrecto ? (r.dato || []) : [];

            $('#fCategoria').html('<option value="">Todos</option>' +
                categorias.map(c => `<option value="${Number(c.idCategoria)}">${escapeHtml(c.nombre)}${c.activo ? '' : ' (inactivo)'}</option>`).join(''));
        })
        .fail(() => mostrarMensaje('No fue posible cargar los tipos de inventario.', 'danger'));
}

function buscar() {
    const $cuerpo = $('#tblArticulos tbody');
    $cuerpo.html('<tr><td colspan="7" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Inventario/GetArticulos', {
        texto: $('#fTexto').val().trim(),
        idCategoria: $('#fCategoria').val(),
        estado: $('#fEstado').val(),
        soloBajoMinimo: $('#fBajoMinimo').is(':checked')
    })
        .done(function (r) {
            articulos = r.esCorrecto ? (r.dato || []) : [];
            actualizarAlertaMinimo();

            if (!articulos.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="7" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const permisos = window.inventarioPermisos || {};

            $cuerpo.html(articulos.map(a => `
                <tr class="${a.activo ? '' : 'text-muted'}">
                    <td>
                        <div class="fw-semibold">${escapeHtml(a.nombre)}</div>
                        ${a.descripcion ? `<div class="small text-muted">${escapeHtml(a.descripcion)}</div>` : ''}
                    </td>
                    <td>${escapeHtml(a.categoria)}</td>
                    <td class="text-end">
                        ${Number(a.cantidad).toLocaleString('es-CR')} <span class="small text-muted">${escapeHtml(a.unidadMedida)}</span>
                        ${a.bajoMinimo ? '<div><span class="badge text-bg-warning">Bajo el mínimo</span></div>' : ''}
                    </td>
                    <td class="text-end">${Number(a.cantidadMinima) > 0 ? Number(a.cantidadMinima).toLocaleString('es-CR') : '<span class="text-muted">—</span>'}</td>
                    <td>${a.fechaVencimiento
                        ? `${fechaCorta(a.fechaVencimiento)}${a.vencido ? ' <span class="badge text-bg-danger">Vencido</span>' : ''}`
                        : '<span class="text-muted">—</span>'}</td>
                    <td>${a.activo
                        ? '<span class="badge text-bg-success">Activo</span>'
                        : '<span class="badge text-bg-secondary">Inactivo</span>'}</td>
                    <td class="text-end text-nowrap">
                        <button type="button" class="btn btn-outline-secondary btn-sm me-1" title="Historial de movimientos"
                                data-accion-articulo="historial" data-id-inventario="${Number(a.idInventario)}">
                            <i class="bi bi-clock-history"></i>
                        </button>
                        ${permisos.crear && a.activo ? `
                        <div class="btn-group me-1">
                            <button type="button" class="btn btn-outline-success btn-sm" title="Registrar entrada"
                                    data-accion-articulo="entrada" data-id-inventario="${Number(a.idInventario)}">
                                <i class="bi bi-box-arrow-in-down"></i>
                            </button>
                            <button type="button" class="btn btn-outline-warning btn-sm" title="Registrar salida"
                                    data-accion-articulo="salida" data-id-inventario="${Number(a.idInventario)}">
                                <i class="bi bi-box-arrow-up"></i>
                            </button>
                        </div>` : ''}
                        ${permisos.editar ? `
                        <div class="btn-group">
                            <button type="button" class="btn btn-outline-primary btn-sm" title="Editar"
                                    data-accion-articulo="editar" data-id-inventario="${Number(a.idInventario)}">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-${a.activo ? 'danger' : 'success'} btn-sm"
                                    title="${a.activo ? 'Desactivar' : 'Activar'}"
                                    data-accion-articulo="estado" data-id-inventario="${Number(a.idInventario)}">
                                <i class="bi ${a.activo ? 'bi-slash-circle' : 'bi-check-circle'}"></i>
                            </button>
                        </div>` : ''}
                    </td>
                </tr>`).join(''));
        })
        .fail(() => $cuerpo.html('<tr><td colspan="7" class="text-center text-danger">No fue posible cargar el inventario.</td></tr>'));
}

// ---------------- Artículos ----------------

function abrirArticulo(a) {
    // En el formulario: tipos activos, más el tipo actual del artículo si está inactivo
    const opciones = categorias
        .filter(c => c.activo || (a && c.idCategoria === a.idCategoria))
        .map(c => `<option value="${Number(c.idCategoria)}">${escapeHtml(c.nombre)}${c.activo ? '' : ' (inactivo)'}</option>`)
        .join('');

    $('#aCategoria').html('<option value="">Seleccione...</option>' + opciones);

    $('#modalArticuloTitulo').text(a ? 'Editar artículo' : 'Nuevo artículo');
    $('#aId').val(a ? a.idInventario : 0);
    $('#aCategoria').val(a ? a.idCategoria : '');
    $('#aNombre').val(a ? a.nombre : '');
    $('#aDescripcion').val(a ? (a.descripcion || '') : '');
    $('#aCantidad').val(a ? a.cantidad : 0).prop('disabled', !!a);
    $('#aCantidadAyuda').toggleClass('d-none', !a);
    $('label[for="aCantidad"]').text(a ? 'Cantidad disponible' : 'Cantidad que ingresa');
    $('#aUnidad').val(a ? a.unidadMedida : '');
    $('#aMinima').val(a ? a.cantidadMinima : 0);
    $('#aVence').val(a && a.fechaVencimiento ? String(a.fechaVencimiento).substring(0, 10) : '');
    $('#aVence').attr('min', a ? '' : fechaIso(new Date()));

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalArticulo')).show();
    setTimeout(() => $('#aNombre').trigger('focus'), 300);
}

function guardarArticulo() {
    if (!$('#aCategoria').val()) {
        mostrarMensaje('Seleccione el tipo de inventario.', 'warning');
        return;
    }

    const nombre = $('#aNombre').val().trim();
    const unidad = $('#aUnidad').val().trim();

    if (!nombre) {
        mostrarMensaje('Indique el nombre del artículo.', 'warning');
        return;
    }

    if (!unidad) {
        mostrarMensaje('Indique la unidad de medida.', 'warning');
        return;
    }

    const dto = {
        idInventario: Number($('#aId').val()) || 0,
        idCategoria: Number($('#aCategoria').val()),
        nombre: nombre,
        descripcion: $('#aDescripcion').val().trim() || null,
        cantidadInicial: Number($('#aCantidad').val()) || 0,
        unidadMedida: unidad,
        fechaVencimiento: $('#aVence').val() || null,
        cantidadMinima: Number($('#aMinima').val()) || 0
    };

    const $boton = $('#btnGuardarArticulo').prop('disabled', true);

    $.ajax({
        url: '/Inventario/GuardarArticulo',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dto)
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalArticulo')).hide();
                cargarCategorias().then(buscar);
            }
        })
        .fail(() => mostrarMensaje('No se pudo guardar el artículo.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

async function cambiarEstado(a) {
    const activar = !a.activo;

    const ok = await confirmar(activar
        ? `¿Activar el artículo "${a.nombre}"?`
        : `¿Desactivar el artículo "${a.nombre}"? Ya no estará disponible para movimientos.`,
        activar ? 'Activar' : 'Desactivar',
        activar ? 'success' : 'danger');

    if (!ok) return;

    $.post('/Inventario/CambiarEstadoArticulo', { idInventario: a.idInventario, activo: activar })
        .done(function (resp) {
            mostrarMensaje(resp.mensaje, resp.esCorrecto ? 'success' : 'danger');
            if (resp.esCorrecto) buscar();
        })
        .fail(() => mostrarMensaje('No se pudo cambiar el estado del artículo.', 'danger'));
}

// ---------------- Entradas y salidas ----------------

function abrirMovimiento(a, tipo) {
    const esSalida = tipo === 'Salida';

    $('#modalMovimientoTitulo').text(esSalida ? 'Registrar salida' : 'Registrar entrada');
    $('#mIdInventario').val(a.idInventario);
    $('#mTipo').val(tipo);
    $('#mResumen').html(`<strong>${escapeHtml(a.nombre)}</strong><br>
        Disponible: ${Number(a.cantidad).toLocaleString('es-CR')} ${escapeHtml(a.unidadMedida)}`);

    $('#mCantidad').val('').attr('max', esSalida ? a.cantidad : '');
    $('#mNombre').val('');
    $('#mCedula').val('');
    $('#mMotivo').val('').attr('placeholder', esSalida ? 'Uso que se le dará a lo retirado' : 'Ej.: Compra, factura 123');
    $('#mMotivoEtiqueta').html(esSalida ? 'Motivo de la salida' : 'Motivo <span class="text-muted small">(opcional)</span>');
    $('#mResponsable').toggleClass('d-none', !esSalida);
    $('#btnGuardarMovimiento').toggleClass('btn-warning', esSalida).toggleClass('btn-success', !esSalida);

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalMovimiento')).show();
    setTimeout(() => $('#mCantidad').trigger('focus'), 300);
}

function guardarMovimiento() {
    const tipo = $('#mTipo').val();
    const cantidad = Number($('#mCantidad').val());

    if (!Number.isInteger(cantidad) || cantidad <= 0) {
        mostrarMensaje('Indique una cantidad válida, mayor a cero.', 'warning');
        return;
    }

    if (tipo === 'Salida') {
        if (!$('#mNombre').val().trim()) {
            mostrarMensaje('Indique el nombre de la persona que retira.', 'warning');
            return;
        }

        if (!$('#mCedula').val().trim()) {
            mostrarMensaje('Indique la cédula de la persona que retira.', 'warning');
            return;
        }

        if (!$('#mMotivo').val().trim()) {
            mostrarMensaje('Indique el motivo de la salida.', 'warning');
            return;
        }
    }

    const dto = {
        idInventario: Number($('#mIdInventario').val()),
        tipo: tipo,
        cantidad: cantidad,
        motivo: $('#mMotivo').val().trim() || null,
        nombreResponsable: tipo === 'Salida' ? $('#mNombre').val().trim() : null,
        cedulaResponsable: tipo === 'Salida' ? $('#mCedula').val().trim() : null
    };

    const $boton = $('#btnGuardarMovimiento').prop('disabled', true);

    $.ajax({
        url: '/Inventario/RegistrarMovimiento',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dto)
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalMovimiento')).hide();
                buscar();
            }
        })
        .fail(() => mostrarMensaje('No se pudo registrar el movimiento.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

// ---------------- Mínimos y historial ----------------

function actualizarAlertaMinimo() {
    $.get('/Inventario/GetArticulos', { estado: 'Activo', soloBajoMinimo: true })
        .done(function (r) {
            const total = r.esCorrecto ? (r.dato || []).length : 0;
            const $alerta = $('#alertaMinimo');

            if (total > 0) {
                $('#alertaMinimoTexto').text(total === 1
                    ? '1 artículo está por debajo de la cantidad mínima.'
                    : `${total} artículos están por debajo de la cantidad mínima.`);
                $alerta.removeClass('d-none').addClass('d-flex');
            } else {
                $alerta.addClass('d-none').removeClass('d-flex');
            }
        });
}

function abrirHistorial(a) {
    const $cuerpo = $('#tblHistorial tbody');
    $('#modalHistorialTitulo').text(`Historial: ${a.nombre}`);
    $('#hResumen').text('Cargando...');
    $cuerpo.html('<tr><td colspan="7" class="text-center text-muted">Cargando...</td></tr>');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalHistorial')).show();

    $.get('/Inventario/GetHistorial', { idInventario: a.idInventario })
        .done(function (r) {
            if (!r.esCorrecto) {
                $('#hResumen').text('');
                $cuerpo.html(`<tr><td colspan="7" class="text-center text-danger">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const h = r.dato;
            const unidad = escapeHtml(h.articulo.unidadMedida);

            $('#hResumen').html(`
                <strong>${escapeHtml(h.articulo.nombre)}</strong> (${escapeHtml(h.articulo.categoria)}) —
                Disponible: <strong>${Number(h.articulo.cantidad).toLocaleString('es-CR')} ${unidad}</strong> ·
                Entradas: ${Number(h.totalEntradas).toLocaleString('es-CR')} ·
                Salidas: ${Number(h.totalSalidas).toLocaleString('es-CR')}`);

            if (!h.movimientos.length) {
                $cuerpo.html(`<tr><td colspan="7" class="text-center text-muted">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            $cuerpo.html(h.movimientos.map(m => `
                <tr>
                    <td class="text-nowrap">${escapeHtml(new Date(m.fecha).toLocaleString('es-CR'))}</td>
                    <td>${m.tipo === 'Entrada'
                        ? '<span class="badge text-bg-success">Entrada</span>'
                        : '<span class="badge text-bg-warning">Salida</span>'}</td>
                    <td class="text-end">${m.tipo === 'Entrada' ? '+' : '−'}${Number(m.cantidad).toLocaleString('es-CR')}</td>
                    <td class="text-end">${Number(m.existenciaResultante).toLocaleString('es-CR')}</td>
                    <td>${m.nombreResponsable
                        ? `${escapeHtml(m.nombreResponsable)}<div class="small text-muted">${escapeHtml(m.cedulaResponsable || '')}</div>`
                        : '<span class="text-muted">—</span>'}</td>
                    <td>${m.motivo ? escapeHtml(m.motivo) : '<span class="text-muted">—</span>'}</td>
                    <td>${escapeHtml(m.usuarioRegistra || '')}</td>
                </tr>`).join(''));
        })
        .fail(() => {
            $('#hResumen').text('');
            $cuerpo.html('<tr><td colspan="7" class="text-center text-danger">No fue posible cargar el historial.</td></tr>');
        });
}