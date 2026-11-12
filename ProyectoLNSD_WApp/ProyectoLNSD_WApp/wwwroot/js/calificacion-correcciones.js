// Autorización de correcciones de notas con el período cerrado (MCNF-01-13).
// Usa los helpers globales de site.js: mostrarMensaje y escapeHtml.

let corClases = [];
let corNotas = [];
let corNotaActual = null;

$(document).ready(function () {
    cargarPeriodosCorreccion();

    $("#corPeriodo").on("change", cargarClasesCorreccion);
    $("#corClase").on("change", cargarNotasCorreccion);
    $("#corBuscar").on("input", pintarNotasCorreccion);

    $("#corCuerpo").on("click", ".btn-autorizar", abrirModalCorreccion);
    $("#btnAutorizar").on("click", autorizarCorreccion);

    $("#modalCorreccion").on("shown.bs.modal", function () {
        $("#corMotivo").focus();
    });
});

function mostrarAvisoCorreccion(texto) {
    let $aviso = $("#corAviso");

    if (!texto) {
        $aviso.addClass('d-none').text('');
        return;
    }

    $aviso.removeClass('d-none').text(texto);
}

function limpiarTablaCorreccion(mensaje) {
    corNotas = [];
    mostrarAvisoCorreccion(mensaje);
    $("#corCuerpo").html('<tr><td colspan="5" class="text-center text-muted">Sin notas para mostrar</td></tr>');
}

function cargarPeriodosCorreccion() {
    $.ajax({
        url: '/Calificacion/GetPeriodosCorreccion',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let periodos = response.dato || [];
            let html = '';

            periodos.forEach(function (p) {
                html += `<option value="${p.id}">${escapeHtml(p.nombre)}${p.activo ? ' (activo)' : ' (cerrado)'}</option>`;
            });

            $("#corPeriodo").html(html);

            // Las correcciones son de períodos cerrados: se empieza por el primero cerrado
            let cerrado = periodos.find(p => !p.activo);

            if (cerrado) {
                $("#corPeriodo").val(cerrado.id);
            }

            cargarClasesCorreccion();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los períodos lectivos.", 'danger');
        }
    });
}

function cargarClasesCorreccion() {
    $.ajax({
        url: '/Calificacion/GetClasesCorreccion',
        type: 'GET',
        data: { idPeriodo: $("#corPeriodo").val() },

        success: function (response) {
            if (!response.esCorrecto) {
                corClases = [];
                $("#corClase").html('');
                limpiarTablaCorreccion(response.mensaje);
                return;
            }

            corClases = response.dato || [];

            let html = '';

            corClases.forEach(function (c, i) {
                html += `<option value="${i}">${escapeHtml(c.descripcion)}</option>`;
            });

            $("#corClase").html(html);

            if (corClases.length === 0) {
                limpiarTablaCorreccion(response.mensaje);
                return;
            }

            cargarNotasCorreccion();
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los cursos del período.", 'danger');
        }
    });
}

function cargarNotasCorreccion() {
    let clase = corClases[parseInt($("#corClase").val(), 10)];

    if (!clase) {
        return;
    }

    $.ajax({
        url: '/Calificacion/GetNotasCorreccion',
        type: 'GET',
        data: {
            idPeriodo: clase.idPeriodo,
            idCurso: clase.idCurso,
            idSeccion: clase.idSeccion,
            idGrupo: clase.idGrupo
        },

        success: function (response) {
            if (!response.esCorrecto) {
                limpiarTablaCorreccion(response.mensaje);
                return;
            }

            corNotas = response.dato || [];
            mostrarAvisoCorreccion(corNotas.length === 0 ? response.mensaje : '');
            pintarNotasCorreccion();
        },

        error: function () {
            mostrarMensaje("No fue posible consultar las notas del curso.", 'danger');
        }
    });
}

function pintarNotasCorreccion() {
    let filtro = $("#corBuscar").val().trim().toLowerCase();

    let filas = corNotas.filter(n => !filtro
        || (n.estudiante + ' ' + n.carnet + ' ' + n.actividad + ' ' + n.rubro).toLowerCase().includes(filtro));

    if (filas.length === 0) {
        $("#corCuerpo").html('<tr><td colspan="5" class="text-center text-muted">Sin notas para mostrar</td></tr>');
        return;
    }

    let colores = { 'Sin autorización': 'secondary', 'Autorizada': 'success', 'Usada': 'dark' };
    let html = '';

    filas.forEach(function (n) {
        let accion = n.autorizable
            ? `<button type="button" class="btn btn-sm btn-outline-primary btn-autorizar" data-nota="${n.idNota}">
                   <i class="bi bi-unlock"></i> Autorizar
               </button>`
            : '';

        html += `<tr>
                    <td>${escapeHtml(n.estudiante)}<div class="small text-muted">${escapeHtml(n.carnet)}</div></td>
                    <td>${escapeHtml(n.actividad)}<div class="small text-muted">${escapeHtml(n.rubro)}</div></td>
                    <td class="text-center">${fmtN(n.puntos)} / ${fmtN(n.puntosActividad)}
                        <div class="small text-muted">${fmtN(n.puntos / n.puntosActividad * 100)}%</div></td>
                    <td><span class="badge text-bg-${colores[n.estado] || 'secondary'}">${escapeHtml(n.estado)}</span>
                        ${n.motivo ? `<div class="small text-muted">${escapeHtml(n.motivo)}</div>` : ''}</td>
                    <td class="text-end">${accion}</td>
                 </tr>`;
    });

    $("#corCuerpo").html(html);
}

function abrirModalCorreccion() {
    let idNota = parseInt($(this).data("nota"), 10);
    corNotaActual = corNotas.find(n => n.idNota === idNota);

    $("#corEstudiante").text(corNotaActual.estudiante);
    $("#corDetalle").text(`${corNotaActual.actividad} · ${corNotaActual.rubro} · nota actual ${fmtN(corNotaActual.puntos)} / ${fmtN(corNotaActual.puntosActividad)}`);
    $("#corMotivo").val('');
    $("#corError").addClass('d-none').text('');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCorreccion')).show();
}

function errorCorreccion(texto) {
    $("#corError").removeClass('d-none').text(texto);
}

async function autorizarCorreccion() {
    let motivo = $("#corMotivo").val().trim();

    $("#corError").addClass('d-none').text('');

    if (!motivo) {
        errorCorreccion("Escriba el motivo de la corrección.");
        return;
    }

    let $boton = $("#btnAutorizar").prop('disabled', true);

    try {
        let response = await $.ajax({
            url: '/Calificacion/AutorizarCorreccion',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ idNota: corNotaActual.idNota, motivo: motivo })
        });

        if (!response.esCorrecto) {
            errorCorreccion(response.mensaje);
            return;
        }

        bootstrap.Modal.getInstance(document.getElementById('modalCorreccion')).hide();
        mostrarMensaje(response.mensaje, 'success');
        cargarNotasCorreccion();
    } catch (e) {
        errorCorreccion("No fue posible autorizar la corrección.");
    } finally {
        $boton.prop('disabled', false);
    }
}

// 20 -> "20", 17.5 -> "17.5" (sin ceros de más)
function fmtN(valor) {
    return String(+Number(valor).toFixed(2));
}
