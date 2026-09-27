$(document).ready(function () {
    cargarHistorial();

    $("#btnBuscarHistorial").on("click", function () {
        cargarHistorial();
    });
});

function cargarHistorial() {
    let desde = $("#txtDesde").val();
    let hasta = $("#txtHasta").val();
    let filtros = {
        correo: $("#txtCorreo").val(),
        desde: desde ? new Date(desde + 'T00:00:00').toISOString() : '',
        hasta: hasta ? new Date(hasta + 'T23:59:59.999').toISOString() : '',
        soloFallidos: $("#chkSoloFallidos").is(":checked")
    };

    $.ajax({
        url: '/Auditoria/GetHistorial',
        type: 'GET',
        data: filtros,

        success: function (response) {
            let filas = '';

            if (response.dato != null) {
                $.each(response.dato, function (index, log) {
                    let badgeEvento = log.tipoEvento === 'Login'
                        ? '<span class="badge bg-primary">Login</span>'
                        : '<span class="badge bg-secondary">Logout</span>';

                    let badgeResultado = log.exitoso
                        ? '<span class="badge bg-success">Exitoso</span>'
                        : '<span class="badge bg-danger">Fallido</span>';

                    filas += `
                        <tr>
                            <td>${formatearFecha(log.fecha)}</td>
                            <td>${badgeEvento}</td>
                            <td>${escapeHtml(log.nombreUsuario ?? '-')}</td>
                            <td>${escapeHtml(log.correo)}</td>
                            <td>${badgeResultado}</td>
                            <td>${escapeHtml(log.mensaje)}</td>
                        </tr>`;
                });
            }

            if (filas === '') {
                filas = '<tr><td colspan="6" class="text-center text-muted">Sin resultados</td></tr>';
            }

            $('#tblHistorial tbody').html(filas);
        },

        error: function () {
            mostrarMensaje('Error al cargar el historial de accesos.', 'danger');
        }
    });
}
function formatearFecha(fechaIso) {
    let fecha = new Date(fechaIso);

    return fecha.toLocaleString('es-CR', {
        dateStyle: 'short',
        timeStyle: 'medium'
    });
}