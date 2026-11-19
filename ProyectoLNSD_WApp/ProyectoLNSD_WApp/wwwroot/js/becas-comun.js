// Utilidades compartidas por las pantallas de becas (MBEF-01).
// escapeHtml, escapeAttr, fechaCorta y formatoMonto están en site.js.

const CLASES_ESTADO_BECA = {
    Activa: 'success',
    Vencida: 'secondary',
    Pendiente: 'warning',
    Inactiva: 'danger'
};

const CLASES_VIGENCIA = {
    'Vigente': 'success',
    'Por iniciar': 'info',
    'Vencida': 'secondary'
};

function badgeEstadoBeca(estado) {
    const clase = CLASES_ESTADO_BECA[estado] || 'secondary';
    return `<span class="badge text-bg-${clase}">${escapeHtml(estado)}</span>`;
}

function badgeVigencia(vigencia) {
    const clase = CLASES_VIGENCIA[vigencia] || 'secondary';
    return `<span class="badge rounded-pill text-bg-${clase} bg-opacity-75">${escapeHtml(vigencia)}</span>`;
}

function rangoFechas(inicio, fin) {
    return `${fechaCorta(inicio)} al ${fechaCorta(fin)}`;
}
