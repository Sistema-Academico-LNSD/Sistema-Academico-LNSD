$(document).ready(function () {
    cargarTodo();

    $("#cmbEstado").on('change', function () {
        cargarTodo();
    });

    $("#btnActualizar").click(function () {
        cargarTodo();
    });
});

// Cada consulta refresca también las cantidades (MBLF-01-08, escenarios 3 y 4)
function cargarTodo() {
    cargarResumen();
    cargarTiquetes();
}

// ---------- MBLF-01-08: cantidades ----------

function cargarResumen() {
    $.ajax({
        url: '/Boleteria/GetResumen',
        type: 'GET',

        success: function (response) {
            if (response.esCorrecto && response.dato != null) {
                $("#cntGenerados").text(response.dato.generados);
                $("#cntDisponibles").text(response.dato.disponibles);
                $("#cntUtilizados").text(response.dato.utilizados);
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("No fue posible obtener las cantidades.", 'danger');
        }
    });
}

// ---------- MBLF-01-06 y 01-07: tiquetes y su estado ----------

function cargarTiquetes() {
    $.ajax({
        url: '/Boleteria/GetTiquetes',
        type: 'GET',
        data: { estado: $("#cmbEstado").val() },

        success: function (response) {
            let filas = '';

            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
            }
            else if (response.dato == null || response.dato.length === 0) {
                filas = `
                    <tr>
                        <td colspan="5" class="text-center text-muted">
                            ${escapeHtml(response.mensaje)}
                        </td>
                    </tr>`;
            }
            else {
                $.each(response.dato, function (index, t) {
                    filas += `
                        <tr>
                            <td class="font-monospace">${escapeHtml(t.codigo)}</td>
                            <td>
                                ${escapeHtml(t.usuarioNombre)} ${escapeHtml(t.usuarioApellido)}<br>
                                <small class="text-muted">${escapeHtml(t.usuarioCorreo)}</small>
                            </td>
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