let supervisionListado = [];

$(document).ready(function () {
    cargarFiltros();

    // HU 01-10, esc. 2 y 3: al elegir un docente o un curso la lista se actualiza sola
    $("#svDocente, #svCurso").on("change", cargarSupervision);

    $("#btnLimpiarSupervision").on("click", function () {
        $("#svDocente").val('');
        $("#svCurso").val('');
        cargarSupervision();
    });

    $("#btnExportarSupervision").on("click", exportarReporte);

    $("#tblSupervision").on("click", ".btn-descargar-sv", function () {
        descargarRecurso(parseInt($(this).data("id")), $(this).attr("data-nombre"));
    });
});

function mostrarVacio(mensaje) {
    $("#tblSupervision tbody").html(`
        <tr>
            <td colspan="7" class="text-center text-muted">${escapeHtml(mensaje)}</td>
        </tr>`);
}

// ---------- Filtros ----------

function cargarFiltros() {
    $.ajax({
        url: '/Recurso/GetFiltrosSupervision',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let opcionesDocente = '<option value="">Todos los docentes</option>';

            $.each(response.dato.docentes, function (index, d) {
                opcionesDocente += `<option value="${d.id}">${escapeHtml(d.nombre)}</option>`;
            });

            let opcionesCurso = '<option value="">Todos los cursos</option>';

            $.each(response.dato.cursos, function (index, c) {
                opcionesCurso += `<option value="${c.id}">${escapeHtml(c.nombre)}</option>`;
            });

            $("#svDocente").html(opcionesDocente);
            $("#svCurso").html(opcionesCurso);

            cargarSupervision();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los filtros.", 'danger');
        }
    });
}

// ---------- Consulta (HU 01-10, esc. 1 a 4) ----------

function cargarSupervision() {
    $.ajax({
        url: '/Recurso/GetRecursosSupervision',
        type: 'GET',
        data: {
            idDocente: $("#svDocente").val(),
            idCurso: $("#svCurso").val()
        },

        success: function (response) {
            if (!response.esCorrecto) {
                supervisionListado = [];
                $("#svResumen").text('');
                mostrarMensaje(response.mensaje, 'danger');
                mostrarVacio(response.mensaje);
                return;
            }

            supervisionListado = response.dato || [];

            if (supervisionListado.length === 0) {
                $("#svResumen").text('0 recursos');
                mostrarVacio(response.mensaje);
                return;
            }

            // Los recursos ya vienen ordenados por docente y curso; se agrupan con un encabezado por docente
            let porDocente = {};

            $.each(supervisionListado, function (index, r) {
                porDocente[r.idDocente] = (porDocente[r.idDocente] || 0) + 1;
            });

            let totalDocentes = Object.keys(porDocente).length;

            $("#svResumen").text(
                `${supervisionListado.length} ${supervisionListado.length === 1 ? 'recurso' : 'recursos'} · ` +
                `${totalDocentes} ${totalDocentes === 1 ? 'docente' : 'docentes'}`);

            let filas = '';
            let docenteActual = null;

            $.each(supervisionListado, function (index, r) {
                if (r.idDocente !== docenteActual) {
                    docenteActual = r.idDocente;

                    filas += `
                        <tr class="table-secondary">
                            <td colspan="7">
                                <i class="bi bi-person-badge"></i>
                                <strong>${escapeHtml(r.nombreDocente)}</strong>
                                <span class="badge bg-secondary ms-2">${porDocente[r.idDocente]}</span>
                            </td>
                        </tr>`;
                }

                filas += `
                    <tr>
                        <td>${escapeHtml(r.codigoCurso)} - ${escapeHtml(r.nombreCurso)}</td>
                        <td>${escapeHtml(r.nombreGrado)}</td>
                        <td>
                            <strong>${escapeHtml(r.titulo)}</strong>
                            ${r.descripcion ? `<br><small class="text-muted">${escapeHtml(r.descripcion)}</small>` : ''}
                        </td>
                        <td>${escapeHtml(r.tipoRecurso)}</td>
                        <td>${contenidoRecurso(r)}</td>
                        <td>${formatearFechaPublicacion(r.fechaPublicacion)}</td>
                        <td class="text-nowrap">${accionRecurso(r)}</td>
                    </tr>`;
            });

            $("#tblSupervision tbody").html(filas);
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los recursos.", 'danger');
        }
    });
}

function contenidoRecurso(r) {
    if (r.modalidad === 'Archivo') {
        return `<i class="bi bi-file-earmark"></i> ${escapeHtml(r.nombreArchivo ?? '')}<br>
                <small class="text-muted">${formatearTamano(r.tamanoBytes)}</small>`;
    }

    return `<i class="bi bi-link-45deg"></i>
            <span class="d-inline-block text-truncate align-bottom" style="max-width: 220px;">
                ${escapeHtml(r.urlEnlace ?? '')}
            </span>`;
}

function accionRecurso(r) {
    if (r.modalidad === 'Archivo') {
        return `<button type="button" class="btn btn-sm btn-outline-primary btn-descargar-sv"
                        data-id="${r.idRecurso}" data-nombre="${escapeHtml(r.nombreArchivo ?? '')}"
                        title="Descargar">
                    <i class="bi bi-download"></i>
                </button>`;
    }

    return `<a class="btn btn-sm btn-outline-primary" href="${escapeHtml(r.urlEnlace ?? '')}"
               target="_blank" rel="noopener noreferrer" title="Abrir enlace">
                <i class="bi bi-box-arrow-up-right"></i>
            </a>`;
}

// ---------- Exportar (HU 01-10, esc. 5) ----------

function exportarReporte() {
    if (supervisionListado.length === 0) {
        mostrarMensaje("No hay registros para exportar con los filtros seleccionados.", 'warning');
        return;
    }

    // El reporte usa los mismos filtros que la tabla
    let consulta = $.param({
        idDocente: $("#svDocente").val(),
        idCurso: $("#svCurso").val()
    });

    let fecha = new Date().toLocaleDateString('en-CA').replaceAll('-', '');

    descargarDesde('/Recurso/ExportarReporte?' + consulta, `Recursos_Academicos_${fecha}.xlsx`);
}