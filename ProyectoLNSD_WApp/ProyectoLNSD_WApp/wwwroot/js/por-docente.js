$(document).ready(function () {
    cargarDocentes();
    cargarPeriodosConsulta("#pdPeriodo", consultarDocente);

    // La consulta se actualiza sola al cambiar el docente o el período
    $("#pdDocente, #pdPeriodo").on("change", consultarDocente);
});

function cargarDocentes() {
    $.ajax({
        url: '/Horario/GetDocentesConsulta',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let opciones = '<option value="">Seleccione un docente</option>';

            $.each(response.dato, function (index, d) {
                opciones += `<option value="${d.id}">${escapeHtml(d.nombre)}</option>`;
            });

            $("#pdDocente").html(opciones);
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los docentes.", 'danger');
        }
    });
}

function consultarDocente() {
    let idDocente = $("#pdDocente").val();

    if (!idDocente) {
        $("#pdContador").text('');
        pintarCuadricula("#pdCuadricula", [], "Seleccione un docente para ver sus clases", formatoClaseDocente);
        return;
    }

    $.ajax({
        url: '/Horario/GetHorariosDeDocente',
        type: 'GET',
        data: {
            idDocente: idDocente,
            idPeriodo: $("#pdPeriodo").val()
        },

        success: function (response) {
            let clases = response.esCorrecto ? (response.dato || []) : [];

            $("#pdContador").text(response.esCorrecto ? textoContador(clases.length) : '');

            pintarCuadricula("#pdCuadricula", clases, response.mensaje, formatoClaseDocente);
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los horarios del docente.", 'danger');
        }
    });
}