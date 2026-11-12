// Registro de calificaciones del docente (MCNF-01-01, 01-02, 01-03 y 01-12).
// El docente crea las actividades (prácticas, exámenes, tareas) de cada rubro, digita las notas y las guarda con un botón.
// Cada actividad tiene sus puntos (lo que vale) y su porcentaje (lo que pesa en la nota del año: cada semestre aporta la mitad).
// En las pruebas el docente reparte el porcentaje anual; en los demás rubros el peso se calcula solo.
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
    $planilla.on("click", ".btn-editar-nota", abrirModalEditarNota);
    $planilla.on("click", ".btn-historial", abrirHistorial);
    $planilla.on("click", ".btn-detalle", abrirDetalle);

    $("#btnGuardarEdicion").on("click", guardarEdicionNota);
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

// Promedio del semestre (rubros y asistencia), nota del año y resultado frente a la nota mínima (MCNF-01-04, 01-05 y 01-14).
// Todo se calcula en el servidor y solo cuenta lo ya evaluado.
function pintarResumenCalificacion() {
    let rubros = calPlanilla.rubros;
    let colores = { 'Aprobado': 'success', 'Reprobado': 'danger', 'Aprobando': 'success', 'Reprobando': 'warning', 'Sin notas': 'secondary' };

    let html = '<div class="table-responsive"><table class="table table-sm align-middle mb-0 cal-tabla"><thead><tr>';
    html += '<th class="cal-fijo">Estudiante</th>';

    rubros.forEach(function (r) {
        html += `<th class="text-center cal-nota">
                    <div class="fw-semibold">${escapeHtml(r.nombre)}</div>
                    <div class="small text-muted fw-normal">${fmtNota(r.porcentaje)}% de la nota del año</div>
                 </th>`;
    });

    if (calPlanilla.consideraAsistencia) {
        html += `<th class="text-center cal-nota"><div class="fw-semibold">Asistencia</div>
                     <div class="small text-muted fw-normal">${fmtNota(calPlanilla.porcentajeAsistencia)}% de la nota del año</div></th>`;
    }

    html += `<th class="text-center cal-nota"><div class="fw-semibold">Promedio del semestre</div>
                 <div class="small text-muted fw-normal">sobre lo evaluado</div></th>`;
    html += `<th class="text-center cal-nota"><div class="fw-semibold">Nota del año</div>
                 <div class="small text-muted fw-normal">suma de los semestres</div></th>`;
    html += `<th class="text-center cal-nota"><div class="fw-semibold">Resultado</div>
                 <div class="small text-muted fw-normal">mínimo ${fmtNota(calPlanilla.notaMinima)}</div></th>`;
    html += '</tr></thead><tbody>';

    calPlanilla.estudiantes.forEach(function (e) {
        html += `<tr>${celdaEstudiante(e)}`;

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

        if (calPlanilla.consideraAsistencia) {
            let asi = e.asistencia;

            html += (!asi || asi.registros === 0)
                ? '<td class="text-center text-muted">—</td>'
                : `<td class="text-center">
                        <div class="fw-semibold">${fmtNota(asi.porcentaje)}%</div>
                        <div class="small text-muted">${asi.registros - asi.ausencias} de ${asi.registros} clases</div>
                   </td>`;
        }

        html += e.promedio == null
            ? '<td class="text-center text-muted">—</td>'
            : `<td class="text-center fw-bold fs-6">${fmtNota(e.promedio)}%</td>`;

        html += e.notaAnual == null
            ? '<td class="text-center text-muted">—</td>'
            : `<td class="text-center">
                    <div class="fw-bold fs-6">${fmtNota(e.notaAnual)}%</div>
                    ${e.resultadoDefinitivo ? '' : '<div class="small text-muted">parcial</div>'}
               </td>`;

        html += `<td class="text-center"><span class="badge text-bg-${colores[e.resultado] || 'secondary'}">${escapeHtml(e.resultado)}</span></td>`;

        html += '</tr>';
    });

    html += '</tbody></table></div>';
    html += `<div class="form-text mt-2">Los valores "X de Y" son puntos de la nota del año: lo ganado de lo ya evaluado.
             La asistencia es el porcentaje de clases sin ausencia, según los registros del período; solo cuenta cuando ya hay registros.
             "Aprobando" o "Reprobando" es el resultado parcial: pasa a "Aprobado" o "Reprobado" cuando el estudiante completa los dos semestres y ambos están cerrados.</div>`;

    return html;
}

function pintarRubroCalificacion(rubro) {
    let resumen = rubro.actividades.length === 0
        ? 'Todavía no hay actividades en este rubro.'
        : `${rubro.actividades.length} ${rubro.actividades.length === 1 ? 'actividad' : 'actividades'}`;

    if (rubro.pesoManual) {
        resumen += ` · ${rubro.actividades.length} de ${rubro.cantidadFija} en este semestre · Quedan ${fmtNota(rubro.pesoRestante)}% por repartir en el año`;
    } else {
        resumen += ` · Este semestre aporta ${fmtNota(rubro.porcentajePeriodo)}% de la nota del año, en partes iguales`;
    }

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

        html += `<tr>${celdaEstudiante(e)}`;

        rubro.actividades.forEach(function (a) {
            let nota = notas[a.idActividad];

            if (nota) {
                html += `<td class="text-center">
                            <div class="fw-semibold">${fmtNota(nota.puntos)} / ${fmtNota(a.puntos)}</div>
                            <div class="small text-muted">${fmtNota(nota.puntos / a.puntos * 100)}%
                                ${nota.observacion ? `<i class="bi bi-chat-left-text" title="${escapeHtml(nota.observacion)}"></i>` : ''}
                                <button type="button" class="btn btn-link btn-sm p-0 ms-1 btn-editar-nota" title="Editar nota u observación"
                                        data-matricula="${e.idMatricula}" data-actividad="${a.idActividad}">
                                    <i class="bi bi-pencil-square"></i>
                                </button>
                            </div>
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

    // Peso de la actividad en la nota del año: automático, salvo en las pruebas (las reparte el docente)
    let $peso = $("#actPorcentaje").val('').addClass('d-none');
    $("#actPeso").removeClass('d-none');

    if (!rubro.pesoManual) {
        $("#actPeso").text(`${fmtNota(rubro.porcentajePeriodo)}% entre las del semestre`);
        $("#actAyudaPeso").text('Se calcula automáticamente');
    } else if (rubro.actividadesFaltantesAnio <= 1) {
        $("#actPeso").text(`${fmtNota(rubro.pesoRestante)}%`);
        $("#actAyudaPeso").text('Última del año: toma lo que queda del rubro');
    } else {
        let maximo = Math.round((rubro.pesoRestante - 0.01 * (rubro.actividadesFaltantesAnio - 1)) * 100) / 100;

        $("#actPeso").addClass('d-none');
        $peso.removeClass('d-none').attr('max', maximo);
        $("#actAyudaPeso").text(`Máximo ${fmtNota(maximo)}% · quedan ${fmtNota(rubro.pesoRestante)}% por repartir en el año`);
    }

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

    // En las pruebas el docente escribe el porcentaje (menos en la última del año, que la calcula el servidor)
    let rubroModal = calPlanilla.rubros.find(r => r.idRubro === parseInt($("#actIdRubro").val(), 10));
    let porcentaje = null;

    if (rubroModal.pesoManual && rubroModal.actividadesFaltantesAnio > 1) {
        porcentaje = parseFloat($("#actPorcentaje").val());

        if (isNaN(porcentaje) || porcentaje <= 0) {
            errorActividad("Escriba el porcentaje que pesa esta actividad en la nota del año.");
            return;
        }
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
                porcentaje: porcentaje,
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

// ------------------------- Editar o corregir una nota (MCNF-01-06 y 01-07) -------------------------

let calNotaEditando = null;

function abrirModalEditarNota() {
    let idMatricula = parseInt($(this).data("matricula"), 10);
    let idActividad = parseInt($(this).data("actividad"), 10);

    let est = calPlanilla.estudiantes.find(e => e.idMatricula === idMatricula);
    let nota = est.notas.find(n => n.idActividad === idActividad);
    let act = calPlanilla.rubros.flatMap(r => r.actividades).find(a => a.idActividad === idActividad);

    calNotaEditando = { idNota: nota.idNota, puntos: nota.puntos, max: act.puntos };

    $("#edtEstudiante").text(est.nombre);
    $("#edtActividad").text(act.nombre);
    $("#edtActual").text(`${fmtNota(nota.puntos)} / ${fmtNota(act.puntos)} (${fmtNota(nota.puntos / act.puntos * 100)}%)`);
    $("#edtObsActual").text(nota.observacion || 'Sin observación');
    $("#edtPuntos").val(nota.puntos).attr('max', act.puntos);
    $("#edtAyudaPuntos").text(`Entre 0 y ${fmtNota(act.puntos)}`);
    $("#edtObservacion").val('');
    $("#edtError").addClass('d-none').text('');
    $("#edtAviso").toggleClass('d-none', calPlanilla.periodoActivo);

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalEditarNota')).show();
}

function errorEdicion(texto) {
    $("#edtError").removeClass('d-none').text(texto);
}

async function guardarEdicionNota() {
    let puntos = parseFloat($("#edtPuntos").val());
    let observacion = $("#edtObservacion").val().trim();

    $("#edtError").addClass('d-none').text('');

    if (isNaN(puntos) || puntos < 0 || puntos > calNotaEditando.max) {
        errorEdicion(`La nota debe estar entre 0 y ${fmtNota(calNotaEditando.max)}.`);
        return;
    }

    if (puntos === calNotaEditando.puntos && observacion === '') {
        errorEdicion("No hay cambios para guardar.");
        return;
    }

    if (puntos !== calNotaEditando.puntos && observacion === '') {
        errorEdicion("Escriba la observación que justifica el cambio de la nota.");
        return;
    }

    let $boton = $("#btnGuardarEdicion").prop('disabled', true);

    try {
        let response = await $.ajax({
            url: '/Calificacion/EditarNota',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                idNota: calNotaEditando.idNota,
                puntosObtenidos: puntos,
                observacion: observacion || null
            })
        });

        if (!response.esCorrecto) {
            errorEdicion(response.mensaje);
            return;
        }

        bootstrap.Modal.getInstance(document.getElementById('modalEditarNota')).hide();
        mostrarMensaje(response.mensaje, 'success');
        cargarPlanillaCalificacion();
    } catch (e) {
        errorEdicion("No fue posible guardar el cambio.");
    } finally {
        $boton.prop('disabled', false);
    }
}

// ------------------------- Historial del estudiante en el curso (MCNF-01-08) -------------------------

function celdaEstudiante(e) {
    return `<td class="cal-fijo">
                <div class="d-flex justify-content-between align-items-start gap-2">
                    <div>
                        <div>${escapeHtml(e.nombre)}</div>
                        <div class="small text-muted">${escapeHtml(e.carnet)}</div>
                    </div>
                    <span class="text-nowrap">
                        <button type="button" class="btn btn-link btn-sm p-0 btn-detalle" title="Ver detalle completo"
                                data-estudiante="${e.idEstudiante}">
                            <i class="bi bi-card-list"></i>
                        </button>
                        <button type="button" class="btn btn-link btn-sm p-0 ms-1 btn-historial" title="Ver historial de notas"
                                data-matricula="${e.idMatricula}">
                            <i class="bi bi-clock-history"></i>
                        </button>
                    </span>
                </div>
            </td>`;
}

// Detalle completo del estudiante en este curso: rubros, actividades, asistencia, promedios y estado (MCNF-01-15)
async function abrirDetalle() {
    let idEstudiante = parseInt($(this).data("estudiante"), 10);

    $("#detCuerpo").html('<div class="text-center text-muted py-4">Cargando...</div>');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalDetalle')).show();

    try {
        let response = await $.ajax({
            url: '/Calificacion/GetDetalleDocente',
            type: 'GET',
            data: {
                idEstudiante: idEstudiante,
                idPeriodo: calClaseActual.idPeriodo,
                idCurso: calClaseActual.idCurso,
                idSeccion: calClaseActual.idSeccion,
                idGrupo: calClaseActual.idGrupo
            }
        });

        $("#detCuerpo").html(response.esCorrecto
            ? renderDetalleCalificaciones(response.dato)
            : `<div class="text-danger">${escapeHtml(response.mensaje)}</div>`);
    } catch (e) {
        $("#detCuerpo").html('<div class="text-danger">No fue posible consultar el detalle.</div>');
    }
}

async function abrirHistorial() {
    let idMatricula = parseInt($(this).data("matricula"), 10);
    let est = calPlanilla.estudiantes.find(e => e.idMatricula === idMatricula);

    $("#hisEstudiante").text(`${est.nombre} · ${calClaseActual.descripcion}`);
    $("#hisCuerpo").html('<tr><td colspan="6" class="text-center text-muted">Cargando...</td></tr>');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalHistorial')).show();

    try {
        let response = await $.ajax({
            url: '/Calificacion/GetHistorial',
            type: 'GET',
            data: {
                idMatricula: idMatricula,
                idPeriodo: calClaseActual.idPeriodo,
                idCurso: calClaseActual.idCurso,
                idSeccion: calClaseActual.idSeccion,
                idGrupo: calClaseActual.idGrupo
            }
        });

        if (!response.esCorrecto) {
            $("#hisCuerpo").html(`<tr><td colspan="6" class="text-center text-danger">${escapeHtml(response.mensaje)}</td></tr>`);
            return;
        }

        let lista = response.dato || [];

        if (lista.length === 0) {
            $("#hisCuerpo").html(`<tr><td colspan="6" class="text-center text-muted">${escapeHtml(response.mensaje || 'Sin movimientos')}</td></tr>`);
            return;
        }

        let colores = { Registro: 'secondary', Edicion: 'primary', Correccion: 'warning' };
        let nombres = { Registro: 'Registro', Edicion: 'Edición', Correccion: 'Corrección' };
        let html = '';

        lista.forEach(function (h) {
            let anterior = h.valorAnterior == null ? '—' : fmtNota(h.valorAnterior);

            html += `<tr>
                        <td class="text-nowrap">${new Date(h.fecha).toLocaleString('es-CR', { dateStyle: 'short', timeStyle: 'short' })}</td>
                        <td>${escapeHtml(h.actividad)}<div class="small text-muted">${escapeHtml(h.rubro)}</div></td>
                        <td><span class="badge text-bg-${colores[h.tipo] || 'secondary'}">${nombres[h.tipo] || escapeHtml(h.tipo)}</span></td>
                        <td class="text-center text-nowrap">${anterior} → <strong>${fmtNota(h.valorNuevo)}</strong> / ${fmtNota(h.puntosActividad)}</td>
                        <td>${h.observacion ? escapeHtml(h.observacion) : '<span class="text-muted">—</span>'}</td>
                        <td>${escapeHtml(h.usuario)}</td>
                     </tr>`;
        });

        $("#hisCuerpo").html(html);
    } catch (e) {
        $("#hisCuerpo").html('<tr><td colspan="6" class="text-center text-danger">No fue posible consultar el historial.</td></tr>');
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
