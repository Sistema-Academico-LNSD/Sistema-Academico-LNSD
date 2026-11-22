// Detalle del expediente: personas involucradas, formularios y cierre
// (MPPAF-01-02, 01-04, 01-06, 01-10 y 01-14).

const POR_ESTUDIANTE = [3, 6, 7, 8, 9, 10];

let expediente = null;
let pautaCatalogo = null;
let plantillas = [];
let selector = null;

$(function () {
    selector = crearSelectorEstudiantes($('#selectorEstudiantes'), { roles: [] });

    cargar();

    $('#btnEditarEstudiantes').on('click', abrirEstudiantes);
    $('#btnGuardarEstudiantes').on('click', guardarEstudiantes);
    $('#btnAgregarOtro').on('click', () => abrirAgregar(Number($('#otroFormulario').val())));
    $('#btnConfirmarAgregar').on('click', confirmarAgregar);
    $('#btnConfirmarCierre').on('click', confirmarCierre);

    $(document).on('click', '[data-agregar]', function () {
        abrirAgregar(Number($(this).data('agregar')));
    });

    $(document).on('click', '#btnAbrirCierre', abrirCierre);

    $(document).on('click', '[data-quitar]', function () {
        quitarBorrador(Number($(this).data('quitar')), Number($(this).data('numero')));
    });
});

function cargar() {
    $.when($.get('/Pauta/GetExpediente', { idExpediente: window.pautaExpedienteId }),
           $.get('/Pauta/GetPautas'),
           $.get('/Pauta/GetPlantillas'))
        .done(function (re, rp, rpl) {
            if (!re[0].esCorrecto) {
                $('#mensajeExpediente').html(`
                    <div class="alert alert-danger">${escapeHtml(re[0].mensaje)}</div>
                    <a class="btn btn-outline-secondary" href="/Pauta/Index">Volver a los expedientes</a>`);
                return;
            }

            plantillas = rpl[0].esCorrecto ? (rpl[0].dato || []) : [];
            const pautas = rp[0].esCorrecto ? (rp[0].dato || []) : [];

            aplicar(re[0].dato);
            pautaCatalogo = pautas.find(p => p.numero === expediente.numeroPauta) || null;
            dibujar();
        })
        .fail(() => $('#mensajeExpediente').html('<div class="alert alert-danger">No fue posible cargar el expediente.</div>'));
}

function aplicar(dato) {
    expediente = dato;
}

function permisoEditar() {
    return expediente.puedeEditar && window.pautaPermisos.editar;
}

function dibujar() {
    $('#contenidoExpediente').removeClass('d-none');

    $('#expCodigo').text(expediente.codigo);
    $('#expPauta').text(`Pauta ${expediente.numeroPauta}. ${expediente.nombrePauta}`);
    $('#expEstado').html(badgeEstado(expediente.estado));

    $('#btnImprimirExpediente')
        .attr('href', `/PautaDocumento/Expediente/${Number(expediente.idExpediente)}`)
        .toggleClass('d-none', !(expediente.formularios || []).length);
    $('#expFecha').text(fechaCorta(expediente.fechaActivacion));
    $('#expResponsable').text(expediente.responsable);

    $('#expOrigenWrap').html(expediente.idExpedienteOrigen
        ? `Activada desde <a href="/Pauta/Expediente/${Number(expediente.idExpedienteOrigen)}">otro expediente</a>`
        : '');

    dibujarEstudiantes();
    dibujarFormularios();
    dibujarAgregar();
    dibujarCierre();
}

function dibujarEstudiantes() {
    $('#btnEditarEstudiantes').toggleClass('d-none', !permisoEditar());

    const lista = expediente.estudiantesInvolucrados || [];

    if (!lista.length) {
        $('#expEstudiantes').html('<span class="text-muted">Aún no se han registrado personas estudiantes.</span>');
        return;
    }

    $('#expEstudiantes').html(`
        <div class="table-responsive">
            <table class="table table-sm mb-0">
                <thead><tr><th>Nombre</th><th>Sección</th><th>Rol</th></tr></thead>
                <tbody>
                    ${lista.map(e => `
                        <tr>
                            <td>${escapeHtml(e.nombreCompleto)}${e.idEstudiante ? '' : ' <span class="badge text-bg-light border">Manual</span>'}</td>
                            <td>${escapeHtml(e.seccion || '')}</td>
                            <td>${escapeHtml(textoRol(e.rol))}</td>
                        </tr>`).join('')}
                </tbody>
            </table>
        </div>`);
}

function dibujarFormularios() {
    const $cuerpo = $('#tblFormularios tbody');
    const lista = expediente.formularios || [];

    if (!lista.length) {
        $cuerpo.html('<tr><td colspan="5" class="text-center text-muted">Todavía no hay formularios.</td></tr>');
        return;
    }

    $cuerpo.html(lista.map(f => {
        const repetido = lista.filter(x => x.numeroFormulario === f.numeroFormulario).length > 1;
        const editable = permisoEditar() && f.estado !== 'Finalizado';
        const puedeQuitar = permisoEditar() && f.estado === 'Borrador';

        return `
            <tr>
                <td><span class="badge text-bg-primary">N°${Number(f.numeroFormulario)}</span></td>
                <td>
                    ${escapeHtml(f.nombre)}${repetido ? ` <span class="text-muted small">(#${Number(f.secuencia)})</span>` : ''}
                    ${f.requerido ? '<span class="badge text-bg-light border ms-1">Requerido</span>' : ''}
                </td>
                <td>${badgeEstado(f.estado)}</td>
                <td>${new Date(f.fechaActualizacion).toLocaleString('es-CR')}</td>
                <td class="text-end">
                    <a class="btn btn-sm btn-outline-secondary" target="_blank" rel="noopener" href="/PautaDocumento/Formulario/${Number(f.idFormulario)}" title="Vista previa / Imprimir">
                        <i class="bi bi-printer"></i>
                    </a>
                    <a class="btn btn-sm btn-outline-primary" href="/Pauta/Formulario/${Number(f.idFormulario)}">
                        <i class="bi ${editable ? 'bi-pencil-square' : 'bi-eye'}"></i> ${editable ? 'Completar' : 'Ver'}
                    </a>
                    ${puedeQuitar ? `<button type="button" class="btn btn-sm btn-outline-danger" data-quitar="${Number(f.idFormulario)}" data-numero="${Number(f.numeroFormulario)}" title="Quitar"><i class="bi bi-trash"></i></button>` : ''}
                </td>
            </tr>`;
    }).join(''));
}

function dibujarAgregar() {
    const visible = permisoEditar() && window.pautaPermisos.crear;
    $('#cardAgregar').toggleClass('d-none', !visible);
    if (!visible) return;

    const faltantes = expediente.formulariosFaltantes || [];

    $('#listaFaltantes').html(faltantes.length
        ? faltantes.map(f => `
            <div class="list-group-item d-flex justify-content-between align-items-center gap-2">
                <div>
                    <span class="badge text-bg-primary">N°${Number(f.numero)}</span>
                    ${escapeHtml(f.nombre)}
                    ${f.requerido ? '<span class="badge text-bg-warning text-dark ms-1">Requerido</span>' : '<span class="badge text-bg-light border ms-1">Sugerido</span>'}
                </div>
                <button type="button" class="btn btn-sm btn-outline-primary" data-agregar="${Number(f.numero)}">
                    <i class="bi bi-plus-lg"></i> Agregar
                </button>
            </div>`).join('')
        : '<div class="list-group-item text-muted">Ya se crearon todos los formularios indicados para esta pauta.</div>');

    $('#otroFormulario').html(plantillas.map(p => `<option value="${Number(p.numero)}">N°${Number(p.numero)}. ${escapeHtml(p.nombre)}</option>`).join(''));
}

function dibujarCierre() {
    const $c = $('#expCierre');

    if (expediente.estado === 'Cerrado') {
        $c.html(`
            <div class="row small">
                <div class="col-md-3"><div class="text-muted">Fecha de cierre</div>${expediente.fechaCierre ? new Date(expediente.fechaCierre).toLocaleDateString('es-CR') : ''}</div>
                <div class="col-md-3"><div class="text-muted">Encargado de la activación</div>${escapeHtml(expediente.nombreCierre || '')}</div>
                <div class="col-md-3"><div class="text-muted">Visto bueno de la dirección</div>${escapeHtml(expediente.vistoBuenoDirector || '')}</div>
                <div class="col-md-3"><div class="text-muted">Devolución a encargados</div>${expediente.devolucionRealizada ? 'Realizada' : 'No aplica'}</div>
            </div>
            ${expediente.observacionCierre ? `<div class="small mt-2"><span class="text-muted">Observaciones:</span> ${escapeHtml(expediente.observacionCierre)}</div>` : ''}`);
        return;
    }

    const pendientes = expediente.pendientesCierre || [];
    const botones = `
        <div class="mt-3 d-flex flex-wrap gap-2">
            ${window.pautaPermisos.crear ? `<a class="btn btn-outline-secondary" href="/Pauta/Nueva?origen=${Number(expediente.idExpediente)}"><i class="bi bi-diagram-2"></i> Activar otra pauta desde este expediente</a>` : ''}
        </div>`;

    if (pendientes.length) {
        $c.html(`
            <div class="text-muted mb-1">Para cerrar el expediente falta:</div>
            <ul class="mb-0">${pendientes.map(p => `<li>${escapeHtml(p)}</li>`).join('')}</ul>${botones}`);
        return;
    }

    $c.html(`
        <div class="text-success"><i class="bi bi-check-circle"></i> Todos los formularios están finalizados. El expediente se puede cerrar.</div>
        <div class="mt-3 d-flex flex-wrap gap-2">
            ${permisoEditar() ? '<button type="button" id="btnAbrirCierre" class="btn btn-dark"><i class="bi bi-lock"></i> Cerrar expediente</button>' : ''}
            ${window.pautaPermisos.crear ? `<a class="btn btn-outline-secondary" href="/Pauta/Nueva?origen=${Number(expediente.idExpediente)}"><i class="bi bi-diagram-2"></i> Activar otra pauta desde este expediente</a>` : ''}
        </div>`);
}

// ---- Personas estudiantes ----

function abrirEstudiantes() {
    selector.setRoles(pautaCatalogo ? pautaCatalogo.rolesEstudiante : []);
    selector.setLista(expediente.estudiantesInvolucrados);
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalEstudiantes')).show();
}

function guardarEstudiantes() {
    const lista = selector.getLista();

    if (lista.some(e => !e.nombreCompleto)) {
        mostrarMensaje('Escriba el nombre de todas las personas estudiantes o quite las filas vacías.', 'warning');
        return;
    }

    const $boton = $('#btnGuardarEstudiantes').prop('disabled', true);

    $.ajax({
        url: `/Pauta/ActualizarEstudiantes?idExpediente=${window.pautaExpedienteId}`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(lista)
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto && r.dato) {
                aplicar(r.dato);
                dibujar();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalEstudiantes')).hide();
            }
        })
        .fail(() => mostrarMensaje('No se pudieron guardar las personas estudiantes.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

// ---- Agregar formulario ----

function abrirAgregar(numero) {
    if (!numero) return;

    const estudiantes = expediente.estudiantesInvolucrados || [];
    const delProcedimiento = pautaCatalogo
        && (pautaCatalogo.requeridos.includes(numero) || pautaCatalogo.sugeridos.includes(numero));
    const pideEstudiante = POR_ESTUDIANTE.includes(numero) && estudiantes.length > 1;

    $('#agNumero').val(numero);
    $('#agJust').val('');
    $('#agEstudiante').html(estudiantes.map(e => `<option value="${Number(e.idPautaEstudiante)}">${escapeHtml(e.nombreCompleto)} (${escapeHtml(textoRol(e.rol))})</option>`).join(''));
    $('#agEstudianteWrap').toggleClass('d-none', !pideEstudiante);
    $('#agJustWrap').toggleClass('d-none', !!delProcedimiento);

    const plantilla = plantillas.find(p => p.numero === numero);
    $('#modalAgregarTitulo').text(`Agregar formulario N°${numero}${plantilla ? '. ' + plantilla.nombre : ''}`);

    if (!pideEstudiante && delProcedimiento) {
        enviarAgregar();
        return;
    }

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalAgregar')).show();
}

function confirmarAgregar() {
    const justificacion = $('#agJust').val().trim();

    if (!$('#agJustWrap').hasClass('d-none') && justificacion.length < 10) {
        mostrarMensaje('Escriba una justificación de al menos 10 caracteres.', 'warning');
        return;
    }

    enviarAgregar();
}

function enviarAgregar() {
    const idEstudiante = $('#agEstudianteWrap').hasClass('d-none') ? null : Number($('#agEstudiante').val()) || null;
    const justificacion = $('#agJust').val().trim();

    $('#btnConfirmarAgregar').prop('disabled', true);

    $.ajax({
        url: '/Pauta/AgregarFormulario',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            idExpediente: window.pautaExpedienteId,
            numeroFormulario: Number($('#agNumero').val()),
            idPautaEstudiante: idEstudiante,
            justificacion: justificacion || null
        })
    })
        .done(function (r) {
            if (r.esCorrecto && r.dato) {
                window.location.href = `/Pauta/Formulario/${Number(r.dato.idFormulario)}`;
                return;
            }

            mostrarMensaje(r.mensaje, 'danger');
            $('#btnConfirmarAgregar').prop('disabled', false);
        })
        .fail(function () {
            mostrarMensaje('No se pudo agregar el formulario.', 'danger');
            $('#btnConfirmarAgregar').prop('disabled', false);
        });
}

async function quitarBorrador(idFormulario, numero) {
    const ok = await confirmar(`¿Quitar el formulario N°${numero} del expediente? Todavía no tiene datos guardados.`, 'Quitar');
    if (!ok) return;

    $.post('/Pauta/EliminarFormulario', { idFormulario })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');
            if (r.esCorrecto) cargar();
        })
        .fail(() => mostrarMensaje('No se pudo quitar el formulario.', 'danger'));
}

// ---- Cierre ----

function abrirCierre() {
    $('#cFecha').val(fechaIso(new Date()));
    $('#cNombre').val(expediente.responsable || '');
    $('#cVistoBueno').val('');
    $('#cDevolucion').prop('checked', false);
    $('#cObservacion').val('');
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCierre')).show();
}

async function confirmarCierre() {
    if (!$('#cVistoBueno').val().trim()) {
        mostrarMensaje('Escriba el nombre de la persona directora que da el visto bueno.', 'warning');
        return;
    }

    const ok = await confirmar('¿Cerrar el expediente? Quedará de solo lectura y no se podrá reabrir.', 'Cerrar expediente', 'dark');
    if (!ok) return;

    const $boton = $('#btnConfirmarCierre').prop('disabled', true);

    $.ajax({
        url: '/Pauta/CerrarExpediente',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            idExpediente: window.pautaExpedienteId,
            fechaCierre: $('#cFecha').val() || null,
            nombreCierre: $('#cNombre').val().trim() || null,
            vistoBuenoDirector: $('#cVistoBueno').val().trim(),
            devolucionRealizada: $('#cDevolucion').is(':checked'),
            observacion: $('#cObservacion').val().trim() || null
        })
    })
        .done(function (r) {
            mostrarMensaje(mensajeEnLineas(r.mensaje), r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto && r.dato) {
                aplicar(r.dato);
                dibujar();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCierre')).hide();
            }
        })
        .fail(() => mostrarMensaje('No se pudo cerrar el expediente.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}
