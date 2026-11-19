// Becas socioeconómicas (MBEF-01-02 a 01-06, 01-08, 01-09 y 01-10).
// Los permisos los decide el servidor; aquí solo se ocultan botones que no aplican.

const permisosBeca = {
    crear: $('#becasPagina').data('puede-crear') === true,
    editar: $('#becasPagina').data('puede-editar') === true
};

let opcionesBeca = null;
let becaActual = null;       // detalle que se está viendo o editando
let idBecaDesactivar = null;
let temporizadorBusqueda = null;

$(function () {
    cargarOpciones();

    $('#formFiltros').on('submit', function (e) {
        e.preventDefault();
        buscarBecas();
    });
    $('#btnLimpiar').on('click', limpiarFiltros);
    $('#fPeriodo, #fGrado, #fEstado').on('change', buscarBecas);

    $('#btnNuevaBeca').on('click', nuevaBeca);
    $('#btnCambiarEstudiante').on('click', mostrarPasoEstudiante);
    $('#btnBuscarEstudiante').on('click', buscarEstudiantes);
    $('#bBuscarEstudiante')
        .on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                buscarEstudiantes();
            }
        })
        .on('input', function () {
            clearTimeout(temporizadorBusqueda);
            temporizadorBusqueda = setTimeout(buscarEstudiantes, 400);
        });
    $('#bPeriodo').on('change', function () {
        if ($('#bBuscarEstudiante').val().trim().length >= 2) buscarEstudiantes();
    });
    $('#bResultados').on('click', '[data-id-estudiante]', function () {
        prepararBeca($(this).data('id-estudiante'), $('#bPeriodo').val());
    });
    $('#bRequisitos').on('change', 'input[type="checkbox"]', actualizarCumplimiento);
    $('#btnGuardarBeca').on('click', guardarBeca);
    $('#btnConfirmarDesactivar').on('click', confirmarDesactivar);

    // Acciones de las filas (sin onclick en línea)
    $(document).on('click', '[data-accion-beca]', function () {
        const d = $(this).data();

        switch (d.accionBeca) {
            case 'detalle': abrirDetalle(d.idBeca); break;
            case 'historial': verHistorialBeca(d.idEstudiante); break;
            case 'activar': activarBeca(d.idBeca, d.estudiante); break;
            case 'desactivar': abrirDesactivar(d.idBeca, d.estudiante); break;
        }
    });
});

// =====================================================================
// Filtros y lista (MBEF-01-08 y 01-10)
// =====================================================================

function cargarOpciones() {
    $.get('/Beca/GetOpciones')
        .done(function (r) {
            if (!r.esCorrecto) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }

            opcionesBeca = r.dato;

            const periodos = opcionesBeca.periodos.map(p => ({
                id: p.id,
                nombre: p.activo ? `${p.nombre} (activo)` : p.nombre
            }));

            llenarOpciones('#fPeriodo', periodos, 'Todos los períodos', opcionesBeca.idPeriodoActivo);
            llenarOpciones('#fGrado', opcionesBeca.grados, 'Todos los grados');

            buscarBecas();
        })
        .fail(() => mostrarMensaje('No fue posible cargar los filtros.', 'danger'));
}

function llenarOpciones(selector, opciones, textoTodos, seleccionado) {
    const html = (textoTodos ? `<option value="">${escapeHtml(textoTodos)}</option>` : '') +
        (opciones || []).map(o => `<option value="${Number(o.id)}">${escapeHtml(o.nombre)}</option>`).join('');

    $(selector).html(html);

    if (seleccionado !== undefined && seleccionado !== null) {
        $(selector).val(String(seleccionado));
    }
}

function limpiarFiltros() {
    $('#fTexto, #fGrado').val('');
    $('#fEstado').val('Activa');
    $('#fPeriodo').val(opcionesBeca && opcionesBeca.idPeriodoActivo ? String(opcionesBeca.idPeriodoActivo) : '');
    buscarBecas();
}

function leerFiltro() {
    const filtro = {
        idPeriodo: $('#fPeriodo').val(),
        idGrado: $('#fGrado').val(),
        texto: $('#fTexto').val().trim(),
        estado: $('#fEstado').val()
    };

    Object.keys(filtro).forEach(k => { if (!filtro[k]) delete filtro[k]; });

    return filtro;
}

function buscarBecas() {
    const $cuerpo = $('#tblBecas tbody');
    $cuerpo.html('<tr><td colspan="7" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Beca/Buscar', leerFiltro())
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="7" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                $('#becasContadores').empty();
                return;
            }

            if (r.mensaje && lista.length >= 500) {
                mostrarMensaje(r.mensaje, 'warning');
            }

            const total = lista.reduce((suma, b) => suma + Number(b.monto), 0);
            const noCumplen = lista.filter(b => b.estado === 'Activa' && !b.cumpleRequisitos).length;

            $('#becasContadores').html(
                `<span class="badge text-bg-light border">Becas: ${lista.length}</span>` +
                `<span class="badge text-bg-light border">Monto total: ${formatoMonto(total)}</span>` +
                (noCumplen ? `<span class="badge text-bg-warning">Activas que ya no cumplen: ${noCumplen}</span>` : ''));

            $cuerpo.html(lista.map(filaBeca).join(''));
        })
        .fail(function () {
            $cuerpo.html('<tr><td colspan="7" class="text-center text-danger">No fue posible consultar las becas.</td></tr>');
        });
}

function filaBeca(b) {
    const datos = `data-id-beca="${Number(b.idBeca)}" data-id-estudiante="${Number(b.idEstudiante)}" ` +
        `data-estudiante="${escapeAttr(b.nombreEstudiante)}"`;

    const puedeModificar = permisosBeca.editar && b.editable;

    // MBEF-01-05: una beca activa que dejó de cumplir se señala para revisarla
    const alerta = b.estado === 'Activa' && !b.cumpleRequisitos
        ? `<div><span class="badge text-bg-warning" title="${escapeAttr('Falta: ' + b.requisitosFaltantes.join(', '))}">
               <i class="bi bi-exclamation-triangle"></i> No cumple requisitos</span></div>`
        : '';

    const motivo = b.estado === 'Inactiva' && b.motivoDesactivacion
        ? `<div class="small text-muted text-truncate" style="max-width: 16rem" title="${escapeAttr(b.motivoDesactivacion)}">
               ${escapeHtml(b.motivoDesactivacion)}</div>`
        : '';

    let botones = `
        <button type="button" class="btn btn-outline-primary btn-sm" data-accion-beca="detalle" ${datos}
                title="${puedeModificar ? 'Ver y actualizar' : 'Ver detalle'}">
            <i class="bi ${puedeModificar ? 'bi-pencil' : 'bi-eye'}"></i>
        </button>
        <button type="button" class="btn btn-outline-secondary btn-sm" data-accion-beca="historial" ${datos}
                title="Historial de becas del estudiante">
            <i class="bi bi-clock-history"></i>
        </button>`;

    if (puedeModificar && b.estado !== 'Activa') {
        botones += `
        <button type="button" class="btn btn-outline-success btn-sm" data-accion-beca="activar" ${datos} title="Activar">
            <i class="bi bi-check-circle"></i>
        </button>`;
    }

    if (puedeModificar && b.estado !== 'Inactiva') {
        botones += `
        <button type="button" class="btn btn-outline-danger btn-sm" data-accion-beca="desactivar" ${datos} title="Desactivar">
            <i class="bi bi-slash-circle"></i>
        </button>`;
    }

    return `
        <tr>
            <td>
                <div class="fw-semibold">${escapeHtml(b.nombreEstudiante)}</div>
                <div class="small text-muted">${escapeHtml(b.identificacion)} · ${escapeHtml(b.carnet)}</div>
            </td>
            <td>${escapeHtml(b.grado)}</td>
            <td>${escapeHtml(b.nombrePeriodo)}</td>
            <td class="text-end text-nowrap">${formatoMonto(b.monto)}</td>
            <td class="text-nowrap">
                <div class="small">${rangoFechas(b.fechaInicio, b.fechaFin)}</div>
                ${b.estado === 'Activa' ? badgeVigencia(b.vigencia) : ''}
            </td>
            <td>${badgeEstadoBeca(b.estadoVisible)}${alerta}${motivo}</td>
            <td class="text-end text-nowrap"><div class="btn-group">${botones}</div></td>
        </tr>`;
}

// =====================================================================
// Registro: elegir período y estudiante (MBEF-01-02)
// =====================================================================

function nuevaBeca() {
    becaActual = null;

    // Solo períodos que no han terminado
    const abiertos = (opcionesBeca ? opcionesBeca.periodos : []).filter(p => !p.finalizado);

    if (!abiertos.length) {
        mostrarMensaje('No hay períodos lectivos vigentes o futuros para registrar becas.', 'warning');
        return;
    }

    llenarOpciones('#bPeriodo',
        abiertos.map(p => ({ id: p.id, nombre: p.activo ? `${p.nombre} (activo)` : p.nombre })),
        null,
        abiertos.some(p => p.activo) ? opcionesBeca.idPeriodoActivo : abiertos[0].id);

    $('#bBuscarEstudiante').val('');
    $('#bResultados').html('<div class="text-muted small">Busque al estudiante por nombre, identificación o carné.</div>');

    $('#modalBecaTitulo').html('<i class="bi bi-cash-coin"></i> Registrar beca');
    mostrarPasoEstudiante();

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBeca')).show();
    setTimeout(() => $('#bBuscarEstudiante').trigger('focus'), 300);
}

function mostrarPasoEstudiante() {
    $('#pasoBeca').addClass('d-none');
    $('#pasoEstudiante').removeClass('d-none');
    $('#btnGuardarBeca').addClass('d-none');
}

function buscarEstudiantes() {
    clearTimeout(temporizadorBusqueda);

    const texto = $('#bBuscarEstudiante').val().trim();

    if (texto.length < 2) {
        $('#bResultados').html('<div class="text-muted small">Escriba al menos dos letras.</div>');
        return;
    }

    $.get('/Beca/BuscarEstudiantes', { texto: texto })
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                $('#bResultados').html(`<div class="text-muted small">${escapeHtml(r.mensaje)}</div>`);
                return;
            }

            $('#bResultados').html(lista.map(e => `
                <button type="button" class="list-group-item list-group-item-action" data-id-estudiante="${Number(e.idEstudiante)}">
                    <div class="fw-semibold">${escapeHtml(e.nombreEstudiante)}</div>
                    <div class="text-muted">${escapeHtml(e.identificacion)} · ${escapeHtml(e.carnet)}${e.grado ? ' · ' + escapeHtml(e.grado) : ''}</div>
                </button>`).join(''));
        })
        .fail(() => $('#bResultados').html('<div class="text-danger small">No fue posible buscar estudiantes.</div>'));
}

function prepararBeca(idEstudiante, idPeriodo) {
    $.get('/Beca/Preparar', { idEstudiante: idEstudiante, idPeriodo: idPeriodo })
        .done(function (r) {
            if (!r.esCorrecto) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }

            // Si ya tenía beca en ese período, el servidor devuelve esa y lo avisa
            mostrarFormulario(r.dato, r.dato.idBeca ? r.mensaje : null);
        })
        .fail(() => mostrarMensaje('No fue posible preparar el registro.', 'danger'));
}

// =====================================================================
// Formulario y detalle (MBEF-01-02, 01-03 y 01-06)
// =====================================================================

function abrirDetalle(idBeca) {
    $.get('/Beca/GetDetalle', { idBeca: idBeca })
        .done(function (r) {
            if (!r.esCorrecto) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }

            mostrarFormulario(r.dato, null);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBeca')).show();
        })
        .fail(() => mostrarMensaje('No fue posible cargar la beca.', 'danger'));
}

function mostrarFormulario(d, aviso) {
    becaActual = d;

    const esNueva = !d.idBeca;
    const puedeGuardar = d.editable && (esNueva ? permisosBeca.crear : permisosBeca.editar);

    $('#modalBecaTitulo').html(esNueva
        ? '<i class="bi bi-cash-coin"></i> Registrar beca'
        : (puedeGuardar ? '<i class="bi bi-pencil"></i> Actualizar beca' : '<i class="bi bi-eye"></i> Detalle de la beca'));

    $('#pasoEstudiante').addClass('d-none');
    $('#pasoBeca').removeClass('d-none');
    $('#btnCambiarEstudiante').toggleClass('d-none', !esNueva);

    // Encabezado: estudiante, período y matrícula
    $('#bNombre').text(d.nombreEstudiante);
    $('#bIdentificacion').text(`${d.identificacion} · ${d.carnet}`);

    const matricula = d.estadoMatricula
        ? `Matrícula ${d.estadoMatricula.toLowerCase()}`
        : 'Sin matrícula en este período';

    $('#bUbicacion').html(
        `${escapeHtml(d.nombrePeriodo)}${d.grado ? ' · ' + escapeHtml(d.grado) : ''} · ` +
        `<span class="${d.estadoMatricula === 'Activa' ? 'text-success' : 'text-danger'}">${escapeHtml(matricula)}</span>`);

    $('#bEstado').html(esNueva ? '<span class="badge text-bg-light border">Nueva</span>'
        : badgeEstadoBeca(d.estadoVisible) + (d.estado === 'Activa' && d.vigencia ? ' ' + badgeVigencia(d.vigencia) : ''));

    // Avisos
    let textoAviso = aviso;
    let claseAviso = 'alert-info';

    if (!d.editable) {
        textoAviso = d.motivoNoEditable;
        claseAviso = 'alert-secondary';
    } else if (d.estado === 'Inactiva' && d.motivoDesactivacion) {
        textoAviso = `Desactivada: ${d.motivoDesactivacion}`;
        claseAviso = 'alert-warning';
    }

    $('#bAviso')
        .removeClass('alert-info alert-secondary alert-warning')
        .addClass(claseAviso)
        .toggleClass('d-none', !textoAviso)
        .text(textoAviso || '');

    // Monto y vigencia (MBEF-01-06: dentro del período)
    $('#bMonto').val(d.monto ?? '');
    $('#bInicio').val(d.fechaInicio || '').attr({ min: d.periodoInicio, max: d.periodoFin });
    $('#bFin').val(d.fechaFin || '').attr({ min: d.periodoInicio, max: d.periodoFin });
    $('#bAyudaVigencia').text(
        `La vigencia debe estar dentro del período ${d.nombrePeriodo} ` +
        `(${rangoFechas(d.periodoInicio, d.periodoFin)}); máximo un año lectivo.`);
    $('#bObservacion').val(d.observacion || '');

    // Requisitos (MBEF-01-03)
    $('#bRequisitos').html(d.requisitos.length
        ? d.requisitos.map(r => filaRequisito(r, puedeGuardar)).join('')
        : '<div class="list-group-item text-muted small">No hay requisitos registrados.</div>');

    if (!$('#bCumplimiento').length) {
        $('#bRequisitos').after('<div id="bCumplimiento" class="small mt-2"></div>');
    }
    actualizarCumplimiento();

    // MBEF-01-04: activar al guardar (solo si todavía no está activa)
    const mostrarActivar = puedeGuardar && d.estado !== 'Activa';
    $('#bActivarFila').toggleClass('d-none', !mostrarActivar);
    $('#bActivar').prop('checked', mostrarActivar && esNueva);

    $('#formBeca').find('input, textarea').prop('disabled', !puedeGuardar);
    $('#bRequisitos input[data-automatico="true"], #bRequisitos input[data-activo="false"]').prop('disabled', true);
    $('#btnGuardarBeca').toggleClass('d-none', !puedeGuardar);

    $('#bAuditoria').html(textoAuditoria(d));
}

function filaRequisito(r, puedeGuardar) {
    const id = `req${Number(r.idRequisito)}`;

    const etiquetas = [
        r.obligatorio ? '<span class="badge text-bg-light border">Obligatorio</span>' : '<span class="badge text-bg-light border text-muted">Opcional</span>',
        r.automatico ? '<span class="badge text-bg-info">Verificado por el sistema</span>' : '',
        r.activo ? '' : '<span class="badge text-bg-secondary">Ya no se exige</span>'
    ].join(' ');

    const estadoAutomatico = r.automatico
        ? `<div class="small ${r.cumplido ? 'text-success' : 'text-danger'}">
               <i class="bi ${r.cumplido ? 'bi-check-circle' : 'bi-x-circle'}"></i> ${r.cumplido ? 'Cumple' : 'No cumple'}</div>`
        : '';

    return `
        <label class="list-group-item d-flex gap-2" for="${id}">
            <input class="form-check-input flex-shrink-0 mt-1" type="checkbox" id="${id}"
                   value="${Number(r.idRequisito)}" ${r.cumplido ? 'checked' : ''}
                   data-automatico="${r.automatico}" data-activo="${r.activo}" data-obligatorio="${r.obligatorio}"
                   data-nombre="${escapeAttr(r.nombre)}" ${puedeGuardar ? '' : 'disabled'} />
            <span class="flex-grow-1">
                <span class="fw-semibold">${escapeHtml(r.nombre)}</span> ${etiquetas}
                ${r.descripcion ? `<span class="d-block small text-muted">${escapeHtml(r.descripcion)}</span>` : ''}
                ${estadoAutomatico}
            </span>
        </label>`;
}

// Indica en vivo si con lo marcado ya cumple los obligatorios (el servidor lo vuelve a validar)
function actualizarCumplimiento() {
    const faltan = $('#bRequisitos input[data-obligatorio="true"][data-activo="true"]')
        .filter(function () { return !this.checked; })
        .map(function () { return $(this).data('nombre'); })
        .get();

    $('#bCumplimiento').html(faltan.length
        ? `<span class="text-danger"><i class="bi bi-x-circle"></i> Faltan requisitos obligatorios: ${escapeHtml(faltan.join(', '))}</span>`
        : '<span class="text-success"><i class="bi bi-check-circle"></i> Cumple los requisitos obligatorios</span>');
}

function textoAuditoria(d) {
    if (!d.idBeca) return '';

    const partes = [];

    if (d.registradaPor) partes.push(`Registrada por ${escapeHtml(d.registradaPor)} el ${fechaHora(d.fechaRegistro)}`);
    if (d.fechaActivacion) partes.push(`Activada el ${fechaHora(d.fechaActivacion)}`);
    if (d.fechaDesactivacion) partes.push(`Desactivada el ${fechaHora(d.fechaDesactivacion)}`);
    if (d.modificadaPor) partes.push(`Última modificación: ${escapeHtml(d.modificadaPor)}, ${fechaHora(d.fechaModificacion)}`);

    return partes.join(' · ');
}

// "2026-11-09T14:30:00" -> "09/11/2026 14:30" (hora del servidor, sin conversión)
function fechaHora(iso) {
    if (!iso) return '';
    return `${fechaCorta(iso)} ${String(iso).substring(11, 16)}`;
}

function guardarBeca() {
    if (!becaActual) return;

    const monto = $('#bMonto').val();

    const dto = {
        idBeca: becaActual.idBeca || 0,
        idEstudiante: becaActual.idEstudiante,
        idPeriodo: becaActual.idPeriodo,
        monto: monto === '' ? null : Number(monto),
        fechaInicio: $('#bInicio').val() || null,
        fechaFin: $('#bFin').val() || null,
        observacion: $('#bObservacion').val().trim() || null,
        idsRequisitosCumplidos: $('#bRequisitos input[data-automatico="false"][data-activo="true"]:checked')
            .map(function () { return Number(this.value); }).get(),
        activar: !$('#bActivarFila').hasClass('d-none') && $('#bActivar').is(':checked')
    };

    const $boton = $('#btnGuardarBeca').prop('disabled', true);

    $.ajax({
        url: '/Beca/Guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dto)
    })
        .done(function (r) {
            if (!r.esCorrecto) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }

            mostrarMensaje(r.mensaje, 'success');
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBeca')).hide();
            buscarBecas();
        })
        .fail(() => mostrarMensaje('No se pudo guardar la beca.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

// =====================================================================
// Activar y desactivar (MBEF-01-04 y 01-05)
// =====================================================================

async function activarBeca(idBeca, nombre) {
    const ok = await confirmar(
        `¿Activar la beca de ${nombre}? Solo se activa si cumple los requisitos obligatorios.`,
        'Activar', 'success');

    if (!ok) return;

    $.post('/Beca/Activar', { idBeca: idBeca })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');
            if (r.esCorrecto) buscarBecas();
        })
        .fail(() => mostrarMensaje('No se pudo activar la beca.', 'danger'));
}

function abrirDesactivar(idBeca, nombre) {
    idBecaDesactivar = idBeca;
    $('#dEstudiante').text(`Beca de ${nombre}. Indique por qué se desactiva; quedará en el historial.`);
    $('#dMotivo').val('');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalDesactivar')).show();
    setTimeout(() => $('#dMotivo').trigger('focus'), 300);
}

function confirmarDesactivar() {
    const motivo = $('#dMotivo').val().trim();

    if (!motivo) {
        mostrarMensaje('Indique el motivo de la desactivación.', 'warning');
        return;
    }

    const $boton = $('#btnConfirmarDesactivar').prop('disabled', true);

    $.post('/Beca/Desactivar', { idBeca: idBecaDesactivar, motivo: motivo })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalDesactivar')).hide();
                buscarBecas();
            }
        })
        .fail(() => mostrarMensaje('No se pudo desactivar la beca.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

// =====================================================================
// Historial de un estudiante (MBEF-01-09)
// =====================================================================

function verHistorialBeca(idEstudiante) {
    $('#hEstudiante').text('');
    $('#hResumen').empty();
    $('#hFilas').html('<tr><td colspan="5" class="text-center text-muted">Cargando...</td></tr>');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalHistorialBeca')).show();

    $.get('/Beca/GetHistorial', { idEstudiante: idEstudiante })
        .done(function (r) {
            if (!r.esCorrecto) {
                $('#hFilas').html(`<tr><td colspan="5" class="text-center text-danger">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const h = r.dato;
            $('#hEstudiante').text(`${h.nombreEstudiante} · ${h.identificacion} · ${h.carnet}`);

            if (!h.becas.length) {
                $('#hFilas').html(`<tr><td colspan="5" class="text-center text-muted">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const activas = h.becas.filter(b => b.estado === 'Activa').length;

            $('#hResumen').html(
                `<span class="badge text-bg-light border">Becas registradas: ${h.becas.length}</span>` +
                `<span class="badge text-bg-light border">Períodos con beca activa: ${activas}</span>` +
                `<span class="badge text-bg-light border">Monto total otorgado: ${formatoMonto(h.montoTotalActivas)}</span>`);

            $('#hFilas').html(h.becas.map(b => `
                <tr>
                    <td>${escapeHtml(b.nombrePeriodo)}<div class="small text-muted">${escapeHtml(b.grado)}</div></td>
                    <td class="text-end text-nowrap">${formatoMonto(b.monto)}</td>
                    <td class="small text-nowrap">${rangoFechas(b.fechaInicio, b.fechaFin)}</td>
                    <td>${badgeEstadoBeca(b.estadoVisible)}</td>
                    <td class="small">${escapeHtml(b.estado === 'Inactiva' ? (b.motivoDesactivacion || '') : '')}</td>
                </tr>`).join(''));
        })
        .fail(() => $('#hFilas').html(
            '<tr><td colspan="5" class="text-center text-danger">No fue posible consultar el historial.</td></tr>'));
}
