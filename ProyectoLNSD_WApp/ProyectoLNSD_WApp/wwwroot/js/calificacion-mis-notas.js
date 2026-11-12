// Consulta de calificaciones del estudiante (MCNF-01-09) y del encargado (MCNF-01-10).
// El estudiante y la relación con el encargado se validan en el servidor.
// Usa renderDetalleCalificaciones (calificacion-detalle.js) y los helpers de site.js.

let conModo = 'estudiante';

$(document).ready(function () {
    conModo = $("#calConsulta").data("modo");

    $("#conPeriodo").on("change", cargarDetalleConsulta);

    if (conModo === 'encargado') {
        $("#conEstudiante").on("change", cargarPeriodosConsulta);
        cargarEstudiantesConsulta();
    } else {
        cargarPeriodosConsulta();
    }
});

function avisoConsulta(texto) {
    let $aviso = $("#conAviso");

    if (!texto) {
        $aviso.addClass('d-none').text('');
        return;
    }

    $aviso.removeClass('d-none').text(texto);
}

function cargarEstudiantesConsulta() {
    $.ajax({
        url: '/Calificacion/GetMisEstudiantes',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                avisoConsulta(response.mensaje);
                return;
            }

            let lista = response.dato || [];
            let html = '';

            lista.forEach(function (e) {
                html += `<option value="${e.idEstudiante}">${escapeHtml(e.nombre)} (${escapeHtml(e.carnet)})</option>`;
            });

            $("#conEstudiante").html(html);

            if (lista.length === 0) {
                avisoConsulta(response.mensaje);
                $("#conPeriodo").html('');
                return;
            }

            cargarPeriodosConsulta();
        },

        error: function () {
            avisoConsulta("No fue posible cargar sus estudiantes.");
        }
    });
}

function cargarPeriodosConsulta() {
    let esEncargado = conModo === 'encargado';

    $.ajax({
        url: esEncargado ? '/Calificacion/GetPeriodosDeEstudiante' : '/Calificacion/GetMisPeriodos',
        type: 'GET',
        data: esEncargado ? { idEstudiante: $("#conEstudiante").val() } : {},

        success: function (response) {
            if (!response.esCorrecto) {
                avisoConsulta(response.mensaje);
                return;
            }

            let periodos = response.dato || [];
            let html = '';

            periodos.forEach(function (p) {
                html += `<option value="${p.id}">${escapeHtml(p.nombre)}${p.activo ? ' (activo)' : ''}</option>`;
            });

            $("#conPeriodo").html(html);

            if (periodos.length === 0) {
                avisoConsulta(response.mensaje);
                $("#conDetalle").html('');
                return;
            }

            let activo = periodos.find(p => p.activo);

            if (activo) {
                $("#conPeriodo").val(activo.id);
            }

            cargarDetalleConsulta();
        },

        error: function () {
            avisoConsulta("No fue posible cargar los períodos lectivos.");
        }
    });
}

function cargarDetalleConsulta() {
    let esEncargado = conModo === 'encargado';

    avisoConsulta('');
    $("#conDetalle").html('<div class="text-center text-muted py-4">Cargando...</div>');

    $.ajax({
        url: esEncargado ? '/Calificacion/GetDetalleDeEstudiante' : '/Calificacion/GetMiDetalle',
        type: 'GET',
        data: esEncargado
            ? { idEstudiante: $("#conEstudiante").val(), idPeriodo: $("#conPeriodo").val() }
            : { idPeriodo: $("#conPeriodo").val() },

        success: function (response) {
            if (!response.esCorrecto) {
                $("#conDetalle").html('');
                avisoConsulta(response.mensaje);
                return;
            }

            $("#conDetalle").html(renderDetalleCalificaciones(response.dato));
        },

        error: function () {
            $("#conDetalle").html('');
            avisoConsulta("No fue posible consultar las calificaciones.");
        }
    });
}
