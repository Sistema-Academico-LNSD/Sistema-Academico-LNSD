// Detalle de calificaciones de un estudiante (MCNF-01-09, 01-10, 01-11 y 01-15).
// Lo usan el estudiante, el encargado, el administrador, el director y el docente.
// Usa el helper global escapeHtml de site.js.

function fmtDet(valor) {
    return String(+Number(valor).toFixed(2));
}

function fmtFechaDet(fecha) {
    return fecha ? new Date(fecha).toLocaleDateString('es-CR') : '—';
}

function badgeEstado(estado) {
    let colores = { 'Aprobado': 'success', 'Reprobado': 'danger', 'Aprobando': 'success', 'Reprobando': 'warning', 'Sin notas': 'secondary',
                    'Promovido': 'success', 'No promovido': 'danger', 'En curso': 'primary', 'Sin cursos': 'secondary' };

    return `<span class="badge text-bg-${colores[estado] || 'secondary'}">${escapeHtml(estado)}</span>`;
}

function renderDetalleCalificaciones(d) {
    let html = `<div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
                    <div>
                        <div class="fw-semibold fs-5">${escapeHtml(d.nombre)}</div>
                        <div class="small text-muted">${escapeHtml(d.carnet)}${d.grado ? ' · ' + escapeHtml(d.grado) : ''} · Año lectivo ${d.anio} · Nota mínima ${fmtDet(d.notaMinima)}</div>
                    </div>`;

    // Promoción del año: aprueba el año quien aprueba todas las materias
    if (d.estado) {
        html += `<div class="text-end">
                    <div>Año lectivo: ${badgeEstado(d.estado)}</div>
                    <div class="small text-muted">${d.aprobados} aprobados · ${d.reprobados} reprobados · ${d.enCurso} en curso</div>
                 </div>`;
    }

    html += '</div>';

    d.cursos.forEach(function (c) {
        html += `<div class="card mb-3">
                    <div class="card-header d-flex flex-wrap justify-content-between align-items-center gap-2">
                        <strong>${escapeHtml(c.curso)}</strong>
                        <span>
                            Nota del año: <strong>${c.notaAnual == null ? '—' : fmtDet(c.notaAnual) + '%'}</strong>
                            ${c.definitivo ? '' : '<span class="small text-muted">(parcial)</span>'}
                            ${badgeEstado(c.estado)}
                        </span>
                    </div>
                    <div class="card-body">`;

        if (c.periodos.length === 0) {
            html += '<div class="text-muted">Sin información en el año lectivo.</div>';
        }

        c.periodos.forEach(function (p) {
            html += renderPeriodoDetalle(p, d.incluyeObservaciones);
        });

        html += '</div></div>';
    });

    return html;
}

function renderPeriodoDetalle(p, conObservaciones) {
    let columnas = conObservaciones ? 7 : 6;

    let html = `<div class="d-flex justify-content-between align-items-center mb-1">
                    <div class="fw-semibold">${escapeHtml(p.periodo)} <span class="small text-muted">${p.activo ? '(en curso)' : '(cerrado)'}</span></div>
                    <div>Promedio del semestre: <strong>${p.promedio == null ? '—' : fmtDet(p.promedio) + '%'}</strong></div>
                </div>
                <div class="table-responsive mb-3">
                <table class="table table-sm align-middle mb-0">
                    <thead>
                        <tr>
                            <th>Rubro / actividad</th>
                            <th>Fecha</th>
                            <th class="text-center">Nota</th>
                            <th class="text-center">Porcentaje</th>
                            <th class="text-center">Peso en el año</th>
                            <th class="text-center">Aporte</th>
                            ${conObservaciones ? '<th>Observación</th>' : ''}
                        </tr>
                    </thead>
                    <tbody>`;

    p.rubros.forEach(function (r) {
        html += `<tr class="table-light">
                    <td colspan="3" class="fw-semibold">${escapeHtml(r.nombre)}
                        <span class="small text-muted fw-normal">${fmtDet(r.porcentaje)}% de la nota del año</span></td>
                    <td class="text-center fw-semibold">${r.evaluado > 0 ? fmtDet(r.acumulado / r.evaluado * 100) + '%' : '—'}</td>
                    <td></td>
                    <td class="text-center small">${fmtDet(r.acumulado)} de ${fmtDet(r.evaluado)}</td>
                    ${conObservaciones ? '<td></td>' : ''}
                 </tr>`;

        if (r.actividades.length === 0) {
            html += `<tr><td colspan="${columnas}" class="text-muted small ps-4">Sin actividades todavía.</td></tr>`;
        }

        r.actividades.forEach(function (a) {
            html += `<tr>
                        <td class="ps-4">${escapeHtml(a.actividad)}</td>
                        <td class="text-nowrap">${fmtFechaDet(a.fecha)}</td>
                        <td class="text-center">${a.puntosObtenidos == null ? '<span class="text-muted">Sin calificar</span>' : fmtDet(a.puntosObtenidos) + ' / ' + fmtDet(a.puntos)}</td>
                        <td class="text-center">${a.porcentaje == null ? '—' : fmtDet(a.porcentaje) + '%'}</td>
                        <td class="text-center">${fmtDet(a.peso)}%</td>
                        <td class="text-center">${a.aporte == null ? '—' : fmtDet(a.aporte)}</td>
                        ${conObservaciones ? `<td class="small">${a.observacion ? escapeHtml(a.observacion) : '<span class="text-muted">—</span>'}</td>` : ''}
                     </tr>`;
        });
    });

    // Asistencia: solo cuenta cuando ya hay registros
    let asi = p.asistencia;

    html += `<tr class="table-light">
                <td colspan="3" class="fw-semibold">Asistencia</td>
                <td class="text-center fw-semibold">${asi.registros === 0 ? '—' : fmtDet(asi.porcentaje) + '%'}</td>
                <td></td>
                <td class="text-center small">${asi.evaluado > 0 ? fmtDet(asi.acumulado) + ' de ' + fmtDet(asi.evaluado) : '—'}</td>
                ${conObservaciones ? '<td></td>' : ''}
             </tr>`;

    if (asi.registros > 0) {
        html += `<tr><td colspan="${columnas}" class="small text-muted ps-4">${asi.registros - asi.ausencias} de ${asi.registros} clases con asistencia (${asi.ausencias} ${asi.ausencias === 1 ? 'ausencia' : 'ausencias'}).</td></tr>`;
    }

    html += '</tbody></table></div>';

    return html;
}
