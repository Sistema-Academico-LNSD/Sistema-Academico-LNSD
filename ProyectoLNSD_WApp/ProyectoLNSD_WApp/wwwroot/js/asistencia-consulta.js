// Consulta y reportes de asistencia (MCAF-01-09, 01-10, 01-12) y cierre del período (MCAF-01-13).
// El servidor limita a un docente a sus propias clases; aquí no se decide nada de permisos.

let opcionesConsulta = null;
let registrosCargados = false;

$(function () {
    cargarOpciones(null);

    $('#fPeriodo').on('change', () => cargarOpciones($('#fPeriodo').val()));
    $('#fGrado').on('change', filtrarSecciones);
    $('#formFiltros').on('submit', function (e) {
        e.preventDefault();
        buscar();
    });
    $('#btnLimpiar').on('click', limpiarFiltros);
    $('#fEstado').on('change', cargarRegistros);
    $('#tab-registros').on('shown.bs.tab', function () {
        if (!registrosCargados) cargarRegistros();
    });
    $('#btnExportar').on('click', exportar);
    $('#btnCerrarPeriodo').on('click', cerrarPeriodo);
});

// ---------- Filtros ----------

function cargarOpciones(idPeriodo) {
    $.get('/Asistencia/GetOpciones', { idPeriodo: idPeriodo || null })
        .done(function (r) {
            if (!r.esCorrecto) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }

            opcionesConsulta = r.dato;

            llenarSelect('#fPeriodo', opcionesConsulta.periodos, null, opcionesConsulta.idPeriodo);
            llenarSelect('#fGrado', opcionesConsulta.grados, 'Todos los grados');
            llenarSelect('#fGrupo', opcionesConsulta.grupos, 'Todos los grupos');
            llenarSelect('#fCurso', opcionesConsulta.cursos, 'Todos los cursos');
            filtrarSecciones();

            $('#fUmbral').attr('placeholder', opcionesConsulta.umbralSugerido);

            buscar();
            cargarCierre();
        })
        .fail(() => mostrarMensaje('No fue posible cargar los filtros.', 'danger'));
}

function filtrarSecciones() {
    if (!opcionesConsulta) return;

    const idGrado = $('#fGrado').val();
    const secciones = opcionesConsulta.secciones
        .filter(s => !idGrado || String(s.idGrado) === idGrado)
        .map(s => ({ id: s.idSeccion, nombre: `${s.nombreGrado} ${s.nombre}` }));

    llenarSelect('#fSeccion', secciones, 'Todas las secciones');
}

function limpiarFiltros() {
    $('#fGrado, #fSeccion, #fGrupo, #fCurso, #fTexto, #fDesde, #fHasta, #fUmbral, #fEstado').val('');
    $('#fSoloBajo').prop('checked', false);
    filtrarSecciones();
    buscar();
}

// Solo se envían los filtros con valor
function leerFiltro(incluirEstado) {
    const filtro = {
        idPeriodo: $('#fPeriodo').val(),
        idGrado: $('#fGrado').val(),
        idSeccion: $('#fSeccion').val(),
        idGrupo: $('#fGrupo').val(),
        idCurso: $('#fCurso').val(),
        texto: $('#fTexto').val().trim(),
        desde: $('#fDesde').val(),
        hasta: $('#fHasta').val(),
        umbral: $('#fUmbral').val(),
        soloBajoUmbral: $('#fSoloBajo').is(':checked') ? 'true' : ''
    };

    if (incluirEstado) {
        filtro.estado = $('#fEstado').val();
    }

    Object.keys(filtro).forEach(k => { if (!filtro[k]) delete filtro[k]; });

    return filtro;
}

function buscar() {
    registrosCargados = false;
    cargarResumen();

    if ($('#tab-registros').hasClass('active')) {
        cargarRegistros();
    }
}

// ---------- Resumen por estudiante y curso ----------

function cargarResumen() {
    const $cuerpo = $('#tblResumen tbody');
    $cuerpo.html('<tr><td colspan="11" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Asistencia/GetResumen', leerFiltro(false))
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="11" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                $('#resumenContadores').empty();
                return;
            }

            const bajos = lista.filter(x => x.bajoUmbral).length;
            const umbral = $('#fUmbral').val() || (opcionesConsulta ? opcionesConsulta.umbralSugerido : '');

            $('#resumenContadores').html(
                `<span class="badge text-bg-light border">Filas: ${lista.length}</span>` +
                `<span class="badge text-bg-danger">Bajo ${escapeHtml(String(umbral))} %: ${bajos}</span>`);

            $cuerpo.html(lista.map(x => `
                <tr class="${x.bajoUmbral ? 'table-danger' : ''}">
                    <td>
                        <div class="fw-semibold">${escapeHtml(x.nombreEstudiante)}</div>
                        <div class="small text-muted">${escapeHtml(x.carnet)}</div>
                    </td>
                    <td>${escapeHtml(x.nombreGrado)} ${escapeHtml(x.nombreSeccion)}</td>
                    <td>${escapeHtml(x.nombreGrupo)}</td>
                    <td>${escapeHtml(x.nombreCurso)}</td>
                    <td class="text-center">${x.presentes}</td>
                    <td class="text-center">${x.ausentes}</td>
                    <td class="text-center">${x.tardias}</td>
                    <td class="text-center">${x.justificadas}</td>
                    <td class="text-center">${x.total}</td>
                    <td class="text-center">${badgePorcentaje(x.porcentaje, x.bajoUmbral)}</td>
                    <td class="text-end">
                        <button type="button" class="btn btn-outline-secondary btn-sm" title="Historial"
                                data-accion="historial" data-id-matricula="${Number(x.idMatricula)}"
                                data-id-curso="${Number(x.idCurso)}" data-estudiante="${escapeAttr(x.nombreEstudiante)}"
                                data-curso="${escapeAttr(x.nombreCurso)}">
                            <i class="bi bi-clock-history"></i>
                        </button>
                    </td>
                </tr>`).join(''));
        })
        .fail(function () {
            $cuerpo.html('<tr><td colspan="11" class="text-center text-danger">No fue posible generar el resumen.</td></tr>');
        });
}

// ---------- Registros individuales ----------

function cargarRegistros() {
    const $cuerpo = $('#tblRegistros tbody');
    $cuerpo.html('<tr><td colspan="8" class="text-center text-muted">Cargando...</td></tr>');
    $('#registrosConteo').text('');

    $.get('/Asistencia/GetRegistros', leerFiltro(true))
        .done(function (r) {
            registrosCargados = true;

            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $cuerpo.html(`<tr><td colspan="8" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            $cuerpo.html(lista.map(x => `
                <tr>
                    <td>${fechaCorta(x.fecha)}</td>
                    <td class="small">${escapeHtml(x.dia)} ${escapeHtml(x.horaInicio)}–${escapeHtml(x.horaFin)}</td>
                    <td>${escapeHtml(x.nombreCurso)}</td>
                    <td>${escapeHtml(x.nombreGrado)} ${escapeHtml(x.nombreSeccion)} · ${escapeHtml(x.nombreGrupo)}</td>
                    <td>${escapeHtml(x.nombreEstudiante)}</td>
                    <td>${badgeEstado(x.estado)}${x.modificado ? ' <span class="small text-muted">(corregido)</span>' : ''}</td>
                    <td class="small">${escapeHtml(x.observacion || '')}</td>
                    <td class="small">${escapeHtml(x.nombreDocente)}</td>
                </tr>`).join(''));

            $('#registrosConteo').text(r.mensaje && r.mensaje.startsWith('Se muestran')
                ? r.mensaje
                : `${lista.length} registro(s)`);
        })
        .fail(function () {
            $cuerpo.html('<tr><td colspan="8" class="text-center text-danger">No fue posible consultar los registros.</td></tr>');
        });
}

// ---------- Reporte Excel (MCAF-01-12) ----------

function exportar() {
    const consulta = $.param(leerFiltro(false));
    descargarArchivo('/Asistencia/ExportarResumen?' + consulta, `Asistencia_${fechaIso(new Date())}.xlsx`);
}

// ---------- Cierre del período (MCAF-01-13) ----------

function cargarCierre() {
    const idPeriodo = $('#fPeriodo').val();
    const $aviso = $('#estadoCierre');

    if (!idPeriodo) {
        $aviso.addClass('d-none');
        return;
    }

    $.get('/Asistencia/GetCierre', { idPeriodo: idPeriodo })
        .done(function (r) {
            if (!r.esCorrecto) {
                $aviso.addClass('d-none');
                return;
            }

            const c = r.dato;

            if (c.cerrado) {
                const fecha = c.fechaCierre ? fechaCorta(c.fechaCierre) : '';
                $aviso.attr('class', 'alert alert-secondary')
                    .html(`<i class="bi bi-lock-fill"></i> El registro de asistencia de <strong>${escapeHtml(c.nombrePeriodo)}</strong> ` +
                        `está cerrado desde el ${fecha}${c.cerradoPor ? ' (' + escapeHtml(c.cerradoPor) + ')' : ''}. Solo se puede consultar.`);
            } else {
                $aviso.attr('class', 'alert alert-light border')
                    .html(`<i class="bi bi-unlock"></i> El registro de asistencia de <strong>${escapeHtml(c.nombrePeriodo)}</strong> está abierto.` +
                        (c.motivo ? ` <span class="text-muted">${escapeHtml(c.motivo)}.</span>` : ''));
            }

            $('#btnCerrarPeriodo').prop('disabled', !c.puedeCerrar).data('periodo', c.nombrePeriodo);
        });
}

async function cerrarPeriodo() {
    const idPeriodo = $('#fPeriodo').val();
    const nombre = $('#btnCerrarPeriodo').data('periodo') || 'seleccionado';

    const ok = await confirmar(
        `¿Cerrar el registro de asistencia del período ${nombre}? Después nadie podrá registrar ni modificar asistencia de ese período.`,
        'Cerrar período', 'danger');

    if (!ok) return;

    $.post('/Asistencia/CerrarPeriodo', { idPeriodo: idPeriodo })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');
            cargarCierre();
        })
        .fail(() => mostrarMensaje('No se pudo cerrar el período.', 'danger'));
}
