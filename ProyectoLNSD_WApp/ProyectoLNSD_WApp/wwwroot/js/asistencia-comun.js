// Utilidades compartidas por las pantallas de asistencia (MCAF-01):
// estados, formatos, historial de un estudiante (MCAF-01-06) y descarga del reporte.

// "Tardia" se guarda sin tilde (CHECK de la tabla); la etiqueta es lo que se muestra
const ESTADOS_ASISTENCIA = [
    { valor: 'Presente', etiqueta: 'Presente', corta: 'P', clase: 'success' },
    { valor: 'Ausente', etiqueta: 'Ausente', corta: 'A', clase: 'danger' },
    { valor: 'Tardia', etiqueta: 'Tardía', corta: 'T', clase: 'warning' },
    { valor: 'Justificada', etiqueta: 'Justificada', corta: 'J', clase: 'info' }
];

function infoEstado(valor) {
    return ESTADOS_ASISTENCIA.find(e => e.valor === valor)
        || { valor: valor, etiqueta: valor, corta: '?', clase: 'secondary' };
}

function badgeEstado(valor) {
    const e = infoEstado(valor);
    return `<span class="badge text-bg-${e.clase}">${escapeHtml(e.etiqueta)}</span>`;
}

function badgePorcentaje(porcentaje, bajoUmbral) {
    const clase = bajoUmbral ? 'text-bg-danger' : 'text-bg-success';
    return `<span class="badge ${clase}">${Number(porcentaje).toFixed(1)} %</span>`;
}

// escapeHtml no escapa comillas: para valores dentro de atributos se usa esta
function escapeAttr(texto) {
    return escapeHtml(texto).replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

// "2026-11-09" -> "09/11/2026" sin pasar por Date (evita el desfase de zona horaria)
function fechaCorta(iso) {
    if (!iso) return '';
    const [anio, mes, dia] = String(iso).substring(0, 10).split('-');
    return `${dia}/${mes}/${anio}`;
}

// Fecha local de hoy en formato yyyy-MM-dd (para los <input type="date">)
function fechaIso(d) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function contadoresEstados(cuenta) {
    return ESTADOS_ASISTENCIA
        .map(e => `<span class="badge text-bg-${e.clase}">${e.etiqueta}: ${cuenta[e.valor] || 0}</span>`)
        .join('');
}

function llenarSelect(selector, opciones, textoTodos, valorSeleccionado) {
    const html = (textoTodos ? `<option value="">${escapeHtml(textoTodos)}</option>` : '') +
        (opciones || []).map(o =>
            `<option value="${Number(o.id)}">${escapeHtml(o.nombre)}</option>`).join('');

    $(selector).html(html);

    if (valorSeleccionado !== undefined && valorSeleccionado !== null) {
        $(selector).val(String(valorSeleccionado));
    }
}

// ---------- Historial de un estudiante en un curso (MCAF-01-06) ----------
// Los botones llevan data-accion="historial" (sin onclick en línea)

$(document).on('click', '[data-accion="historial"]', function () {
    const d = $(this).data();
    verHistorial(d.idMatricula, d.idCurso, d.estudiante, d.curso);
});

function verHistorial(idMatricula, idCurso, nombreEstudiante, nombreCurso) {
    $('#historialSubtitulo').text(`${nombreEstudiante} · ${nombreCurso}`);
    $('#historialResumen').empty();
    $('#historialFilas').html('<tr><td colspan="4" class="text-center text-muted">Cargando...</td></tr>');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalHistorial')).show();

    $.get('/Asistencia/GetHistorial', { idMatricula: idMatricula, idCurso: idCurso })
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                $('#historialFilas').html(
                    `<tr><td colspan="4" class="text-center ${clase}">${escapeHtml(r.mensaje)}</td></tr>`);
                return;
            }

            const cuenta = {};
            lista.forEach(x => { cuenta[x.estado] = (cuenta[x.estado] || 0) + 1; });
            $('#historialResumen').html(contadoresEstados(cuenta));

            $('#historialFilas').html(lista.map(x => `
                <tr>
                    <td>${fechaCorta(x.fecha)}</td>
                    <td>${escapeHtml(x.dia)} ${escapeHtml(x.horaInicio)}–${escapeHtml(x.horaFin)}</td>
                    <td>${badgeEstado(x.estado)}${x.modificado ? ' <span class="small text-muted">(corregido)</span>' : ''}</td>
                    <td class="small">${escapeHtml(x.observacion || '')}</td>
                </tr>`).join(''));
        })
        .fail(function () {
            $('#historialFilas').html(
                '<tr><td colspan="4" class="text-center text-danger">No fue posible consultar el historial.</td></tr>');
        });
}

// ---------- Descarga de archivos (reporte Excel, MCAF-01-12) ----------
// Si el servidor responde JSON (sin datos o error), se muestra como mensaje.

function descargarArchivo(url, nombreSugerido) {
    fetch(url, { credentials: 'same-origin' })
        .then(function (respuesta) {
            const tipo = respuesta.headers.get('Content-Type') || '';

            if (tipo.includes('application/json')) {
                return respuesta.json().then(r => mostrarMensaje(r.mensaje, 'warning'));
            }

            if (tipo.includes('text/html')) {
                mostrarMensaje('Su sesión expiró. Inicie sesión nuevamente.', 'warning');
                return;
            }

            if (!respuesta.ok) {
                throw new Error('descarga');
            }

            return respuesta.blob().then(function (archivo) {
                const enlaceTemporal = URL.createObjectURL(archivo);
                const enlace = document.createElement('a');

                enlace.href = enlaceTemporal;
                enlace.download = nombreSugerido;
                document.body.appendChild(enlace);
                enlace.click();
                enlace.remove();

                setTimeout(() => URL.revokeObjectURL(enlaceTemporal), 1000);
            });
        })
        .catch(() => mostrarMensaje('No se pudo descargar el reporte. Intente nuevamente.', 'danger'));
}
