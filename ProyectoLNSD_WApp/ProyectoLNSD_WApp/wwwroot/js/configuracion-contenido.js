// HU 05: Misión, Visión e Historia
// Estado actual de cada texto ("Publicado" | "Borrador" | undefined si aún no existe)
let estadosContenido = {};

$(document).ready(function () {
    cargarContenidosInstitucionales();

    $(".btn-guardar-contenido").click(function () {
        let tipo = $(this).data("tipo");
        let publicar = String($(this).data("publicar")) === "true";
        guardarContenidoInstitucional(tipo, publicar);
    });
});

function pintarEstadoContenido(tipo, estado) {
    let $badge = $("#estado-" + tipo);

    if (estado === "Publicado") {
        $badge.attr("class", "badge bg-success").text("Publicado");
    }
    else if (estado === "Borrador") {
        $badge.attr("class", "badge bg-warning text-dark").text("Borrador");
    }
    else {
        $badge.attr("class", "badge bg-secondary").text("Sin contenido");
    }
}

function cargarContenidosInstitucionales() {
    $.ajax({
        url: '/Configuracion/GetContenidosInstitucionales',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            estadosContenido = {};

            $.each(response.dato, function (index, c) {
                estadosContenido[c.tipo] = c.estado;
                $("#texto-" + c.tipo).val(c.descripcion);
                pintarEstadoContenido(c.tipo, c.estado);

                if (c.fechaActualizacion) {
                    $("#fecha-" + c.tipo).text("Última actualización: " +
                        new Date(c.fechaActualizacion + "Z").toLocaleString());
                }
            });
        },

        error: function () {
            mostrarMensaje("Error al cargar el contenido institucional.", 'danger');
        }
    });
}

function guardarContenidoInstitucional(tipo, publicar) {
    // Guardar como borrador un texto ya publicado lo oculta del sitio: se pide confirmar.
    if (!publicar && estadosContenido[tipo] === "Publicado") {
        if (!confirm("Este texto está publicado. Si lo guarda como borrador dejará de mostrarse en el sitio. ¿Desea continuar?")) {
            return;
        }
    }

    $.ajax({
        url: '/Configuracion/GuardarContenidoInstitucional',
        type: 'POST',
        data: {
            Tipo: tipo,
            Descripcion: $("#texto-" + tipo).val(),
            publicar: publicar
        },

        success: function (response) {
            if (response.esCorrecto) {
                cargarContenidosInstitucionales();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al guardar el contenido institucional.", 'danger');
        }
    });
}
