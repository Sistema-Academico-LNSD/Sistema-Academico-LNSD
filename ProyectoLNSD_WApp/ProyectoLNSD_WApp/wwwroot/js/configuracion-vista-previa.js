// Vista previa del sitio (MCFG-01-07): confirmar la publicación de todo lo pendiente.
$(function () {
    $("#btnConfirmarPublicacion").on("click", async function () {
        const ok = await confirmar(
            "Se publicarán en el sitio web todos los cambios pendientes. ¿Confirmar la publicación?",
            "Publicar", "success");

        if (!ok) {
            return;
        }

        $.ajax({
            url: '/Configuracion/PublicarPendientes',
            type: 'POST',
            data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() },

            success: function (response) {
                if (response.esCorrecto) {
                    mostrarMensaje(response.mensaje, 'success');

                    // Si quedó algo sin publicar por estar incompleto, se deja el mensaje a la vista
                    if (response.mensaje.indexOf("No se publicaron") < 0) {
                        setTimeout(function () { location.reload(); }, 1500);
                    }
                }
                else {
                    mostrarMensaje(response.mensaje, 'danger');
                }
            },

            error: function () {
                mostrarMensaje("Error al publicar los cambios.", 'danger');
            }
        });
    });
});
