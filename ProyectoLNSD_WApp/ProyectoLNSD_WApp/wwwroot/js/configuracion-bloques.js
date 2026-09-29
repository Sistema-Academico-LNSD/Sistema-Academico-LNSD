// HU 04: contenido de la página principal (bloques)

$(document).ready(function () {
    cargarBloques();

    $("#btnNuevoBloque").click(function () {
        abrirModalBloque();
    });

    $("#btnBloqueBorrador").click(function () {
        guardarBloque(false);
    });

    $("#btnBloquePublicar").click(function () {
        guardarBloque(true);
    });
});

function cargarBloques() {
    $.ajax({
        url: '/Configuracion/GetBloques',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let filas = '';

            if (!response.dato || response.dato.length === 0) {
                filas = '<tr><td colspan="5" class="text-center text-muted">No hay contenido registrado.</td></tr>';
            }
            else {
                $.each(response.dato, function (index, b) {
                    let estado = b.estado === 'Publicado'
                        ? '<span class="badge bg-success">Publicado</span>'
                        : '<span class="badge bg-warning text-dark">Borrador</span>';

                    // HU 08: interruptor publicar / despublicar
                    let accionEstado = b.estado === 'Publicado'
                        ? `<button class="btn btn-sm btn-outline-secondary" onclick="cambiarEstadoContenido(${b.idContenido}, false, cargarBloques)">
                                <i class="bi bi-eye-slash"></i> Despublicar
                           </button>`
                        : `<button class="btn btn-sm btn-outline-success" onclick="cambiarEstadoContenido(${b.idContenido}, true, cargarBloques)">
                                <i class="bi bi-cloud-upload"></i> Publicar
                           </button>`;

                    filas += `
                        <tr>
                            <td>${b.orden}</td>
                            <td>${escapeHtml(b.titulo)}</td>
                            <td>${estado}</td>
                            <td>${new Date(b.fechaActualizacion + "Z").toLocaleString()}</td>
                            <td class="text-end">
                                ${accionEstado}
                                <button class="btn btn-sm btn-outline-primary" onclick="editarBloque(${b.idContenido})">
                                    <i class="bi bi-pencil"></i> Editar
                                </button>
                                <button class="btn btn-sm btn-outline-danger" onclick="eliminarBloque(${b.idContenido})">
                                    <i class="bi bi-trash"></i> Eliminar
                                </button>
                            </td>
                        </tr>`;
                });
            }

            $("#tbodyBloques").html(filas);
        },

        error: function () {
            mostrarMensaje("Error al cargar el contenido de la página principal.", 'danger');
        }
    });
}

function abrirModalBloque() {
    $("#formBloque")[0].reset();
    $("#bloqueId").val(0);
    $("#bloqueEstado").val("");
    $("#tituloModalBloque").text("Nuevo contenido");
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBloque')).show();
}

function editarBloque(id) {
    $.ajax({
        url: '/Configuracion/GetBloque',
        type: 'GET',
        data: { id: id },

        success: function (response) {
            if (!response.esCorrecto || !response.dato) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let b = response.dato;
            $("#bloqueId").val(b.idContenido);
            $("#bloqueEstado").val(b.estado);
            $("#bloqueTitulo").val(b.titulo);
            $("#bloqueDescripcion").val(b.descripcion);
            $("#bloqueOrden").val(b.orden);
            $("#tituloModalBloque").text("Editar contenido");

            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBloque')).show();
        },

        error: function () {
            mostrarMensaje("Error al cargar el contenido.", 'danger');
        }
    });
}

function guardarBloque(publicar) {
    // HU 08 escenario 5: publicar cambios de un contenido ya publicado pide confirmación.
    if (publicar && $("#bloqueEstado").val() === "Publicado") {
        if (!confirm("Se actualizará el contenido visible en el sitio web. ¿Confirmar la publicación?")) {
            return;
        }
    }

    // Guardar como borrador un contenido ya publicado lo oculta del sitio: se pide confirmar.
    if (!publicar && $("#bloqueEstado").val() === "Publicado") {
        if (!confirm("Este contenido está publicado. Si lo guarda como borrador dejará de mostrarse en el sitio. ¿Desea continuar?")) {
            return;
        }
    }

    let datos = new FormData($("#formBloque")[0]);
    datos.append("publicar", publicar);

    $.ajax({
        url: '/Configuracion/GuardarBloque',
        type: 'POST',
        data: datos,
        processData: false,
        contentType: false,

        success: function (response) {
            if (response.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalBloque')).hide();
                cargarBloques();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al guardar el contenido.", 'danger');
        }
    });
}

function eliminarBloque(id) {
    if (!confirm("¿Seguro que desea eliminar este contenido? Se quitará también del sitio web.")) {
        return;
    }

    $.ajax({
        url: '/Configuracion/EliminarBloque',
        type: 'POST',
        data: { id: id },

        success: function (response) {
            if (response.esCorrecto) {
                cargarBloques();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al eliminar el contenido.", 'danger');
        }
    });
}
