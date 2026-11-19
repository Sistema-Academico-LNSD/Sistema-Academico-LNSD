// Requisitos de elegibilidad para becas socioeconómicas (MBEF-01-01).

const VERIFICACIONES = {
    '': 'Administrador',
    MatriculaActiva: 'Sistema (matrícula activa)'
};

let requisitos = [];

$(function () {
    cargarRequisitos();

    $('#rVerInactivos').on('change', cargarRequisitos);
    $('#btnNuevoRequisito').on('click', () => abrirRequisito(null));
    $('#btnGuardarRequisito').on('click', guardarRequisito);
    $('#formRequisito').on('submit', function (e) {
        e.preventDefault();
        guardarRequisito();
    });

    $(document).on('click', '[data-accion-requisito]', function () {
        const d = $(this).data();
        const requisito = requisitos.find(r => r.idRequisito === Number(d.idRequisito));

        if (!requisito) return;

        if (d.accionRequisito === 'editar') abrirRequisito(requisito);
        else cambiarEstado(requisito);
    });
});

function cargarRequisitos() {
    const $cuerpo = $('#tblRequisitos tbody');
    $cuerpo.html('<tr><td colspan="5" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Beca/GetRequisitos', { incluirInactivos: $('#rVerInactivos').is(':checked') })
        .done(function (r) {
            requisitos = r.esCorrecto ? (r.dato || []) : [];

            if (!requisitos.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="5" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            $cuerpo.html(requisitos.map(x => `
                <tr class="${x.activo ? '' : 'text-muted'}">
                    <td>
                        <div class="fw-semibold">${escapeHtml(x.nombre)}</div>
                        ${x.descripcion ? `<div class="small text-muted">${escapeHtml(x.descripcion)}</div>` : ''}
                    </td>
                    <td>${x.obligatorio
                        ? '<span class="badge text-bg-primary">Obligatorio</span>'
                        : '<span class="badge text-bg-light border">Opcional</span>'}</td>
                    <td class="small">${escapeHtml(VERIFICACIONES[x.verificacion || ''] || x.verificacion)}</td>
                    <td>${x.activo
                        ? '<span class="badge text-bg-success">Activo</span>'
                        : '<span class="badge text-bg-secondary">Inactivo</span>'}</td>
                    <td class="text-end text-nowrap">
                        <div class="btn-group">
                            <button type="button" class="btn btn-outline-primary btn-sm" title="Editar"
                                    data-accion-requisito="editar" data-id-requisito="${Number(x.idRequisito)}">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-${x.activo ? 'danger' : 'success'} btn-sm"
                                    title="${x.activo ? 'Desactivar' : 'Activar'}"
                                    data-accion-requisito="estado" data-id-requisito="${Number(x.idRequisito)}">
                                <i class="bi ${x.activo ? 'bi-slash-circle' : 'bi-check-circle'}"></i>
                            </button>
                        </div>
                    </td>
                </tr>`).join(''));
        })
        .fail(() => $cuerpo.html('<tr><td colspan="5" class="text-center text-danger">No fue posible cargar los requisitos.</td></tr>'));
}

function abrirRequisito(r) {
    $('#modalRequisitoTitulo').text(r ? 'Editar requisito' : 'Nuevo requisito');
    $('#rId').val(r ? r.idRequisito : 0);
    $('#rNombre').val(r ? r.nombre : '');
    $('#rDescripcion').val(r ? (r.descripcion || '') : '');
    $('#rVerificacion').val(r ? (r.verificacion || '') : '');
    $('#rObligatorio').prop('checked', r ? r.obligatorio : true);

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalRequisito')).show();
    setTimeout(() => $('#rNombre').trigger('focus'), 300);
}

function guardarRequisito() {
    const nombre = $('#rNombre').val().trim();

    if (!nombre) {
        mostrarMensaje('Indique el nombre del requisito.', 'warning');
        return;
    }

    const dto = {
        idRequisito: Number($('#rId').val()) || 0,
        nombre: nombre,
        descripcion: $('#rDescripcion').val().trim() || null,
        obligatorio: $('#rObligatorio').is(':checked'),
        verificacion: $('#rVerificacion').val() || null
    };

    const $boton = $('#btnGuardarRequisito').prop('disabled', true);

    $.ajax({
        url: '/Beca/GuardarRequisito',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dto)
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalRequisito')).hide();
                cargarRequisitos();
            }
        })
        .fail(() => mostrarMensaje('No se pudo guardar el requisito.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

async function cambiarEstado(r) {
    const activar = !r.activo;

    const ok = await confirmar(activar
        ? `¿Activar el requisito "${r.nombre}"? Se exigirá de nuevo para activar becas.`
        : `¿Desactivar el requisito "${r.nombre}"? Dejará de exigirse; las becas que ya lo tienen marcado lo conservan.`,
        activar ? 'Activar' : 'Desactivar',
        activar ? 'success' : 'danger');

    if (!ok) return;

    $.post('/Beca/CambiarEstadoRequisito', { idRequisito: r.idRequisito, activo: activar })
        .done(function (resp) {
            mostrarMensaje(resp.mensaje, resp.esCorrecto ? 'success' : 'danger');
            if (resp.esCorrecto) cargarRequisitos();
        })
        .fail(() => mostrarMensaje('No se pudo cambiar el estado del requisito.', 'danger'));
}
