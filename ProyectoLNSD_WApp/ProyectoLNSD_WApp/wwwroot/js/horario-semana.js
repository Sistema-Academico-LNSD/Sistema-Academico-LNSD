// Funciones compartidas por las pantallas que muestran horarios como cuadrícula semanal

const DIAS_SEMANA = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes'];
const MARCA_PERIODO_ACTIVO = ' (activo)';

// Llena la lista de períodos, deja elegido el activo y avisa cuando está lista
function cargarPeriodosConsulta(selector, alTerminar) {
    $.ajax({
        url: '/Horario/GetPeriodos',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto || !response.dato || response.dato.length === 0) {
                $(selector).html('<option value="">Sin períodos</option>');

                // Sin períodos, la consulta avisará del motivo
                alTerminar();
                return;
            }

            let opciones = '';

            $.each(response.dato, function (index, p) {
                opciones += `<option value="${p.id}">${escapeHtml(p.nombre)}</option>`;
            });

            $(selector).html(opciones);

            let activo = response.dato.find(p => p.nombre.endsWith(MARCA_PERIODO_ACTIVO)) ?? response.dato[0];

            $(selector).val(activo.id);

            alTerminar();
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los períodos.", 'danger');
        }
    });
}

function textoContador(total) {
    return total + (total === 1 ? ' clase' : ' clases');
}

// El nombre del grupo puede traer ya la palabra "Grupo" (ej. "Grupo C"); así no se repite
function etiquetaGrupo(nombre) {
    return /^grupo\b/i.test(nombre) ? nombre : `Grupo ${nombre}`;
}

// Cómo se ve cada clase dentro de la cuadrícula del docente
function formatoClaseDocente(h) {
    return `<strong>${escapeHtml(h.nombreCurso)}</strong><br>
            <span class="text-muted">${escapeHtml(h.nombreGrado)} - ${escapeHtml(h.nombreSeccion)} · ${escapeHtml(etiquetaGrupo(h.nombreGrupo))}</span><br>
            <i class="bi bi-geo-alt"></i> ${escapeHtml(h.aula)}`;
}

// Cómo se ve cada clase en la cuadrícula del estudiante, del encargado y de la vista por grupo
function formatoClaseEstudiante(h) {
    return `<strong>${escapeHtml(h.nombreCurso)}</strong><br>
            <span class="text-muted">${escapeHtml(h.nombreDocente)}</span><br>
            <i class="bi bi-geo-alt"></i> ${escapeHtml(h.aula)}`;
}

// Días en columnas y bloques de horas en filas (HU 01-08, esc. 3).
// Cada combinación distinta de hora de inicio y fin es una fila, de la más temprana a la más tardía.
function pintarCuadricula(selector, horarios, mensajeVacio, formatoClase) {
    if (!horarios || horarios.length === 0) {
        $(selector).html(`
            <div class="text-center text-muted py-4">
                <i class="bi bi-calendar-x fs-2"></i>
                <p class="mb-0 mt-2">${escapeHtml(mensajeVacio)}</p>
            </div>`);
        return;
    }

    // Lunes a viernes y, si en los datos apareciera otro día, también se agrega
    let dias = DIAS_SEMANA.slice();

    $.each(horarios, function (index, h) {
        if (!dias.includes(h.dia)) {
            dias.push(h.dia);
        }
    });

    let bloques = [];

    $.each(horarios, function (index, h) {
        let clave = `${h.horaInicio}|${h.horaFin}`;

        if (!bloques.includes(clave)) {
            bloques.push(clave);
        }
    });

    // Todas las horas tienen formato HH:mm, así que ordenar como texto las ordena bien
    bloques.sort();

    let html = `
        <div class="table-responsive">
            <table class="table table-bordered align-middle mb-0">
                <thead>
                    <tr>
                        <th style="width: 9rem;">Hora</th>`;

    $.each(dias, function (index, d) {
        html += `<th class="text-center">${escapeHtml(d)}</th>`;
    });

    html += `</tr></thead><tbody>`;

    $.each(bloques, function (index, clave) {
        let [inicio, fin] = clave.split('|');

        html += `<tr><th class="text-nowrap bg-light">${escapeHtml(inicio)} - ${escapeHtml(fin)}</th>`;

        $.each(dias, function (i, dia) {
            let clases = horarios.filter(h => h.dia === dia && h.horaInicio === inicio && h.horaFin === fin);

            html += `<td style="min-width: 11rem;">`;

            $.each(clases, function (j, h) {
                html += `<div class="border rounded p-2 mb-1 bg-light small">${formatoClase(h)}</div>`;
            });

            html += `</td>`;
        });

        html += `</tr>`;
    });

    html += `</tbody></table></div>`;

    $(selector).html(html);
}