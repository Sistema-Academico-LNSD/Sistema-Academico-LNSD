// Tipos de inventario (MIVF-01-02 y 01-03).

let tipos = [];

$(function () {
    cargarTipos();

    $('#tVerInactivos').on('change', cargarTipos);
    $('#btnNuevoTipo').on('click', () => abrirTipo(null));
    $('#btnGuardarTipo').on('click', guardarTipo);
    $('#formTipo').on('submit', function (e) {
        e.preventDefault();
        guardarTipo();
    });

    $(document).on('click', '[data-accion-tipo]', function () {
        const d = $(this).data();
        const tipo = tipos.find(t => t.idCategoria === Number(d.idCategoria));

        if (!tipo) return;

        if (d.accionTipo === 'editar') abrirTipo(tipo);
        else cambiarEstado(tipo);
    });
});

function cargarTipos() {
    const $cuerpo = $('#tblTipos tbody');
    $cuerpo.html('<tr><td colspan="4" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Inventario/GetCategorias', { incluirInactivas: $('#tVerInactivos').is(':checked') })
        .done(function (r) {
            tipos = r.esCorrecto ? (r.dato || []) : [];

            if (!tipos.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="4" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const puedeEditar = window.inventarioPermisos && window.inventarioPermisos.editar;

            $cuerpo.html(tipos.map(t => `
                <tr class="${t.activo ? '' : 'text-muted'}">
                    <td class="fw-semibold">${escapeHtml(t.nombre)}</td>
                    <td>${Number(t.totalArticulos)}</td>
                    <td>${t.activo
                        ? '<span class="badge text-bg-success">Activo</span>'
                        : '<span class="badge text-bg-secondary">Inactivo</span>'}</td>
                    <td class="text-end text-nowrap">
                        ${puedeEditar ? `
                        <div class="btn-group">
                            <button type="button" class="btn btn-outline-primary btn-sm" title="Editar"
                                    data-accion-tipo="editar" data-id-categoria="${Number(t.idCategoria)}">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-${t.activo ? 'danger' : 'success'} btn-sm"
                                    title="${t.activo ? 'Desactivar' : 'Activar'}"
                                    data-accion-tipo="estado" data-id-categoria="${Number(t.idCategoria)}">
                                <i class="bi ${t.activo ? 'bi-slash-circle' : 'bi-check-circle'}"></i>
                            </button>
                        </div>` : ''}
                    </td>
                </tr>`).join(''));
        })
        .fail(() => $cuerpo.html('<tr><td colspan="4" class="text-center text-danger">No fue posible cargar los tipos de inventario.</td></tr>'));
}

function abrirTipo(t) {
    $('#modalTipoTitulo').text(t ? 'Editar tipo' : 'Nuevo tipo');
    $('#tId').val(t ? t.idCategoria : 0);
    $('#tNombre').val(t ? t.nombre : '');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalTipo')).show();
    setTimeout(() => $('#tNombre').trigger('focus'), 300);
}

function guardarTipo() {
    const nombre = $('#tNombre').val().trim();

    if (!nombre) {
        mostrarMensaje('Indique el nombre del tipo de inventario.', 'warning');
        return;
    }

    const dto = {
        idCategoria: Number($('#tId').val()) || 0,
        nombre: nombre
    };

    const $boton = $('#btnGuardarTipo').prop('disabled', true);

    $.ajax({
        url: '/Inventario/GuardarCategoria',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dto)
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalTipo')).hide();
                cargarTipos();
            }
        })
        .fail(() => mostrarMensaje('No se pudo guardar el tipo de inventario.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

async function cambiarEstado(t) {
    const activar = !t.activo;

    const ok = await confirmar(activar
        ? `¿Activar el tipo "${t.nombre}"? Podrá usarse de nuevo para registrar artículos.`
        : `¿Desactivar el tipo "${t.nombre}"? No se podrán registrar artículos nuevos en él.`,
        activar ? 'Activar' : 'Desactivar',
        activar ? 'success' : 'danger');

    if (!ok) return;

    $.post('/Inventario/CambiarEstadoCategoria', { idCategoria: t.idCategoria, activo: activar })
        .done(function (resp) {
            mostrarMensaje(resp.mensaje, resp.esCorrecto ? 'success' : 'danger');
            if (resp.esCorrecto) cargarTipos();
        })
        .fail(() => mostrarMensaje('No se pudo cambiar el estado del tipo de inventario.', 'danger'));
}