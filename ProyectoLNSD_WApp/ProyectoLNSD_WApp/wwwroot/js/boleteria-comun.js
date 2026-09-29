// El servidor guarda y devuelve la fecha en UTC pero sin la "Z" al final.
// Sin ella, el navegador la tomaría como hora local y mostraría 6 horas de diferencia.
function formatearFecha(valor) {
    if (!valor) {
        return '—';
    }

    let texto = /(Z|[+-]\d{2}:\d{2})$/i.test(valor) ? valor : valor + 'Z';

    return new Date(texto).toLocaleString('es-CR', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}

function badgeEstado(estado) {
    let clase = estado === 'Disponible' ? 'bg-success' : 'bg-secondary';

    return `<span class="badge ${clase}">${escapeHtml(estado)}</span>`;
}