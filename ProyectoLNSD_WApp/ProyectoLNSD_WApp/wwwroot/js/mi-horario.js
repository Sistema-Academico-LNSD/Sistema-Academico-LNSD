$(document).ready(function () {
    cargarPeriodosConsulta("#mhPeriodo", cargarMiHorario);

    $("#mhPeriodo").on("change", cargarMiHorario);
});

function cargarMiHorario() {
    $.ajax({
        url: '/Horario/GetMiHorario',
        type: 'GET',
        data: { idPeriodo: $("#mhPeriodo").val() },

        success: function (response) {
            // Si no es un docente activo o no hay período, la lista queda vacía y se muestra el motivo
            let clases = response.esCorrecto ? (response.dato || []) : [];

            $("#mhContador").text(response.esCorrecto ? textoContador(clases.length) : '');

            pintarCuadricula("#mhCuadricula", clases, response.mensaje, formatoClaseDocente);
        },

        error: function () {
            mostrarMensaje("No fue posible consultar su horario.", 'danger');
        }
    });
}