// Consulta institucional de calificaciones del administrador y director (MCNF-01-11 y 01-15).
// Usa renderDetalleCalificaciones y badgeEstado (calificacion-detalle.js) y los helpers de site.js.

let cnsClases = [];
let cnsTemporizador = null;
let cnsFilas = [];

$(document).ready(function () {
    cargarPeriodosConsulta();

    $("#cnsPeriodo").on("change", cargarClasesConsulta);
    $("#cnsCurso, #cnsSeccion, #cnsGrupo").on("change", consultarNotas);

    // La búsqueda por nombre espera un momento a que termine de escribir
    $("#cnsBuscar").on("input", function () {
        clearTimeout(cnsTemporizador);
        cnsTemporizador = setTimeout(consultarNotas, 400);
    });

    $("#cnsCuerpo").on("click", ".btn-ver-detalle", verDetalleConsulta);
});

function avisoConsulta(texto) {
    let $aviso = $("#cnsAviso");

    if (!texto) {
        $aviso.addClass('d-none').text('');
        return;
    }

    $aviso.removeClass('d-none').text(texto);
}

function cargarPeriodosConsulta() {
    $.ajax({
        url: '/Calificacion/GetPeriodosConsulta',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                avisoConsulta(response.mensaje);
                return;
            }

            let periodos = response.dato || [];
            let html = '';

            periodos.forEach(function (p) {
                html += `<option value="${p.id}">${escapeHtml(p.nombre)}${p.activo ? ' (activo)' : ' (cerrado)'}</option>`;
            });

            $("#cnsPeriodo").html(html);

            let activo = periodos.find(p => p.activo);

            if (activo) {
                $("#cnsPeriodo").val(activo.id);
            }

            cargarClasesConsulta();
        },

        error: function () {
            avisoConsulta("No fue posible cargar los períodos lectivos.");
        }
    });
}

// Las opciones de curso, grado y sección y grupo salen de las clases del período que ya tienen notas
function cargarClasesConsulta() {
    $.ajax({
        url: '/Calificacion/GetClasesConsulta',
        type: 'GET',
        data: { idPeriodo: $("#cnsPeriodo").val() },

        success: function (response) {
            cnsClases = response.esCorrecto ? (response.dato || []) : [];

            llenarFiltro("#cnsCurso", cnsClases, c => c.idCurso, c => c.curso, "Todos los cursos");
            llenarFiltro("#cnsSeccion", cnsClases, c => c.idSeccion, c => `${c.grado} ${c.seccion}`, "Todas");
            llenarFiltro("#cnsGrupo", cnsClases, c => c.idGrupo, c => c.grupo, "Todos");

            consultarNotas();
        },

        error: function () {
            avisoConsulta("No fue posible cargar los cursos del período.");
        }
    });
}

function llenarFiltro(selector, clases, obtenerId, obtenerTexto, textoTodos) {
    let vistos = {};
    let html = `<option value="">${textoTodos}</option>`;

    clases.forEach(function (c) {
        let id = obtenerId(c);

        if (!vistos[id]) {
            vistos[id] = true;
            html += `<option value="${id}">${escapeHtml(obtenerTexto(c))}</option>`;
        }
    });

    $(selector).html(html);
}

function consultarNotas() {
    avisoConsulta('');
    $("#cnsCuerpo").html('<tr><td colspan="7" class="text-center text-muted">Cargando...</td></tr>');

    $.ajax({
        url: '/Calificacion/GetConsultaNotas',
        type: 'GET',
        data: {
            idPeriodo: $("#cnsPeriodo").val(),
            idCurso: $("#cnsCurso").val() || null,
            idSeccion: $("#cnsSeccion").val() || null,
            idGrupo: $("#cnsGrupo").val() || null,
            texto: $("#cnsBuscar").val().trim()
        },

        success: function (response) {
            if (!response.esCorrecto) {
                cnsFilas = [];
                $("#cnsCuerpo").html('<tr><td colspan="7" class="text-center text-muted">Sin resultados</td></tr>');
                avisoConsulta(response.mensaje);
                return;
            }

            cnsFilas = response.dato || [];
            avisoConsulta(response.mensaje && response.mensaje !== 'Operación realizada correctamente' ? response.mensaje : '');
            pintarConsulta();
        },

        error: function () {
            $("#cnsCuerpo").html('<tr><td colspan="7" class="text-center text-muted">Sin resultados</td></tr>');
            avisoConsulta("No fue posible consultar las calificaciones.");
        }
    });
}

function pintarConsulta() {
    if (cnsFilas.length === 0) {
        $("#cnsCuerpo").html('<tr><td colspan="7" class="text-center text-muted">Sin resultados</td></tr>');
        return;
    }

    let html = '';

    cnsFilas.forEach(function (f, i) {
        html += `<tr>
                    <td>${escapeHtml(f.estudiante)}<div class="small text-muted">${escapeHtml(f.carnet)}</div></td>
                    <td>${escapeHtml(f.curso)}</td>
                    <td>${escapeHtml(f.grado)} ${escapeHtml(f.seccion)} · Grupo ${escapeHtml(f.grupo)}</td>
                    <td class="text-center">${f.promedio == null ? '—' : fmtDet(f.promedio) + '%'}</td>
                    <td class="text-center">${f.notaAnual == null ? '—' : fmtDet(f.notaAnual) + '%'}</td>
                    <td class="text-center">${badgeEstado(f.estado)}</td>
                    <td class="text-end">
                        <button type="button" class="btn btn-sm btn-outline-primary btn-ver-detalle" data-fila="${i}">
                            <i class="bi bi-card-list"></i> Detalle
                        </button>
                    </td>
                 </tr>`;
    });

    $("#cnsCuerpo").html(html);
}

function verDetalleConsulta() {
    let fila = cnsFilas[parseInt($(this).data("fila"), 10)];

    $("#cnsDetalle").html('<div class="text-center text-muted py-4">Cargando...</div>');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalDetalleConsulta')).show();

    $.ajax({
        url: '/Calificacion/GetDetalleConsulta',
        type: 'GET',
        data: { idEstudiante: fila.idEstudiante, idPeriodo: $("#cnsPeriodo").val() },

        success: function (response) {
            $("#cnsDetalle").html(response.esCorrecto
                ? renderDetalleCalificaciones(response.dato)
                : `<div class="text-danger">${escapeHtml(response.mensaje)}</div>`);
        },

        error: function () {
            $("#cnsDetalle").html('<div class="text-danger">No fue posible consultar el detalle.</div>');
        }
    });
}
