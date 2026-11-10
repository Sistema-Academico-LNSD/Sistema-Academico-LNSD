// HU 08: publicar / despublicar contenido del sitio (misión, visión, historia, bloques y banners)
// alTerminar: función que vuelve a cargar la lista o la tarjeta que corresponda.

async function cambiarEstadoContenido(id, publicar, alTerminar) {
    let mensajeConfirmacion = publicar
        ? "¿Desea publicar este contenido en el sitio web?"
        : "¿Desea despublicar este contenido? Dejará de mostrarse en el sitio web.";

    if (!(await confirmar(mensajeConfirmacion, publicar ? "Publicar" : "Despublicar", publicar ? "success" : "warning"))) {
        return;
    }

    $.ajax({
        url: '/Configuracion/CambiarEstadoContenido',
        type: 'POST',
        data: { id: id, publicar: publicar },

        success: function (response) {
            if (response.esCorrecto) {
                if (alTerminar) alTerminar();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                // Incluye el caso de contenido incompleto: el mensaje lista los campos que faltan
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al cambiar el estado del contenido.", 'danger');
        }
    });
}
