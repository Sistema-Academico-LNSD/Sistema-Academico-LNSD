let cursosDocente = [];

$(document).ready(function () {
    $("#recArchivo").attr("accept", RECURSO_CONFIG.extensiones.join(","));
    $("#edArchivo").attr("accept", RECURSO_CONFIG.extensiones.join(","));

    reiniciarFecha();
    actualizarModalidad();
    cargarMisCursos();
    cargarMisRecursos();

    // Publicar
    $("#formRecurso input[name='Modalidad']").on("change", actualizarModalidad);
    $("#recCurso").on("change", cargarGrados);

    $("#formRecurso").on("submit", function (e) {
        e.preventDefault();
        publicarRecurso();
    });

    $("#btnLimpiarRecurso").on("click", limpiarFormulario);

    // Mis recursos: búsqueda y orden
    $("#btnBuscarRecursos").on("click", cargarMisRecursos);
    $("#btnRestablecerRecursos").on("click", restablecerFiltros);
    $("#filtroCurso").on("change", llenarFiltroGrados);
    $("#filtroOrden").on("change", cargarMisRecursos);

    $("#filtroTitulo").on("keydown", function (e) {
        if (e.key === 'Enter') {
            cargarMisRecursos();
        }
    });

    // Mis recursos: editar y eliminar
    $("#tblRecursos").on("click", ".btn-editar-recurso", function () {
        abrirEditar(parseInt($(this).data("id")));
    });

    $("#tblRecursos").on("click", ".btn-eliminar-recurso", function () {
        eliminarRecurso(parseInt($(this).data("id")));
    });

    $("#formEditarRecurso").on("submit", function (e) {
        e.preventDefault();
        guardarEdicion();
    });

        $("#tblRecursos").on("click", ".btn-descargar-recurso", function () {
        descargarRecurso(parseInt($(this).data("id")), $(this).attr("data-nombre"));
    });
});

// ---------- Utilidades ----------

function modalidadActual() {
    return $("#formRecurso input[name='Modalidad']:checked").val();
}

// Fecha de hoy en hora local (YYYY-MM-DD). No se puede publicar con fecha futura.
function reiniciarFecha() {
    let hoy = new Date().toLocaleDateString('en-CA');

    $("#recFecha").val(hoy).attr("max", hoy);
}

function mensajeTamanoMaximo() {
    return `El archivo supera el tamaño máximo permitido de ${RECURSO_CONFIG.tamanoMaximo / (1024 * 1024)} MB.`;
}

// ---------- Formulario ----------

function actualizarModalidad() {
    let esArchivo = modalidadActual() === 'Archivo';

    $("#bloqueArchivo").toggleClass('d-none', !esArchivo);
    $("#bloqueEnlace").toggleClass('d-none', esArchivo);

    // Los campos deshabilitados no se envían con el formulario
    $("#recArchivo").prop('disabled', !esArchivo);
    $("#recUrl").prop('disabled', esArchivo);

    let tipos = esArchivo ? RECURSO_CONFIG.tiposArchivo : RECURSO_CONFIG.tiposEnlace;
    let opciones = '<option value="">Seleccione el tipo</option>';

    $.each(tipos, function (index, tipo) {
        opciones += `<option value="${escapeHtml(tipo)}">${escapeHtml(tipo)}</option>`;
    });

    $("#recTipo").html(opciones);
}

function bloquearFormulario(mensaje) {
    $("#avisoRecurso").html(`
        <div class="alert alert-warning mb-0">
            <i class="bi bi-exclamation-triangle"></i>
            ${escapeHtml(mensaje)}
        </div>`);

    $("#fsRecurso").prop('disabled', true);
    $("#recCurso").html('<option value="">Sin cursos disponibles</option>');
}

function cargarMisCursos() {
    $.ajax({
        url: '/Recurso/GetMisCursos',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto || !response.dato || response.dato.length === 0) {
                bloquearFormulario(response.mensaje);
                return;
            }

            cursosDocente = response.dato;

            let opciones = '<option value="">Seleccione un curso</option>';

            $.each(cursosDocente, function (index, c) {
                opciones += `<option value="${c.idCurso}">${escapeHtml(c.codigo)} - ${escapeHtml(c.nombre)}</option>`;
            });

            $("#recCurso").html(opciones);
            cargarGrados();

            $("#recCurso").html(opciones);
            cargarGrados();
            llenarFiltroCursos();
        },

        error: function () {
            mostrarMensaje("No fue posible obtener sus cursos.", 'danger');
        }
    });
}

// Los grados disponibles dependen del curso elegido
function cargarGrados() {
    let idCurso = parseInt($("#recCurso").val());
    let curso = cursosDocente.find(c => c.idCurso === idCurso);

    let opciones = '<option value="">Seleccione un grado</option>';

    if (curso) {
        $.each(curso.grados, function (index, g) {
            opciones += `<option value="${g.idGrado}">${escapeHtml(g.nombre)}</option>`;
        });
    }

    $("#recGrado").html(opciones);

    if (curso && curso.grados.length === 1) {
        $("#recGrado").val(curso.grados[0].idGrado);
    }
}

function limpiarFormulario() {
    $("#formRecurso")[0].reset();

    reiniciarFecha();
    actualizarModalidad();
    cargarGrados();
}

// ---------- Publicar ----------

function publicarRecurso() {
    let esArchivo = modalidadActual() === 'Archivo';

    // Validaciones rápidas en el navegador; el servidor las repite todas
    if (!$("#recCurso").val()) {
        mostrarMensaje("Seleccione un curso.", 'warning');
        return;
    }

    if (!$("#recGrado").val()) {
        mostrarMensaje("Seleccione un grado.", 'warning');
        return;
    }

    if (!$("#recTitulo").val().trim()) {
        mostrarMensaje("El título es requerido.", 'warning');
        return;
    }

    if (!$("#recTipo").val()) {
        mostrarMensaje("Seleccione el tipo de recurso.", 'warning');
        return;
    }

    if (esArchivo) {
        let archivo = $("#recArchivo")[0].files[0];

        if (!archivo) {
            mostrarMensaje("Debe seleccionar un archivo antes de continuar.", 'warning');
            return;
        }

        let extension = '.' + archivo.name.split('.').pop().toLowerCase();

        if (!RECURSO_CONFIG.extensiones.includes(extension)) {
            mostrarMensaje("Formato no permitido. Formatos admitidos: " + RECURSO_CONFIG.extensiones.join(", "), 'warning');
            return;
        }

        if (archivo.size > RECURSO_CONFIG.tamanoMaximo) {
            mostrarMensaje(mensajeTamanoMaximo(), 'warning');
            return;
        }
    }
    else if (!$("#recUrl").val().trim()) {
        mostrarMensaje("Ingrese el enlace del recurso.", 'warning');
        return;
    }

    let datos = new FormData($("#formRecurso")[0]);

    $("#btnPublicarRecurso").prop('disabled', true);

    $.ajax({
        url: '/Recurso/Publicar',
        type: 'POST',
        data: datos,
        processData: false,   // necesario para enviar archivos
        contentType: false,

        success: function (response) {
            if (response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'success');
                mostrarResumen(response.dato);
                limpiarFormulario();
                cargarMisRecursos();
            }
            else {
                // 1211 = el recurso ya existe en el curso (aviso, no error grave)
                mostrarMensaje(response.mensaje, response.codigo === 1211 ? 'warning' : 'danger');
            }
        },

        error: function (xhr) {
            if (xhr.status === 413) {
                mostrarMensaje(mensajeTamanoMaximo(), 'danger');
            }
            else {
                mostrarMensaje("No se pudo registrar el recurso. Intente nuevamente.", 'danger');
            }
        },

        complete: function () {
            $("#btnPublicarRecurso").prop('disabled', false);
        }
    });
}

// Vista previa del recurso recién publicado (HU 01-03, escenario 5)
function mostrarResumen(r) {
    let contenido = r.modalidad === 'Archivo'
        ? `<i class="bi bi-file-earmark"></i> ${escapeHtml(r.nombreArchivo)} (${formatearTamano(r.tamanoBytes)})`
        : `<i class="bi bi-link-45deg"></i> <a href="${escapeHtml(r.urlEnlace)}" target="_blank" rel="noopener noreferrer">${escapeHtml(r.urlEnlace)}</a>`;

    $("#resumenPublicacion").html(`
        <div class="card shadow-sm border-success">
            <div class="card-header bg-success text-white">
                <i class="bi bi-check-circle"></i> Recurso publicado
            </div>
            <div class="card-body">
                <h5 class="mb-1">${escapeHtml(r.titulo)}</h5>
                <p class="text-muted mb-2">${escapeHtml(r.descripcion ?? 'Sin descripción')}</p>
                <div class="row g-2">
                    <div class="col-md-4"><strong>Curso:</strong> ${escapeHtml(r.codigoCurso)} - ${escapeHtml(r.nombreCurso)}</div>
                    <div class="col-md-4"><strong>Grado:</strong> ${escapeHtml(r.nombreGrado)}</div>
                    <div class="col-md-4"><strong>Tipo:</strong> ${escapeHtml(r.tipoRecurso)}</div>
                    <div class="col-md-4"><strong>Docente:</strong> ${escapeHtml(r.nombreDocente)}</div>
                    <div class="col-md-4"><strong>Fecha:</strong> ${formatearFechaPublicacion(r.fechaPublicacion)}</div>
                    <div class="col-md-4">${contenido}</div>
                </div>
            </div>
        </div>`);


}

// =====================================================
// Mis recursos: filtros, tabla, editar y eliminar
// =====================================================

let recursosListados = [];

function contenidoRecurso(r) {
    if (r.modalidad === 'Archivo') {
        return `<i class="bi bi-file-earmark"></i> ${escapeHtml(r.nombreArchivo ?? '')}<br>
                <small class="text-muted">${formatearTamano(r.tamanoBytes)}</small>`;
    }

    return `<i class="bi bi-link-45deg"></i>
            <a href="${escapeHtml(r.urlEnlace ?? '')}" target="_blank" rel="noopener noreferrer"
               class="d-inline-block text-truncate align-bottom" style="max-width: 220px;">
                ${escapeHtml(r.urlEnlace ?? '')}
            </a>`;
}

// ---------- Filtros ----------

function llenarFiltroCursos() {
    let opciones = '<option value="">Todos los cursos</option>';

    $.each(cursosDocente, function (index, c) {
        opciones += `<option value="${c.idCurso}">${escapeHtml(c.codigo)} - ${escapeHtml(c.nombre)}</option>`;
    });

    $("#filtroCurso").html(opciones);
    llenarFiltroGrados();
}

// Con un curso elegido solo se ofrecen sus grados; sin curso, los de todos mis cursos
function llenarFiltroGrados() {
    let idCurso = parseInt($("#filtroCurso").val());
    let grados = [];

    if (idCurso) {
        let curso = cursosDocente.find(c => c.idCurso === idCurso);
        grados = curso ? curso.grados : [];
    }
    else {
        $.each(cursosDocente, function (i, c) {
            $.each(c.grados, function (j, g) {
                if (!grados.some(x => x.idGrado === g.idGrado)) {
                    grados.push(g);
                }
            });
        });
    }

    let seleccionado = $("#filtroGrado").val();
    let opciones = '<option value="">Todos los grados</option>';

    $.each(grados, function (index, g) {
        opciones += `<option value="${g.idGrado}">${escapeHtml(g.nombre)}</option>`;
    });

    $("#filtroGrado").html(opciones);

    if (seleccionado && grados.some(g => String(g.idGrado) === seleccionado)) {
        $("#filtroGrado").val(seleccionado);
    }
}

function restablecerFiltros() {
    $("#filtroTitulo").val('');
    $("#filtroCurso").val('');
    $("#filtroDesde").val('');
    $("#filtroHasta").val('');
    $("#filtroOrden").val('fecha_desc');

    llenarFiltroGrados();
    $("#filtroGrado").val('');

    cargarMisRecursos();
}

// ---------- Listado (HU 01-06 y 01-07) ----------

function cargarMisRecursos() {
    let filtros = {
        idCurso: $("#filtroCurso").val(),
        idGrado: $("#filtroGrado").val(),
        titulo: $("#filtroTitulo").val().trim(),
        fechaDesde: $("#filtroDesde").val(),
        fechaHasta: $("#filtroHasta").val(),
        orden: $("#filtroOrden").val()
    };

    $.ajax({
        url: '/Recurso/GetMisRecursos',
        type: 'GET',
        data: filtros,

        success: function (response) {
            let filas = '';

            if (!response.esCorrecto) {
                recursosListados = [];
                $("#contadorRecursos").text('');

                mostrarMensaje(response.mensaje, 'warning');

                filas = `<tr><td colspan="7" class="text-center text-muted">${escapeHtml(response.mensaje)}</td></tr>`;
                $("#tblRecursos tbody").html(filas);
                return;
            }

            recursosListados = response.dato || [];

            let total = recursosListados.length;
            $("#contadorRecursos").text(total + (total === 1 ? ' recurso' : ' recursos'));

            if (total === 0) {
                filas = `<tr><td colspan="7" class="text-center text-muted">${escapeHtml(response.mensaje)}</td></tr>`;
            }
            else {
                $.each(recursosListados, function (index, r) {
                    let modificado = r.fechaModificacion
                        ? `<br><small class="text-muted">Editado: ${escapeHtml(formatearFechaHora(r.fechaModificacion))}</small>`
                        : '';

                    let descripcion = r.descripcion
                        ? `<br><small class="text-muted">${escapeHtml(r.descripcion)}</small>`
                        : '';

                    let botonDescarga = r.modalidad === 'Archivo'
                        ? `<button type="button" class="btn btn-sm btn-outline-secondary btn-descargar-recurso"
                                   data-id="${r.idRecurso}" data-nombre="${escapeHtml(r.nombreArchivo ?? '')}"
                                   title="Descargar">
                               <i class="bi bi-download"></i>
                           </button>`
                        : '';

                    filas += `
                        <tr>
                            <td><strong>${escapeHtml(r.titulo)}</strong>${descripcion}</td>
                            <td>${escapeHtml(r.codigoCurso)} - ${escapeHtml(r.nombreCurso)}</td>
                            <td>${escapeHtml(r.nombreGrado)}</td>
                            <td>${escapeHtml(r.tipoRecurso)}</td>
                            <td>${contenidoRecurso(r)}</td>
                            <td>${formatearFechaPublicacion(r.fechaPublicacion)}${modificado}</td>
                            <td class="text-nowrap">
                                ${botonDescarga}
                                <button type="button" class="btn btn-sm btn-outline-primary btn-editar-recurso"
                                        data-id="${r.idRecurso}" title="Editar">
                                    <i class="bi bi-pencil-square"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar-recurso"
                                        data-id="${r.idRecurso}" title="Eliminar">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </td>
                        </tr>`;
                });
            }

            $("#tblRecursos tbody").html(filas);
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los recursos.", 'danger');
        }
    });
}

// ---------- Editar (HU 01-04) ----------

function abrirEditar(idRecurso) {
    $.ajax({
        url: '/Recurso/GetRecurso',
        type: 'GET',
        data: { idRecurso: idRecurso },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                cargarMisRecursos();
                return;
            }

            llenarFormularioEdicion(response.dato);

            bootstrap.Modal
                .getOrCreateInstance(document.getElementById('modalEditarRecurso'))
                .show();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar el recurso.", 'danger');
        }
    });
}

function llenarFormularioEdicion(r) {
    let esArchivo = r.modalidad === 'Archivo';

    $("#edIdRecurso").val(r.idRecurso);
    $("#edIdCurso").val(r.idCurso);
    $("#edModalidad").val(r.modalidad);
    $("#edCurso").val(`${r.codigoCurso} - ${r.nombreCurso}`);
    $("#edTitulo").val(r.titulo);
    $("#edDescripcion").val(r.descripcion ?? '');

    let hoy = new Date().toLocaleDateString('en-CA');
    $("#edFecha").val(r.fechaPublicacion.substring(0, 10)).attr("max", hoy);

    // Grados del curso; el grado actual siempre debe estar entre las opciones
    let curso = cursosDocente.find(c => c.idCurso === r.idCurso);
    let grados = curso ? curso.grados.slice() : [];

    if (!grados.some(g => g.idGrado === r.idGrado)) {
        grados.unshift({ idGrado: r.idGrado, nombre: r.nombreGrado });
    }

    let opcionesGrado = '';

    $.each(grados, function (index, g) {
        opcionesGrado += `<option value="${g.idGrado}">${escapeHtml(g.nombre)}</option>`;
    });

    $("#edGrado").html(opcionesGrado).val(r.idGrado);

    // Los tipos dependen de la modalidad, que no cambia al editar
    let tipos = esArchivo ? RECURSO_CONFIG.tiposArchivo : RECURSO_CONFIG.tiposEnlace;
    let opcionesTipo = '';

    $.each(tipos, function (index, tipo) {
        opcionesTipo += `<option value="${escapeHtml(tipo)}">${escapeHtml(tipo)}</option>`;
    });

    $("#edTipo").html(opcionesTipo).val(r.tipoRecurso);

    $("#edBloqueArchivo").toggleClass('d-none', !esArchivo);
    $("#edBloqueEnlace").toggleClass('d-none', esArchivo);

    // Los campos deshabilitados no se envían con el formulario
    $("#edArchivo").prop('disabled', !esArchivo).val('');
    $("#edUrl").prop('disabled', esArchivo).val(esArchivo ? '' : (r.urlEnlace ?? ''));

    $("#edArchivoActual").text(
        esArchivo ? `Archivo actual: ${r.nombreArchivo} (${formatearTamano(r.tamanoBytes)})` : '');
}

function guardarEdicion() {
    let esArchivo = $("#edModalidad").val() === 'Archivo';

    if (!$("#edTitulo").val().trim()) {
        mostrarMensaje("El título es requerido.", 'warning');
        return;
    }

    if (!$("#edGrado").val()) {
        mostrarMensaje("Seleccione un grado.", 'warning');
        return;
    }

    if (!$("#edTipo").val()) {
        mostrarMensaje("Seleccione el tipo de recurso.", 'warning');
        return;
    }

    if (esArchivo) {
        let archivo = $("#edArchivo")[0].files[0];

        // El archivo nuevo es opcional; si se elige, se valida igual que al publicar
        if (archivo) {
            let extension = '.' + archivo.name.split('.').pop().toLowerCase();

            if (!RECURSO_CONFIG.extensiones.includes(extension)) {
                mostrarMensaje("Formato no permitido. Formatos admitidos: " + RECURSO_CONFIG.extensiones.join(", "), 'warning');
                return;
            }

            if (archivo.size > RECURSO_CONFIG.tamanoMaximo) {
                mostrarMensaje(mensajeTamanoMaximo(), 'warning');
                return;
            }
        }
    }
    else if (!$("#edUrl").val().trim()) {
        mostrarMensaje("Ingrese el enlace del recurso.", 'warning');
        return;
    }

    let datos = new FormData($("#formEditarRecurso")[0]);

    $("#btnGuardarEdicion").prop('disabled', true);

    $.ajax({
        url: '/Recurso/Actualizar',
        type: 'POST',
        data: datos,
        processData: false,
        contentType: false,

        success: function (response) {
            if (response.esCorrecto) {
                bootstrap.Modal
                    .getOrCreateInstance(document.getElementById('modalEditarRecurso'))
                    .hide();

                mostrarMensaje(response.mensaje, 'success');
                cargarMisRecursos();
            }
            else {
                mostrarMensaje(response.mensaje, response.codigo === 1211 ? 'warning' : 'danger');

                // Si el recurso ya no existe, se cierra el modal y se refresca la lista
                if (response.codigo === 1216) {
                    bootstrap.Modal
                        .getOrCreateInstance(document.getElementById('modalEditarRecurso'))
                        .hide();

                    cargarMisRecursos();
                }
            }
        },

        error: function (xhr) {
            if (xhr.status === 413) {
                mostrarMensaje(mensajeTamanoMaximo(), 'danger');
            }
            else {
                mostrarMensaje("No se pudo actualizar el recurso. Intente nuevamente.", 'danger');
            }
        },

        complete: function () {
            $("#btnGuardarEdicion").prop('disabled', false);
        }
    });
}

// ---------- Eliminar (HU 01-05) ----------

async function eliminarRecurso(idRecurso) {
    let recurso = recursosListados.find(r => r.idRecurso === idRecurso);

    if (!recurso) {
        return;
    }

    // Si cancela, no se llama al servidor y el recurso queda intacto
    if (!(await confirmar(`¿Desea eliminar el recurso "${recurso.titulo}"? Esta acción no se puede deshacer.`, "Eliminar"))) {
        return;
    }

    $.ajax({
        url: '/Recurso/Eliminar',
        type: 'POST',
        data: { idRecurso: idRecurso },

        success: function (response) {
            if (response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }

            // En ambos casos se refresca: si ya no existía, desaparece de la lista
            cargarMisRecursos();
        },

        error: function () {
            mostrarMensaje("No se pudo eliminar el recurso. Intente nuevamente.", 'danger');
        }
    });
}