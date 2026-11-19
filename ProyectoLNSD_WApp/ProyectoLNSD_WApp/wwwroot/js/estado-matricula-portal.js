(() => {
    'use strict';
    const ownPanel = document.getElementById('estadoMatriculaPropio');
    const guardianPanels = [...document.querySelectorAll('.estado-matricula-encargado[data-estudiante-id]')];
    if (!ownPanel && guardianPanels.length === 0) return;

    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const badgeClass = estado => ({ Activa: 'success', Inactiva: 'secondary', Pendiente: 'warning text-dark', Anulada: 'danger' }[estado] || 'secondary');

    async function request(url) {
        const response = await fetch(url, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
        let data;
        try { data = await response.json(); } catch { throw new Error('No fue posible leer la respuesta del servidor.'); }
        if (!response.ok || data?.esCorrecto === false) throw new Error(data?.mensaje || 'No fue posible consultar las matrículas.');
        return data?.dato ?? data;
    }

    function renderList(list, includeStudentName) {
        if (!list.length) return '<div class="alert alert-info mb-0">No hay matrículas registradas para este estudiante.</div>';
        const ordered = [...list].sort((a, b) => String(b.fechaMatricula || '').localeCompare(String(a.fechaMatricula || '')));
        return `<div class="table-responsive"><table class="table table-sm table-striped align-middle mb-0"><thead><tr>${includeStudentName ? '<th>Estudiante</th>' : ''}<th>Período</th><th>Grado / grupo / sección</th><th>Estado</th><th>Cursos</th></tr></thead><tbody>${ordered.map(m => `<tr>${includeStudentName ? `<td>${escapeHtml(m.nombreEstudiante)}</td>` : ''}<td>${escapeHtml(m.nombrePeriodo)}</td><td>${escapeHtml(`${m.nombreGrado || '—'} / ${m.nombreGrupo || '—'} / ${m.nombreSeccion || '—'}`)}</td><td><span class="badge text-bg-${badgeClass(m.estado)}">${escapeHtml(m.estado)}</span></td><td>${escapeHtml((m.cursos || []).map(c => c.nombre).join(', ') || 'Sin cursos')}</td></tr>`).join('')}</tbody></table></div>`;
    }

    async function initOwn() {
        try {
            const list = await request('/api/matriculas/mi-estado');
            ownPanel.innerHTML = renderList(Array.isArray(list) ? list : [], false);
        } catch (error) {
            ownPanel.innerHTML = `<div class="alert alert-warning mb-0">${escapeHtml(error.message)} Si considera que es un error, comuníquese con la administración.</div>`;
        }
    }

    async function initGuardian() {
        try {
            const list = await request('/api/matriculas/mis-estudiantes');
            const matriculas = Array.isArray(list) ? list : [];
            guardianPanels.forEach(panel => {
                const id = Number(panel.dataset.estudianteId);
                panel.innerHTML = renderList(matriculas.filter(m => Number(m.idEstudiante) === id), false);
            });
        } catch (error) {
            guardianPanels.forEach(panel => panel.innerHTML = `<div class="alert alert-warning mb-0">${escapeHtml(error.message)} Si considera que es un error, comuníquese con la administración.</div>`);
        }
    }

    if (ownPanel) initOwn();
    if (guardianPanels.length) initGuardian();
})();
