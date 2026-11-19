// Estudiante y encargado: vigencia y monto de las becas activas (MBEF-01-07).
// El servidor decide de quién son las becas a partir de la sesión.

$(function () {
    $.get('/Beca/GetMisBecas')
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                const clase = r.esCorrecto ? 'alert-info' : 'alert-danger';
                $('#mbContenido').html(
                    `<div class="alert ${clase}"><i class="bi bi-info-circle"></i> ${escapeHtml(r.mensaje)}</div>`);
                return;
            }

            $('#mbContenido').html(lista.map(tarjetaEstudiante).join(''));
        })
        .fail(() => $('#mbContenido').html(
            '<div class="alert alert-danger">No fue posible consultar las becas. Intente nuevamente.</div>'));
});

function tarjetaEstudiante(e) {
    return `
        <div class="card mb-4">
            <div class="card-header">
                <span class="fw-semibold">${escapeHtml(e.nombreEstudiante)}</span>
                <span class="small text-muted ms-2">${escapeHtml(e.carnet)}${e.grado ? ' · ' + escapeHtml(e.grado) : ''}</span>
            </div>
            <div class="card-body">
                <div class="row g-3">${e.becas.map(tarjetaBeca).join('')}</div>
            </div>
        </div>`;
}

function tarjetaBeca(b) {
    const restante = b.diasRestantes !== null && b.diasRestantes !== undefined
        ? `<div class="small text-muted">${b.diasRestantes === 0 ? 'Vence hoy' : `Quedan ${b.diasRestantes} días`}</div>`
        : '';

    return `
        <div class="col-md-6 col-xl-4">
            <div class="border rounded p-3 h-100">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <span class="small text-muted">${escapeHtml(b.nombrePeriodo)}</span>
                    ${badgeVigencia(b.vigencia)}
                </div>
                <div class="fs-4 fw-semibold">${formatoMonto(b.monto)}</div>
                <div class="small text-muted mb-1">Monto asignado</div>
                <div><i class="bi bi-calendar-range"></i> ${rangoFechas(b.fechaInicio, b.fechaFin)}</div>
                ${restante}
            </div>
        </div>`;
}
