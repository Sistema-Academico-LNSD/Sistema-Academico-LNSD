// HU 03: imágenes y banners

$(document).ready(function () {
    cargarBanners();

    $("#btnNuevoBanner").click(function () {
        abrirModalBanner();
    });

    $("#btnBannerBorrador").click(function () {
        guardarBanner(false);
    });

    $("#btnBannerPublicar").click(function () {
        guardarBanner(true);
    });

    // Vista previa local de la imagen elegida
    $("#bannerImagen").change(function () {
        let archivo = this.files[0];
        if (!archivo) return;

        let lector = new FileReader();
        lector.onload = function (e) {
            $("#bannerPreview").attr("src", e.target.result).removeClass("d-none");
        };
        lector.readAsDataURL(archivo);
    });
});

function cargarBanners() {
    $.ajax({
        url: '/Configuracion/GetBanners',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let filas = '';

            if (!response.dato || response.dato.length === 0) {
                filas = '<tr><td colspan="6" class="text-center text-muted">No hay banners registrados.</td></tr>';
            }
            else {
                $.each(response.dato, function (index, b) {
                    let estado = b.estado === 'Publicado'
                        ? '<span class="badge bg-success">Publicado</span>'
                        : '<span class="badge bg-warning text-dark">Borrador</span>';

                    // HU 08: interruptor publicar / despublicar
                    let accionEstado = b.estado === 'Publicado'
                        ? `<button class="btn btn-sm btn-outline-secondary" onclick="cambiarEstadoContenido(${b.idContenido}, false, cargarBanners)">
                                <i class="bi bi-eye-slash"></i> Despublicar
                           </button>`
                        : `<button class="btn btn-sm btn-outline-success" onclick="cambiarEstadoContenido(${b.idContenido}, true, cargarBanners)">
                                <i class="bi bi-cloud-upload"></i> Publicar
                           </button>`;

                    filas += `
                        <tr>
                            <td><img src="${escapeHtml(b.rutaImagen)}" alt="" style="height:48px; max-width:120px; object-fit:cover;" class="rounded" /></td>
                            <td>${escapeHtml(b.titulo)}</td>
                            <td>${b.orden}</td>
                            <td>${estado}</td>
                            <td>${new Date(b.fechaActualizacion + "Z").toLocaleString()}</td>
                            <td class="text-end">
                                ${accionEstado}
                                <button class="btn btn-sm btn-outline-primary" onclick="editarBanner(${b.idContenido})">
                                    <i class="bi bi-pencil"></i> Editar
                                </button>
                                <button class="btn btn-sm btn-outline-danger" onclick="eliminarBanner(${b.idContenido})">
                                    <i class="bi bi-trash"></i> Eliminar
                                </button>
                            </td>
                        </tr>`;
                });
            }

            $("#tbodyBanners").html(filas);
        },

        error: function () {
            mostrarMensaje("Error al cargar los banners.", 'danger');
        }
    });
}

function abrirModalBanner() {
    $("#formBanner")[0].reset();
    $("#bannerId").val(0);
    $("#bannerEstado").val("");
    $("#bannerPreview").attr("src", "").addClass("d-none");
    $("#tituloModalBanner").text("Nuevo banner");
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBanner')).show();
}

function editarBanner(id) {
    $.ajax({
        url: '/Configuracion/GetBanner',
        type: 'GET',
        data: { id: id },

        success: function (response) {
            if (!response.esCorrecto || !response.dato) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let b = response.dato;
            $("#formBanner")[0].reset();   // limpia el input de archivo
            $("#bannerId").val(b.idContenido);
            $("#bannerEstado").val(b.estado);
            $("#bannerTitulo").val(b.titulo);
            $("#bannerDescripcion").val(b.descripcion);
            $("#bannerOrden").val(b.orden);
            $("#bannerPreview").attr("src", b.rutaImagen || "").toggleClass("d-none", !b.rutaImagen);
            $("#tituloModalBanner").text("Editar banner");

            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBanner')).show();
        },

        error: function () {
            mostrarMensaje("Error al cargar el banner.", 'danger');
        }
    });
}

function guardarBanner(publicar) {
    // HU 08 escenario 5: publicar cambios de un banner ya publicado pide confirmación.
    if (publicar && $("#bannerEstado").val() === "Publicado") {
        if (!confirm("Se actualizará el contenido visible en el sitio web. ¿Confirmar la publicación?")) {
            return;
        }
    }

    // Guardar como borrador un banner ya publicado lo oculta del sitio: se pide confirmar.
    if (!publicar && $("#bannerEstado").val() === "Publicado") {
        if (!confirm("Este banner está publicado. Si lo guarda como borrador dejará de mostrarse en el sitio. ¿Desea continuar?")) {
            return;
        }
    }

    // FormData incluye los campos, el archivo "imagen" y el token antiforgery.
    let datos = new FormData($("#formBanner")[0]);
    datos.append("publicar", publicar);

    $.ajax({
        url: '/Configuracion/GuardarBanner',
        type: 'POST',
        data: datos,
        processData: false,   // necesario para enviar archivos
        contentType: false,

        success: function (response) {
            if (response.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBanner')).hide();
                cargarBanners();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al guardar el banner.", 'danger');
        }
    });
}

function eliminarBanner(id) {
    if (!confirm("¿Seguro que desea eliminar este banner? Se quitará también del sitio web.")) {
        return;
    }

    $.ajax({
        url: '/Configuracion/EliminarBanner',
        type: 'POST',
        data: { id: id },

        success: function (response) {
            if (response.esCorrecto) {
                cargarBanners();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al eliminar el banner.", 'danger');
        }
    });
}
