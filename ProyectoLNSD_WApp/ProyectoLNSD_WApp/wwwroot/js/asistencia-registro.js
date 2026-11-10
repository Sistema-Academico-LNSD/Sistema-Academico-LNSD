// Pasar asistencia (MCAF-01-01 a 01-05). El docente elige una de sus clases y la fecha,
// marca el estado de cada estudiante y guarda la lista completa.

let clasesDocente = [];
let listaActual = null;   // última lista cargada (AsistenciaListaDTO)

const DIAS_SEMANA = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

$(function () {
    const hoy = fechaIso(new Date());
    $('#asFecha').attr('max', hoy).val(hoy);

    cargarClases();

    $('#asClase').on('change', sugerirFecha);
    $('#asFecha').on('change', ocultarLista);
    $('#btnCargarLista').on('click', cargarLista);
    $('#btnTodosPresentes').on('click', marcarTodosPresentes);
    $('#btnGuardarLista').on('click', guardarLista);
    $('#tblLista').on('change', 'input[type="radio"]', actualizarContadores);
});

function cargarClases() {
    $.get('/Asistencia/GetMisClases')
        .done(function (r) {
            const $select = $('#asClase');
            clasesDocente = r.esCorrecto ? (r.dato || []) : [];

            if (!clasesDocente.length) {
                $select.html('<option value="">No tiene clases disponibles</option>');
                mostrarMensaje(r.mensaje || 'No tiene clases asignadas.', r.esCorrecto ? 'warning' : 'danger');
                return;
            }

            $select.html('<option value="">Seleccione una clase</option>' +
                clasesDocente.map(c =>
                    `<option value="${Number(c.idHorario)}">${escapeHtml(describirClase(c))}</option>`).join(''));
        })
        .fail(() => mostrarMensaje('No fue posible cargar sus clases.', 'danger'));
}

function describirClase(c) {
    return `${c.dia} ${c.horaInicio}–${c.horaFin} · ${c.codigoCurso} ${c.nombreCurso} · ` +
        `${c.nombreGrado} ${c.nombreSeccion} · Grupo ${c.nombreGrupo} · Aula ${c.aula}`;
}

// Propone la fecha más reciente (hasta hoy) que cae en el día de la clase
function sugerirFecha() {
    ocultarLista();

    const clase = clasesDocente.find(c => String(c.idHorario) === $('#asClase').val());
    if (!clase) return;

    const diaClase = DIAS_SEMANA.indexOf(clase.dia);
    if (diaClase < 0) return;

    const fecha = new Date();
    while (fecha.getDay() !== diaClase) {
        fecha.setDate(fecha.getDate() - 1);
    }

    $('#asFecha').val(fechaIso(fecha));
}

function ocultarLista() {
    listaActual = null;
    $('#tarjetaLista').addClass('d-none');
}

function cargarLista() {
    const idHorario = $('#asClase').val();
    const fecha = $('#asFecha').val();

    if (!idHorario) {
        mostrarMensaje('Seleccione una clase.', 'warning');
        return;
    }

    if (!fecha) {
        mostrarMensaje('Seleccione la fecha.', 'warning');
        return;
    }

    $.get('/Asistencia/GetLista', { idHorario: idHorario, fecha: fecha })
        .done(function (r) {
            if (!r.esCorrecto) {
                ocultarLista();
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }

            pintarLista(r.dato);
        })
        .fail(() => mostrarMensaje('No fue posible cargar la lista de estudiantes.', 'danger'));
}

function pintarLista(lista) {
    listaActual = lista;

    const c = lista.clase;
    const editable = lista.puedeGuardar;

    $('#listaTitulo').text(`${c.codigoCurso} ${c.nombreCurso}`);
    $('#listaSubtitulo').text(
        `${c.nombreGrado} ${c.nombreSeccion} · Grupo ${c.nombreGrupo} · ${c.dia} ${fechaCorta(lista.fecha)} · ` +
        `${c.horaInicio}–${c.horaFin} · Aula ${c.aula}`);

    $('#listaEstado')
        .attr('class', 'badge ' + (lista.yaRegistrada ? 'text-bg-info' : 'text-bg-secondary'))
        .text(lista.yaRegistrada ? 'Lista ya registrada: puede corregirla' : 'Lista nueva');

    if (lista.aviso) {
        $('#listaAviso').text(lista.aviso).removeClass('d-none');
    } else {
        $('#listaAviso').addClass('d-none').text('');
    }

    const filas = lista.estudiantes.map((e, i) => `
        <tr data-id-matricula="${Number(e.idMatricula)}">
            <td class="text-muted">${i + 1}</td>
            <td>
                <div class="fw-semibold">${escapeHtml(e.nombreEstudiante)}</div>
                <div class="small text-muted">${escapeHtml(e.carnet)}</div>
            </td>
            <td>${grupoEstados(e, editable)}</td>
            <td>
                <input type="text" class="form-control form-control-sm" maxlength="250"
                       value="${escapeAttr(e.observacion || '')}" ${editable ? '' : 'disabled'}
                       placeholder="Opcional" aria-label="Observación de ${escapeAttr(e.nombreEstudiante)}" />
            </td>
            <td class="text-end">
                <button type="button" class="btn btn-outline-secondary btn-sm" title="Historial en este curso"
                        data-accion="historial" data-id-matricula="${Number(e.idMatricula)}"
                        data-id-curso="${Number(c.idCurso)}" data-estudiante="${escapeAttr(e.nombreEstudiante)}"
                        data-curso="${escapeAttr(c.nombreCurso)}">
                    <i class="bi bi-clock-history"></i>
                </button>
            </td>
        </tr>`).join('');

    $('#tblLista tbody').html(filas ||
        '<tr><td colspan="5" class="text-center text-muted">No hay estudiantes matriculados en esta clase.</td></tr>');

    $('#btnTodosPresentes, #btnGuardarLista').prop('disabled', !editable);
    $('#btnGuardarLista').html(lista.yaRegistrada
        ? '<i class="bi bi-save"></i> Guardar cambios'
        : '<i class="bi bi-save"></i> Guardar asistencia');

    $('#tarjetaLista').removeClass('d-none');
    actualizarContadores();
}

// Botones P / A / T / J (en pantallas grandes muestran la palabra completa)
function grupoEstados(e, editable) {
    const nombre = `estado-${Number(e.idMatricula)}`;

    return `<div class="btn-group btn-group-sm" role="group" aria-label="Estado de asistencia">` +
        ESTADOS_ASISTENCIA.map(s => {
            const id = `${nombre}-${s.valor}`;
            return `<input type="radio" class="btn-check" name="${nombre}" id="${id}" value="${s.valor}"
                           autocomplete="off" ${e.estado === s.valor ? 'checked' : ''} ${editable ? '' : 'disabled'} />
                    <label class="btn btn-outline-${s.clase}" for="${id}" title="${s.etiqueta}">${s.corta}<span class="d-none d-xl-inline">${s.etiqueta.substring(1)}</span></label>`;
        }).join('') +
        `</div>`;
}

function marcarTodosPresentes() {
    // Solo marca a quienes no tienen estado: no pisa lo que el docente ya marcó
    $('#tblLista tbody tr[data-id-matricula]').each(function () {
        if (!$(this).find('input[type="radio"]:checked').length) {
            $(this).find('input[value="Presente"]').prop('checked', true);
        }
    });

    actualizarContadores();
}

function actualizarContadores() {
    const cuenta = {};
    let sinMarcar = 0;

    $('#tblLista tbody tr[data-id-matricula]').each(function () {
        const estado = $(this).find('input[type="radio"]:checked').val();

        if (estado) {
            cuenta[estado] = (cuenta[estado] || 0) + 1;
            $(this).removeClass('table-warning');
        } else {
            sinMarcar++;
        }
    });

    $('#listaContadores').html(contadoresEstados(cuenta) +
        (sinMarcar ? ` <span class="badge text-bg-light border">Sin marcar: ${sinMarcar}</span>` : ''));
}

function guardarLista() {
    if (!listaActual) return;

    const registros = [];
    let sinMarcar = 0;

    $('#tblLista tbody tr[data-id-matricula]').each(function () {
        const estado = $(this).find('input[type="radio"]:checked').val();

        if (!estado) {
            sinMarcar++;
            $(this).addClass('table-warning');
            return;
        }

        registros.push({
            idMatricula: Number($(this).data('id-matricula')),
            estado: estado,
            observacion: $(this).find('input[type="text"]').val().trim() || null
        });
    });

    if (sinMarcar) {
        mostrarMensaje(`Falta marcar la asistencia de ${sinMarcar} estudiante(s).`, 'warning');
        return;
    }

    const $boton = $('#btnGuardarLista').prop('disabled', true);

    $.ajax({
        url: '/Asistencia/Guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            idHorario: listaActual.clase.idHorario,
            fecha: listaActual.fecha,
            registros: registros
        }),

        success: function (r) {
            if (r.esCorrecto) {
                mostrarMensaje(r.mensaje, 'success');
                pintarLista(r.dato);
            } else {
                mostrarMensaje(r.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje('No se pudo guardar la asistencia. Intente nuevamente.', 'danger');
        },

        complete: function () {
            $boton.prop('disabled', !(listaActual && listaActual.puedeGuardar));
        }
    });
}
