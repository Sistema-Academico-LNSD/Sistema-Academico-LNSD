let materialListado = [];
let recursoDetalle = null;

$(document).ready(function () {
    cargarFiltros();

    // HU 01-08, esc. 3: al cambiar un filtro la lista se actualiza sola
    $("#mtCurso, #mtGrado").on("change", cargarMaterial);

    $("#btnLimpiarMaterial").on("click", function () {
        $("#mtCurso").val('');
        cargarMaterial();
    });

    $("#tblMaterial").on("click", ".btn-detalle-material", function () {
        abrirDetalle(parseInt($(this).data("id")));
    });

    $("#tblMaterial").on("click", ".btn-descargar-material", function () {
        descargarRecurso(parseInt($(this).data("id")), $(this).attr("data-nombre"));
    });

    $("#btnDetalleDescargar").on("click", function () {
        if (recursoDetalle) {
            descargarRecurso(recursoDetalle.idRecurso, recursoDetalle.nombreArchivo);
        }
    });
});

function mostrarVacio(mensaje) {
    $("#tblMaterial tbody").html(`
        <tr>
            <td colspan="6" class="text-center text-muted">${escapeHtml(mensaje)}</td>
        </tr>`);
}

// ---------- Filtros ----------

function cargarFiltros() {
    $.ajax({
        url: '/Recurso/GetFiltrosMaterial',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                $("#mtGrado, #mtCurso, #btnLimpiarMaterial").prop('disabled', true);
                mostrarVacio(response.mensaje);
                return;
            }

            let filtros = response.dato;

            // El estudiante solo ve el material de su grado
            $("#mtGrado").html(
                `<option value="${filtros.idGrado}">${escapeHtml(filtros.nombreGrado)}</option>`);

            let opciones = '<option value="">Todos los cursos</option>';

            $.each(filtros.cursos, function (index, c) {
                opciones += `<option value="${c.idCurso}">${escapeHtml(c.codigo)} - ${escapeHtml(c.nombre)}</option>`;
            });

            $("#mtCurso").html(opciones);

            cargarMaterial();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los filtros.", 'danger');
        }
    });
}

// ---------- Listado ----------

function cargarMaterial() {
    $.ajax({
        url: '/Recurso/GetMaterial',
        type: 'GET',
        data: { idCurso: $("#mtCurso").val() },

        success: function (response) {
            if (!response.esCorrecto) {
                materialListado = [];
                mostrarMensaje(response.mensaje, 'warning');
                mostrarVacio(response.mensaje);
                return;
            }

            materialListado = response.dato || [];

            if (materialListado.length === 0) {
                mostrarVacio(response.mensaje);
                return;
            }

            let filas = '';

            $.each(materialListado, function (index, r) {
                let descripcion = r.descripcion
                    ? `<br><small class="text-muted">${escapeHtml(r.descripcion)}</small>`
                    : '';

                let accion = r.modalidad === 'Archivo'
                    ? `<button type="button" class="btn btn-sm btn-primary btn-descargar-material"
                               data-id="${r.idRecurso}" data-nombre="${escapeHtml(r.nombreArchivo ?? '')}"
                               title="Descargar">
                           <i class="bi bi-download"></i>
                       </button>`
                    : `<a class="btn btn-sm btn-primary" href="${escapeHtml(r.urlEnlace ?? '')}"
                          target="_blank" rel="noopener noreferrer" title="Abrir enlace">
                           <i class="bi bi-box-arrow-up-right"></i>
                       </a>`;

                filas += `
                    <tr>
                        <td><strong>${escapeHtml(r.titulo)}</strong>${descripcion}</td>
                        <td>${escapeHtml(r.codigoCurso)} - ${escapeHtml(r.nombreCurso)}</td>
                        <td>${escapeHtml(r.tipoRecurso)}</td>
                        <td>${escapeHtml(r.nombreDocente)}</td>
                        <td>${formatearFechaPublicacion(r.fechaPublicacion)}</td>
                        <td class="text-nowrap">
                            <button type="button" class="btn btn-sm btn-outline-secondary btn-detalle-material"
                                    data-id="${r.idRecurso}" title="Ver detalle">
                                <i class="bi bi-eye"></i>
                            </button>
                            ${accion}
                        </td>
                    </tr>`;
            });

            $("#tblMaterial tbody").html(filas);
        },

        error: function () {
            mostrarMensaje("No fue posible consultar el material.", 'danger');
        }
    });
}

// ---------- Detalle (HU 01-09, esc. 3) ----------

function abrirDetalle(idRecurso) {
    let r = materialListado.find(x => x.idRecurso === idRecurso);

    if (!r) {
        return;
    }

    recursoDetalle = r;

    let esArchivo = r.modalidad === 'Archivo';

    $("#dtTitulo").text(r.titulo);
    $("#dtDescripcion").text(r.descripcion ?? 'Sin descripción');
    $("#dtCurso").text(`${r.codigoCurso} - ${r.nombreCurso}`);
    $("#dtGrado").text(r.nombreGrado);
    $("#dtTipo").text(r.tipoRecurso);
    $("#dtDocente").text(r.nombreDocente);
    $("#dtFecha").text(formatearFechaPublicacion(r.fechaPublicacion));

    if (esArchivo) {
        $("#dtContenido").text(`${r.nombreArchivo} (${formatearTamano(r.tamanoBytes)})`);
    }
    else {
        $("#dtContenido").text(r.urlEnlace);
    }

    $("#btnDetalleDescargar").toggleClass('d-none', !esArchivo);
    $("#lnkDetalleAbrir")
        .toggleClass('d-none', esArchivo)
        .attr("href", esArchivo ? '#' : r.urlEnlace);

    bootstrap.Modal
        .getOrCreateInstance(document.getElementById('modalDetalleMaterial'))
        .show();
}