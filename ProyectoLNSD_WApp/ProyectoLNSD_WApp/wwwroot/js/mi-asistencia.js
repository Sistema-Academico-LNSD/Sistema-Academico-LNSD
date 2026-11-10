// Mi asistencia (MCAF-01-07) y asistencia de los estudiantes del encargado (MCAF-01-08).
// El servidor identifica a la persona por la sesión: solo llega información propia.

$(function () {
    cargarMiAsistencia(null);
    $('#maPeriodo').on('change', () => cargarMiAsistencia($('#maPeriodo').val()));
});

function cargarMiAsistencia(idPeriodo) {
    $('#maContenido').html('<p class="text-center text-muted">Cargando...</p>');

    $.get('/Asistencia/GetMiAsistencia', { idPeriodo: idPeriodo || null })
        .done(function (r) {
            if (!r.esCorrecto) {
                $('#maContenido').html(`<div class="alert alert-danger">${escapeHtml(r.mensaje)}</div>`);
                return;
            }

            const d = r.dato;

            llenarSelect('#maPeriodo', d.periodos, null, d.idPeriodo);
            $('#maPeriodo').prop('disabled', !d.periodos.length);

            if (!d.estudiantes.length) {
                $('#maContenido').html(`<div class="alert alert-info">${escapeHtml(r.mensaje)}</div>`);
                return;
            }

            $('#maContenido').html(
                (r.mensaje ? `<div class="alert alert-info">${escapeHtml(r.mensaje)}</div>` : '') +
                d.estudiantes.map(tarjetaEstudiante).join(''));
        })
        .fail(() => $('#maContenido').html(
            '<div class="alert alert-danger">No fue posible consultar la asistencia.</div>'));
}

function tarjetaEstudiante(e) {
    const cursos = e.cursos.length
        ? `<div class="table-responsive">
               <table class="table table-sm align-middle">
                   <thead>
                       <tr>
                           <th>Curso</th>
                           <th class="text-center">Presentes</th>
                           <th class="text-center">Ausentes</th>
                           <th class="text-center">Tardías</th>
                           <th class="text-center">Justificadas</th>
                           <th class="text-center">% asistencia</th>
                       </tr>
                   </thead>
                   <tbody>
                       ${e.cursos.map(c => `
                           <tr>
                               <td>${escapeHtml(c.nombreCurso)}</td>
                               <td class="text-center">${c.presentes}</td>
                               <td class="text-center">${c.ausentes}</td>
                               <td class="text-center">${c.tardias}</td>
                               <td class="text-center">${c.justificadas}</td>
                               <td class="text-center">${badgePorcentaje(c.porcentaje, c.bajoUmbral)}</td>
                           </tr>`).join('')}
                   </tbody>
               </table>
           </div>`
        : '<p class="text-muted">Todavía no hay asistencia registrada en este período.</p>';

    const incidencias = e.incidencias.length
        ? `<div class="table-responsive">
               <table class="table table-sm table-striped align-middle mb-0">
                   <thead>
                       <tr>
                           <th>Fecha</th>
                           <th>Curso</th>
                           <th>Clase</th>
                           <th>Estado</th>
                           <th>Observación</th>
                       </tr>
                   </thead>
                   <tbody>
                       ${e.incidencias.map(x => `
                           <tr>
                               <td>${fechaCorta(x.fecha)}</td>
                               <td>${escapeHtml(x.nombreCurso)}</td>
                               <td class="small">${escapeHtml(x.dia)} ${escapeHtml(x.horaInicio)}–${escapeHtml(x.horaFin)}</td>
                               <td>${badgeEstado(x.estado)}</td>
                               <td class="small">${escapeHtml(x.observacion || '')}</td>
                           </tr>`).join('')}
                   </tbody>
               </table>
           </div>`
        : '<p class="text-muted mb-0">Sin ausencias, tardías ni justificaciones registradas.</p>';

    return `
        <div class="card mb-4">
            <div class="card-header">
                <h2>${escapeHtml(e.nombreEstudiante)}</h2>
                <span class="small text-muted fw-normal">
                    Carné ${escapeHtml(e.carnet)} · ${escapeHtml(e.nombrePeriodo)} ·
                    ${escapeHtml(e.nombreGrado)} ${escapeHtml(e.nombreSeccion)} · Grupo ${escapeHtml(e.nombreGrupo)}
                </span>
            </div>
            <div class="card-body">
                <h3 class="section-heading"><i class="bi bi-percent"></i> Asistencia por curso</h3>
                ${cursos}
                <h3 class="section-heading mt-4"><i class="bi bi-exclamation-circle"></i> Ausencias, tardías y justificaciones</h3>
                ${incidencias}
            </div>
        </div>`;
}
