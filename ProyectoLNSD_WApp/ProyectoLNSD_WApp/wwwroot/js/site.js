// -------------------------------------------------------------
// Utilidades globales del sitio (Carga en _Layout antes que los scripts de cada página).
// Inyecta en todas las peticiones POST de una vez los hearders automaticamente , para evitar tener que hacerlo en cada llamada ajax.
// -------------------------------------------------------------


$.ajaxSetup({
    beforeSend: function (xhr, settings) {
        let metodo = (settings.type || 'GET').toUpperCase();

        if (metodo === 'GET') {
            return;
        }

        let token = $("input[name='__RequestVerificationToken']").first().val();

        if (token) {
            xhr.setRequestHeader('X-CSRF-TOKEN', token);
        }
    }
});

function escapeHtml(texto) {
    return $('<div>').text(texto ?? '').html();
}

// -------------------------------------------------------------
// Feedback visual unificado
// -------------------------------------------------------------

/**
 * Notificación tipo "toast" en la esquina superior derecha.
 * @param {string} mensaje
 * @param {'success'|'danger'|'warning'|'info'} tipo
 */
function mostrarMensaje(mensaje, tipo = 'success') {
    let $contenedor = $('#toastContainer');

    if ($contenedor.length === 0) {
        $contenedor = $('<div id="toastContainer" class="toast-container position-fixed top-0 end-0 p-3"></div>')
            .css('z-index', 1090)
            .appendTo('body');
    }

    const iconos = {
        success: 'bi-check-circle-fill',
        danger: 'bi-exclamation-triangle-fill',
        warning: 'bi-exclamation-circle-fill',
        info: 'bi-info-circle-fill'
    };

    let $toast = $(`
        <div class="toast align-items-center text-bg-${tipo} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    <i class="bi ${iconos[tipo] ?? iconos.info}"></i>
                    <span class="toast-texto"></span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Cerrar"></button>
            </div>
        </div>`);

    $toast.find('.toast-texto').text(mensaje ?? '');

    $contenedor.append($toast);

    let toast = new bootstrap.Toast($toast[0], { delay: tipo === 'danger' ? 6000 : 3500 });

    $toast.on('hidden.bs.toast', function () { $toast.remove(); });

    toast.show();
}

/**
 * Confirmación con modal de Bootstrap.
 */
function confirmar(mensaje, textoBoton = 'Confirmar', tipoBoton = 'danger') {
    return new Promise(function (resolve) {
        let $modal = $(`
            <div class="modal fade" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <i class="bi bi-question-circle"></i> Confirmar acción
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                        </div>
                        <div class="modal-body"></div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-${tipoBoton} btn-confirmar"></button>
                        </div>
                    </div>
                </div>
            </div>`);

        $modal.find('.modal-body').text(mensaje);
        $modal.find('.btn-confirmar').text(textoBoton);

        let confirmado = false;
        let modal = new bootstrap.Modal($modal[0]);

        $modal.find('.btn-confirmar').on('click', function () {
            confirmado = true;
            modal.hide();
        });

        $modal.on('hidden.bs.modal', function () {
            $modal.remove();
            resolve(confirmado);
        });

        $modal.appendTo('body');
        modal.show();
    });

    
// ============================================================
// Fechas UTC renderizadas en servidor
// El navegador convierte la fecha UTC a la hora local del usuario.
// ============================================================
$(function ()
{
    $("[data-fecha-utc]").each(function ()
    {
        let fecha = new Date($(this).attr("data-fecha-utc"));

        if (isNaN(fecha))
        {
            return;
        }

        let opciones = $(this).attr("data-formato") === "fecha-hora"
            ? { dateStyle: 'medium', timeStyle: 'short' }
            : { dateStyle: 'medium' };

        $(this).text(fecha.toLocaleString('es-CR', opciones));
    });
});

}