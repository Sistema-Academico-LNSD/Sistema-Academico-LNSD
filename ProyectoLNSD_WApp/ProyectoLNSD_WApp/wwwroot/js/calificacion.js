// Registro de calificaciones del docente (MCNF-01-01, 01-02, 01-03 y 01-12).
// El docente crea las actividades (prácticas, exámenes, tareas) de cada rubro, digita las notas y las guarda con un botón.
// Cada actividad tiene sus puntos (lo que vale) y su porcentaje (lo que pesa en la nota final del semestre).
// Usa los helpers globales de site.js: mostrarMensaje y escapeHtml.

let calClases = [];
let calClaseActual = null;
let calPlanilla = null;
let calRubroActivo = null;   // rubro con la pestaña abierta (se conserva al recargar); 0 = resumen del semestre
let calBorradores = {};      // "matricula_actividad" -> nota digitada que aún no se guarda
let calFallidas = {};        // "matricula_actividad" -> true si el servidor rechazó esa nota
let calPeriodoPrevio = null;
let calClasePrevia = "0";

$(document).ready(function () {
    cargarPeriodosCalificacion();

    $("#calPeriodo").on("change", function () {
        if (!descartarBorradores()) {
            $(this).val(calPeriodoPrevio);
            return;
        }

        calPeriodoPrevio = $(this).val();
        cargarClasesCalificacion();
    });

    $("#calClase").on("change", function () {
        if (!descartarBorradores()) {
            $(this).val(calClasePrevia);
            return;
        }

        calClasePrevia = $(this).val();
        cargarPlanillaCalificacion();
    });

    // Avisa si intenta salir con notas digitadas sin guardar
    window.addEventListener("beforeunload", function (e) {
        if (totalBorradores() > 0) {
            e.preventDefault();
            e.returnValue = "";
        }
    });

    const $planilla = $("#calPlanilla");

    $planilla.on("click", ".nav-link", function () {
        calRubroActivo = parseInt($(this).data("rubro"), 10);
    });

    $planilla.on("click", ".btn-nueva-actividad", abrirModalActividad);
    $planilla.on("click", "#btnGuardarNotas", guardarNotasCalificacion);
    $planilla.on("input", ".nota-input", alEscribirNota);

    // Enter baja a la nota del siguiente estudiante en la misma actividad (no guarda)
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
            calPeriodoPrevio = $("#calPeriodo").val();

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
            calClasePrevia = "0";

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

    if (calRubroActivo === null || (calRubroActivo !== 0 && !rubros.some(r => r.idRubro === calRubroActivo))) {
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

    // Resumen del semestre: promedio por rubro y promedio general de cada estudiante
    let resumenActivo = calRubroActivo === 0;

    nav += `<li class="nav-item" role="presentation">
                <button type="button" class="nav-link ${resumenActivo ? 'active' : ''}" data-bs-toggle="tab"
                        data-bs-target="#calRubro0" data-rubro="0">
                    <i class="bi bi-calculator"></i> Resumen del semestre
                </button>
            </li>`;

    paneles += `<div class="tab-pane fade ${resumenActivo ? 'show active' : ''}" id="calRubro0">
                    ${pintarResumenCalificacion()}
                </div>`;

    let barra = `<div class="d-flex justify-content-between align-items-center mb-2 flex-wrap gap-2">
                    <span id="calPendientes" class="small text-muted"></span>
                    <button type="button" id="btnGuardarNotas" class="btn btn-success btn-sm" disabled>
                        <i class="bi bi-save"></i> Guardar notas
                    </button>
                 </div>`;

    $("#calPlanilla").html(barra + nav + '</ul>' + paneles + '</div>');

    actualizarPendientes();
}

// Promedio del semestre: solo cuenta lo ya calificado (la asistencia se suma en la siguiente parte)
function pintarResumenCalificacion() {
    let rubros = calPlanilla.rubros;

    let html = '<div class="table-responsive"><table class="table table-sm align-middle mb-0 cal-tabla"><thead><tr>';
    html += '<th class="cal-fijo">Estudiante</th>';

    rubros.forEach(function (r) {
        html += `<th class="text-center cal-nota">
                    <div class="fw-semibold">${escapeHtml(r.nombre)}</div>
                    <div class="small text-muted fw-normal">${fmtNota(r.porcentaje)}% de la nota</div>
                 </th>`;
    });

    html += `<th class="text-center cal-nota"><div class="fw-semibold">Promedio del semestre</div>
                 <div class="small text-muted fw-normal">sobre lo calificado</div></th>`;
    html += '</tr></thead><tbody>';

    calPlanilla.estudiantes.forEach(function (e) {
        html += `<tr>
                    <td class="cal-fijo">
                        <div>${escapeHtml(e.nombre)}</div>
                        <div class="small text-muted">${escapeHtml(e.carnet)}</div>
                    </td>`;

        rubros.forEach(function (r) {
            let res = e.resumen.find(x => x.idRubro === r.idRubro);

            if (!res || res.evaluado <= 0) {
                html += '<td class="text-center text-muted">—</td>';
                return;
            }

            html += `<td class="text-center">
                        <div class="fw-semibold">${fmtNota(res.acumulado / res.evaluado * 100)}%</div>
                        <div class="small text-muted">${fmtNota(res.acumulado)} de ${fmtNota(res.evaluado)}</div>
                     </td>`;
        });

        html += e.promedio == null
            ? '<td class="text-center text-muted">—</td>'
            : `<td class="text-center fw-bold fs-6">${fmtNota(e.promedio)}%</td>`;

        html += '</tr>';
    });

    html += '</tbody></table></div>';
    html += `<div class="form-text mt-2">Los valores "X de Y" son puntos de la nota final: lo ganado de lo ya calificado.
             El promedio del semestre es lo ganado entre lo calificado, por 100.</div>`;

    return html;
}

function pintarRubroCalificacion(rubro) {
    let resumen = rubro.actividades.length === 0
        ? 'Todavía no hay actividades en este rubro.'
        : `${rubro.actividades.length} ${rubro.actividades.length === 1 ? 'actividad' : 'actividades'}`;

    resumen += rubro.cantidadFija
        ? ` · Siempre ${rubro.cantidadFija}, cada una pesa ${fmtNota(rubro.porcentaje / rubro.cantidadFija)}%`
        : ` · El ${fmtNota(rubro.porcentaje)}% se reparte en partes iguales entre las actividades`;

    let completo = rubro.cantidadFija && rubro.actividades.length >= rubro.cantidadFija;

    let boton = calPlanilla.periodoActivo && !completo
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
                    <div class="small text-muted fw-normal">${fmtNota(a.puntos)} pts · ${fmtNota(a.porcentaje)}%${a.fecha ? ' · ' + fmtFecha(a.fecha) : ''}</div>
                 </th>`;
    });

    html += `<th class="text-center cal-nota"><div class="fw-semibold">Promedio del rubro</div>
                 <div class="small text-muted fw-normal">sobre lo calificado</div></th>`;
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
                html += `<td class="text-center">
                            <div class="fw-semibold">${fmtNota(nota.puntos)} / ${fmtNota(a.puntos)}</div>
                            <div class="small text-muted">${fmtNota(nota.puntos / a.puntos * 100)}%</div>
                         </td>`;
            } else if (calPlanilla.periodoActivo) {
                let clave = e.idMatricula + '_' + a.idActividad;
                let borrador = calBorradores[clave] ?? '';
                let invalida = calFallidas[clave] ? ' is-invalid' : '';
                let pendiente = borrador !== '' ? ' nota-pendiente' : '';

                html += `<td class="cal-nota">
                            <input type="number" min="0" max="${a.puntos}" step="0.01"
                                   class="form-control form-control-sm text-center nota-input${pendiente}${invalida}"
                                   data-matricula="${e.idMatricula}" data-actividad="${a.idActividad}" data-max="${a.puntos}"
                                   value="${escapeHtml(borrador)}" placeholder="/ ${fmtNota(a.puntos)}">
                            <div class="small text-muted text-center cal-pct">${textoPorcentaje(borrador, a.puntos)}</div>
                         </td>`;
            } else {
                html += '<td class="text-center text-muted">—</td>';
            }
        });

        let res = e.resumen.find(x => x.idRubro === rubro.idRubro);

        html += (!res || res.evaluado <= 0)
            ? '<td class="text-center text-muted">—</td>'
            : `<td class="text-center">
                    <div class="fw-semibold">${fmtNota(res.acumulado / res.evaluado * 100)}%</div>
                    <div class="small text-muted">${fmtNota(res.acumulado)} de ${fmtNota(res.evaluado)}</div>
               </td>`;

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

    $("#actPeso").text(rubro.cantidadFija
        ? `${fmtNota(rubro.porcentaje / rubro.cantidadFija)}%`
        : `${fmtNota(rubro.porcentaje)}% entre las actividades`);

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

// ------------------------- Registrar notas (se digitan y se guardan con el botón) -------------------------

function claveNota($input) {
    return $input.data("matricula") + '_' + $input.data("actividad");
}

function totalBorradores() {
    return Object.keys(calBorradores).length;
}

// Si hay notas sin guardar, pide confirmación antes de cambiar de período o de curso
function descartarBorradores() {
    if (totalBorradores() === 0) {
        return true;
    }

    if (!confirm("Tiene notas digitadas sin guardar. ¿Desea descartarlas?")) {
        return false;
    }

    calBorradores = {};
    calFallidas = {};

    return true;
}

function textoPorcentaje(texto, max) {
    let v = parseFloat(texto);

    if (texto === '' || isNaN(v) || v < 0 || v > max) {
        return '&nbsp;';
    }

    return fmtNota(v / max * 100) + '%';
}

function alEscribirNota() {
    let $input = $(this);
    let clave = claveNota($input);
    let texto = $input.val().trim();
    let max = parseFloat($input.data("max"));

    $input.removeClass("is-invalid");
    delete calFallidas[clave];

    if (texto === '') {
        delete calBorradores[clave];
        $input.removeClass("nota-pendiente");
    } else {
        calBorradores[clave] = texto;
        $input.addClass("nota-pendiente");
    }

    // Porcentaje en vivo: puntos obtenidos / puntos de la actividad x 100
    $input.siblings(".cal-pct").html(textoPorcentaje(texto, max));

    actualizarPendientes();
}

function actualizarPendientes() {
    let n = totalBorradores();

    $("#calPendientes").text(n === 0
        ? 'Digite las notas y luego presione "Guardar notas".'
        : `${n} ${n === 1 ? 'nota' : 'notas'} sin guardar`);

    $("#btnGuardarNotas").prop('disabled', n === 0);
}

async function guardarNotasCalificacion() {
    let items = [];
    let hayInvalidas = false;

    $("#calPlanilla .nota-input").each(function () {
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
            hayInvalidas = true;
            return;
        }

        items.push({
            idMatricula: parseInt($input.data("matricula"), 10),
            idActividad: parseInt($input.data("actividad"), 10),
            puntosObtenidos: puntos
        });
    });

    if (hayInvalidas) {
        mostrarMensaje("Hay notas fuera de rango (marcadas en rojo). Corríjalas antes de guardar.", 'danger');
        return;
    }

    if (items.length === 0) {
        return;
    }

    let $boton = $("#btnGuardarNotas").prop('disabled', true);

    try {
        let response = await $.ajax({
            url: '/Calificacion/GuardarNotas',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(items)
        });

        if (!response.esCorrecto) {
            mostrarMensaje(response.mensaje, 'danger');
            $boton.prop('disabled', false);
            return;
        }

        let fallos = [];

        response.dato.forEach(function (r) {
            let clave = r.idMatricula + '_' + r.idActividad;

            if (r.exito) {
                delete calBorradores[clave];
                delete calFallidas[clave];
            } else {
                calFallidas[clave] = true;
                fallos.push(r);
            }
        });

        if (fallos.length === 0) {
            mostrarMensaje(response.mensaje, 'success');
        } else {
            mostrarMensaje(`${response.mensaje}. ${fallos[0].mensaje}`, 'warning');
        }

        // Se recarga para traer los porcentajes y promedios calculados por el servidor
        cargarPlanillaCalificacion();
    } catch (e) {
        mostrarMensaje("No fue posible guardar las notas.", 'danger');
        $boton.prop('disabled', false);
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
