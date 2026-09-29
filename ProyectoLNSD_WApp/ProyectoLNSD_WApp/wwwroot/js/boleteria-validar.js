// Datos del tiquete que pasó la validación. "Marcar como utilizado" usa
// estos valores y no lo que haya en las cajas de texto en ese momento.
let tiqueteValidado = null;

$(document).ready(function () {
    $("#btnValidar").click(validarTiquete);

    $("#txtCodigo, #txtCorreo").on('keydown', function (e) {
        if (e.key === 'Enter') {
            validarTiquete();
        }
    });

    // Si cambian los datos después de validar, el resultado anterior deja de aplicar
    $("#txtCodigo, #txtCorreo").on('input', function () {
        tiqueteValidado = null;
        $("#resultadoValidacion").html('');
    });

    $(document).on('click', '#btnMarcarUtilizado', marcarUtilizado);
});

function mostrarError(mensaje) {
    $("#resultadoValidacion").html(`
        <div class="alert alert-danger d-flex align-items-center gap-2 mb-0">
            <i class="bi bi-x-circle-fill fs-4"></i>
            <div>${escapeHtml(mensaje)}</div>
        </div>`);
}

function pintarValido(tiquete) {
    $("#resultadoValidacion").html(`
        <div class="alert alert-success d-flex align-items-start gap-2">
            <i class="bi bi-person-check fs-4"></i>
            <div>
                <strong>${escapeHtml(tiquete.usuarioNombre)} ${escapeHtml(tiquete.usuarioApellido)}</strong>
                — ${escapeHtml(tiquete.usuarioCorreo)}<br>
                Tiquete <span class="font-monospace">${escapeHtml(tiquete.codigo)}</span>
                válido y disponible. Puede continuar con el ingreso.
            </div>
        </div>

        <button type="button" id="btnMarcarUtilizado" class="btn btn-success">
            <i class="bi bi-check2-circle"></i>
            Marcar como utilizado
        </button>`);
}

function limpiarFormulario() {
    tiqueteValidado = null;
    $("#txtCodigo").val('');
    $("#txtCorreo").val('');
    $("#resultadoValidacion").html('');
    $("#txtCodigo").focus();
}

// ---------- MBLF-01-04: validar ----------

function validarTiquete() {
    if ($("#btnValidar").prop('disabled')) {
        return;
    }

    let codigo = $("#txtCodigo").val().trim().toUpperCase();
    let correo = $("#txtCorreo").val().trim();

    tiqueteValidado = null;

    if (!codigo || !correo) {
        mostrarError("Debe indicar el código del tiquete y el correo de quien lo presenta.");
        return;
    }

    $("#btnValidar").prop('disabled', true);

    $.ajax({
        url: '/Boleteria/ValidarTiquete',
        type: 'POST',
        data: { codigo: codigo, correo: correo },

        success: function (response) {
            if (response.esCorrecto && response.dato != null) {
                tiqueteValidado = { codigo: codigo, correo: correo };
                pintarValido(response.dato);
            }
            else {
                mostrarError(response.mensaje);
            }
        },

        error: function () {
            mostrarError("No fue posible validar el tiquete.");
        },

        complete: function () {
            $("#btnValidar").prop('disabled', false);
        }
    });
}

// ---------- MBLF-01-05: marcar como utilizado ----------

function marcarUtilizado() {
    if (tiqueteValidado == null) {
        return;
    }

    $("#btnMarcarUtilizado").prop('disabled', true);

    $.ajax({
        url: '/Boleteria/MarcarUtilizado',
        type: 'POST',
        data: tiqueteValidado,

        success: function (response) {
            if (response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'success');
                limpiarFormulario();
            }
            else {
                tiqueteValidado = null;
                mostrarError(response.mensaje);
            }
        },

        error: function () {
            tiqueteValidado = null;
            mostrarError("No se pudo actualizar el estado. El tiquete conserva su estado anterior.");
        }
    });
}