$(document).ready(function () {
    cargarTodo();

    $("#btnGenerarTiquete").click(function () {
        generarTiquete();
    });
});

function cargarTodo() {
    cargarTiqueteActual();
    cargarMisTiquetes();
}


// ---------- MBLF-01-03: visualizar el tiquete ----------

function cargarTiqueteActual() {
    $.ajax({
        url: '/Boleteria/GetTiqueteActual',
        type: 'GET',

        success: function (response) {
            if (response.esCorrecto && response.dato != null) {
                pintarTiqueteActual(response.dato);
                return;
            }

            if (response.codigo === 1104) {
                $("#tiqueteActual").html(`
                    <div class="text-center text-muted py-4">
                        <i class="bi bi-ticket-perforated fs-1"></i>
                        <p class="mb-0 mt-2">${escapeHtml(response.mensaje)}</p>
                    </div>`);
                return;
            }

            $("#tiqueteActual").html('');
            mostrarMensaje(response.mensaje, 'danger');
        },

        error: function () {
            mostrarMensaje("No fue posible realizar la consulta.", 'danger');
        }
    });
}

function pintarTiqueteActual(tiquete) {
    let disponible = tiquete.estado === 'Disponible';

    let detalle = disponible
        ? `Generado el ${escapeHtml(formatearFecha(tiquete.fechaGeneracion))}`
        : `Este tiquete ya fue utilizado el ${escapeHtml(formatearFecha(tiquete.fechaUtilizacion))}`;

    $("#tiqueteActual").html(`
        <div class="text-center py-3">
            <div class="text-muted small">Código de tiquete</div>
            <div class="display-6 fw-bold font-monospace my-2">${escapeHtml(tiquete.codigo)}</div>
            <div class="mb-2">${badgeEstado(tiquete.estado)}</div>
            <div class="text-muted">${detalle}</div>
        </div>`);
}

// ---------- MBLF-01-02: consultar mis tiquetes ----------

function cargarMisTiquetes() {
    $.ajax({
        url: '/Boleteria/GetMisTiquetes',
        type: 'GET',

        success: function (response) {
            let filas = '';

            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
            }
            else if (response.dato == null || response.dato.length === 0) {
                filas = `
                    <tr>
                        <td colspan="4" class="text-center text-muted">
                            ${escapeHtml(response.mensaje)}
                        </td>
                    </tr>`;
            }
            else {
                $.each(response.dato, function (index, t) {
                    filas += `
                        <tr>
                            <td class="font-monospace">${escapeHtml(t.codigo)}</td>
                            <td>${badgeEstado(t.estado)}</td>
                            <td>${escapeHtml(formatearFecha(t.fechaGeneracion))}</td>
                            <td>${escapeHtml(formatearFecha(t.fechaUtilizacion))}</td>
                        </tr>`;
                });
            }

            $("#tblTiquetes tbody").html(filas);
        },

        error: function () {
            mostrarMensaje("No fue posible realizar la consulta.", 'danger');
        }
    });
}

// ---------- MBLF-01-01: generar tiquete ----------

function generarTiquete() {
    let $boton = $("#btnGenerarTiquete");

    // Evita doble clic mientras se procesa la solicitud
    $boton.prop('disabled', true);

    $.ajax({
        url: '/Boleteria/GenerarTiquete',
        type: 'POST',

        success: function (response) {
            if (response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'success');
                cargarTodo();
            }
            else {
                // 1102 = ya tiene un tiquete disponible (aviso, no error grave)
                mostrarMensaje(response.mensaje, response.codigo === 1102 ? 'warning' : 'danger');
            }
        },

        error: function () {
            mostrarMensaje("No se pudo generar el tiquete. Intente nuevamente.", 'danger');
        },

        complete: function () {
            $boton.prop('disabled', false);
        }
    });
}