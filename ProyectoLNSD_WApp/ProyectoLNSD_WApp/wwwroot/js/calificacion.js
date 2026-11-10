// Registro de calificaciones del docente (MCNF-01-01, 01-02, 01-03 y 01-12).
// El docente crea las actividades (prácticas, exámenes, tareas) de cada rubro y registra la nota de cada estudiante.
// Usa los helpers globales de site.js: mostrarMensaje y escapeHtml.

let calClases = [];
let calClaseActual = null;
let calPlanilla = null;
let calRubroActivo = null;   // rubro con la pestaña abierta (se conserva al recargar)

$(document).ready(function () {
    cargarPeriodosCalificacion();

    $("#calPeriodo").on("change", cargarClasesCalificacion);
    $("#calClase").on("change", cargarPlanillaCalificacion);

    const $planilla = $("#calPlanilla");

    $planilla.on("click", ".nav-link", function () {
        calRubroActivo = parseInt($(this).data("rubro"), 10);
    });

    $planilla.on("click", ".btn-nueva-actividad", abrirModalActividad);
    $planilla.on("change", ".nota-input", guardarCeldaCalificacion);
    $planilla.on("input", ".nota-input", function () { $(this).removeClass("is-invalid"); });

    // Enter baja a la nota del siguiente estudiante en la misma actividad
    $planilla.on("keydown", ".nota-input", function (e) {
        if (e.key !== "Enter") {
            return;
        }

        e.preventDefault();

        let actividad = $(this).data("actividad");
        let $siguiente = $(this).closest("tr").nextAll("tr").find(`.nota-input[data-actividad="${actividad}"]`).first();

        if ($siguiente.length) {
            $siguiente.focus();
        } else {
            $(this).blur();
        }
    });

    $("#btnGuardarActividad").on("click", guardarActividadCalificacion);

    $("#modalActividad").on("keydown", "input", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            guardarActividadCalificacion();
        }
    });

    $("#modalActividad").on("shown.bs.modal", function () {
        $("#actNombre").focus();
    });
});

// ------------------------- Carga de períodos, clases y planilla -------------------------

function cargarPeriodosCalificacion() {
    $.ajax({
        url: '/Calificacion/GetPeriodos',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let html = '';

            (response.dato || []).forEach(function (p) {
                html += `<option value="${p.id}" ${p.activo ? 'selected' : ''}>${escapeHtml(p.nombre)}${p.activo ? ' (activo)' : ''}</option>`;
            });

            $("#calPeriodo").html(html);

            cargarClasesCalificacion();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los períodos lectivos.", 'danger');
        }
    });
}

function cargarClasesCalificacion() {
    calClaseActual = null;
    calRubroActivo = null;

    $.ajax({
        url: '/Calificacion/GetMisClases',
        type: 'GET',
        data: { idPeriodo: $("#calPeriodo").val() },

        success: function (response) {
            if (!response.esCorrecto) {
                calClases = [];
                $("#calClase").html('');
                limpiarPlanilla(response.mensaje);
                return;
            }

            calClases = response.dato || [];

            let html = '';

            calClases.forEach(function (c, i) {
                html += `<option value="${i}">${escapeHtml(c.descripcion)}</option>`;
            });

            $("#calClase").html(html);

            if (calClases.length === 0) {
                limpiarPlanilla(response.mensaje);
                return;
            }

            cargarPlanillaCalificacion();
        },

        error: function () {
            mostrarMensaje("No fue posible consultar sus cursos.", 'danger');
        }
    });
}

function limpiarPlanilla(mensaje) {
    mostrarAviso(mensaje);
    $("#calPlanilla").html('');
    $("#calContador").text('');
}

function cargarPlanillaCalificacion() {
    let seleccionada = calClases[parseInt($("#calClase").val(), 10)];

    // Si cambia de curso se empieza otra vez en la primera pestaña
    if (seleccionada !== calClaseActual) {
        calRubroActivo = null;
    }

    calClaseActual = seleccionada;

    if (!calClaseActual) {
        return;
    }

    $.ajax({
        url: '/Calificacion/GetPlanilla',
        type: 'GET',
        data: {
            idPeriodo: calClaseActual.idPeriodo,
            idCurso: calClaseActual.idCurso,
            idSeccion: calClaseActual.idSeccion,
            idGrupo: calClaseActual.idGrupo
        },

        success: function (response) {
            if (!response.esCorrecto) {
                limpiarPlanilla(response.mensaje);
                return;
            }

            calPlanilla = response.dato;

            let avisos = [];

            if (!calPlanilla.periodoActivo) {
                avisos.push("El período está cerrado: la planilla es solo de consulta.");
            }

            if (calPlanilla.estudiantes.length === 0) {
                avisos.push(response.mensaje);
            }

            mostrarAviso(avisos.join(' '));

            let n = calPlanilla.estudiantes.length;
            $("#calContador").text(n + (n === 1 ? ' estudiante' : ' estudiantes'));

            pintarPlanillaCalificacion();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar la planilla de calificaciones.", 'danger');
        }
    });
}

// ------------------------- Pintado: una pestaña por rubro -------------------------

function pintarPlanillaCalificacion() {
    let rubros = calPlanilla.rubros;

    if (calPlanilla.estudiantes.length === 0 || rubros.length === 0) {
        $("#calPlanilla").html('');
        return;
    }

    if (!rubros.some(r => r.idRubro === calRubroActivo)) {
        calRubroActivo = rubros[0].idRubro;
    }

    let nav = '<ul class="nav nav-tabs mb-3" role="tablist">';
    let paneles = '<div class="tab-content">';

    rubros.forEach(function (r) {
        let activa = r.idRubro === calRubroActivo;

        nav += `<li class="nav-item" role="presentation">
                    <button type="button" class="nav-link ${activa ? 'active' : ''}" data-bs-toggle="tab"
                            data-bs-target="#calRubro${r.idRubro}" data-rubro="${r.idRubro}">
                        ${escapeHtml(r.nombre)}
                        <span class="badge text-bg-light border">${fmtNota(r.porcentaje)}%</span>
                        <span class="text-muted small">(${r.actividades.length})</span>
                    </button>
                </li>`;

        paneles += `<div class="tab-pane fade ${activa ? 'show active' : ''}" id="calRubro${r.idRubro}">
                        ${pintarRubroCalificacion(r)}
                    </div>`;
    });

    $("#calPlanilla").html(nav + '</ul>' + paneles + '</div>');
}

function pintarRubroCalificacion(rubro) {
    let totalPuntos = rubro.actividades.reduce((suma, a) => suma + a.puntos, 0);

    let resumen = rubro.actividades.length === 0
        ? 'Todavía no hay actividades en este rubro.'
        : `${rubro.actividades.length} ${rubro.actividades.length === 1 ? 'actividad' : 'actividades'} · ${fmtNota(totalPuntos)} puntos en total`;

    let boton = calPlanilla.periodoActivo
        ? `<button type="button" class="btn btn-sm btn-primary btn-nueva-actividad" data-rubro="${rubro.idRubro}">
               <i class="bi bi-plus-lg"></i> Agregar actividad
           </button>`
        : '';

    let html = `<div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="text-muted small">${resumen}</span>
                    ${boton}
                </div>`;

    if (rubro.actividades.length === 0) {
        return html;
    }

    html += '<div class="table-responsive"><table class="table table-sm align-middle mb-0 cal-tabla"><thead><tr>';
    html += '<th class="cal-fijo">Estudiante</th>';

    rubro.actividades.forEach(function (a) {
        html += `<th class="text-center cal-nota">
                    <div class="fw-semibold">${escapeHtml(a.nombre)}</div>
                    <div class="small text-muted fw-normal">${fmtNota(a.puntos)} pts${a.fecha ? ' · ' + fmtFecha(a.fecha) : ''}</div>
                 </th>`;
    });

    html += '</tr></thead><tbody>';

    calPlanilla.estudiantes.forEach(function (e) {
        let notas = {};
        e.notas.forEach(n => notas[n.idActividad] = n);

        html += `<tr>
                    <td class="cal-fijo">
                        <div>${escapeHtml(e.nombre)}</div>
                        <div class="small text-muted">${escapeHtml(e.carnet)}</div>
                    </td>`;

        rubro.actividades.forEach(function (a) {
            let nota = notas[a.idActividad];

            if (nota) {
                html += `<td class="text-center fw-semibold">${fmtNota(nota.puntos)}</td>`;
            } else if (calPlanilla.periodoActivo) {
                html += `<td class="cal-nota">
                            <input type="number" min="0" max="${a.puntos}" step="0.01"
                                   class="form-control form-control-sm text-center nota-input"
                                   data-matricula="${e.idMatricula}" data-actividad="${a.idActividad}" data-max="${a.puntos}"
                                   placeholder="/ ${fmtNota(a.puntos)}">
                         </td>`;
            } else {
                html += '<td class="text-center text-muted">—</td>';
            }
        });

        html += '</tr>';
    });

    html += '</tbody></table></div>';

    return html;
}

// ------------------------- Nueva actividad -------------------------

function abrirModalActividad() {
    let idRubro = parseInt($(this).data("rubro"), 10);
    let rubro = calPlanilla.rubros.find(r => r.idRubro === idRubro);

    $("#actIdRubro").val(idRubro);
    $("#actRubroNombre").text(rubro.nombre);
    $("#actNombre").val('');
    $("#actPuntos").val('');
    $("#actFecha").val('');
    $("#actError").addClass('d-none').text('');

    let min = rubro.puntosMinimo;
    let max = rubro.puntosMaximo;
    let ayuda = 'Hasta 100 puntos';

    if (min != null && max != null) {
        ayuda = `Entre ${fmtNota(min)} y ${fmtNota(max)} puntos`;
    } else if (min != null) {
        ayuda = `Mínimo ${fmtNota(min)} puntos`;
    } else if (max != null) {
        ayuda = `Máximo ${fmtNota(max)} puntos`;
    }

    $("#actAyudaPuntos").text(ayuda);

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalActividad')).show();
}

function errorActividad(texto) {
    $("#actError").removeClass('d-none').text(texto);
}

async function guardarActividadCalificacion() {
    let nombre = $("#actNombre").val().trim();
    let puntos = parseFloat($("#actPuntos").val());
    let fecha = $("#actFecha").val();

    $("#actError").addClass('d-none').text('');

    if (!nombre) {
        errorActividad("Escriba el nombre de la actividad.");
        return;
    }

    if (isNaN(puntos) || puntos <= 0 || puntos > 100) {
        errorActividad("El valor debe estar entre 0.01 y 100 puntos.");
        return;
    }

    let $boton = $("#btnGuardarActividad").prop('disabled', true);

    try {
        let response = await $.ajax({
            url: '/Calificacion/CrearActividad',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                idPeriodo: calClaseActual.idPeriodo,
                idCurso: calClaseActual.idCurso,
                idSeccion: calClaseActual.idSeccion,
                idGrupo: calClaseActual.idGrupo,
                idRubro: parseInt($("#actIdRubro").val(), 10),
                nombre: nombre,
                puntos: puntos,
                fecha: fecha || null
            })
        });

        if (!response.esCorrecto) {
            errorActividad(response.mensaje);
            return;
        }

        bootstrap.Modal.getInstance(document.getElementById('modalActividad')).hide();
        mostrarMensaje(response.mensaje, 'success');
        cargarPlanillaCalificacion();
    } catch (e) {
        errorActividad("No fue posible agregar la actividad.");
    } finally {
        $boton.prop('disabled', false);
    }
}

// ------------------------- Registrar una nota -------------------------

async function guardarCeldaCalificacion() {
    let $input = $(this);
    let texto = $input.val().trim();

    if (texto === '') {
        return;
    }

    let puntos = parseFloat(texto);
    let max = parseFloat($input.data("max"));

    // Validación en el navegador (la regla oficial está en el servidor, MCNF-01-12)
    if (isNaN(puntos) || puntos < 0 || puntos > max) {
        $input.addClass('is-invalid');
        mostrarMensaje(`La nota debe estar entre 0 y ${fmtNota(max)}.`, 'danger');
        return;
    }

    $input.prop('disabled', true);

    try {
        let response = await $.ajax({
            url: '/Calificacion/GuardarNota',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                idMatricula: parseInt($input.data("matricula"), 10),
                idActividad: parseInt($input.data("actividad"), 10),
                puntosObtenidos: puntos
            })
        });

        if (!response.esCorrecto) {
            $input.prop('disabled', false).addClass('is-invalid');
            mostrarMensaje(response.mensaje, 'danger');
            return;
        }

        // La nota queda fija en la celda (editar llega en la siguiente parte)
        $input.closest('td').addClass('text-center fw-semibold').text(fmtNota(response.dato.puntos));

        // Se mantiene la planilla en memoria al día
        let est = calPlanilla.estudiantes.find(e => e.idMatricula === parseInt($input.data("matricula"), 10));

        if (est) {
            est.notas.push(response.dato);
        }
    } catch (e) {
        $input.prop('disabled', false).addClass('is-invalid');
        mostrarMensaje("No fue posible guardar la nota.", 'danger');
    }
}

// ------------------------- Auxiliares -------------------------

function mostrarAviso(texto) {
    let $aviso = $("#calAviso");

    if (!texto) {
        $aviso.addClass('d-none').text('');
        return;
    }

    $aviso.removeClass('d-none').text(texto);
}

// 20 -> "20", 17.5 -> "17.5" (sin ceros de más)
function fmtNota(valor) {
    return String(+Number(valor).toFixed(2));
}

function fmtFecha(texto) {
    let f = new Date(texto);

    return isNaN(f) ? '' : f.toLocaleDateString('es-CR', { day: '2-digit', month: '2-digit' });
}
