// Catálogo de plantillas (MPPAF-01-01).

$(function () {
    $.when($.get('/Pauta/GetPlantillas'), $.get('/Pauta/GetPautas'))
        .done(function (rp, rpautas) {
            const plantillas = rp[0].esCorrecto ? (rp[0].dato || []) : [];
            const pautas = rpautas[0].esCorrecto ? (rpautas[0].dato || []) : [];

            if (!plantillas.length) {
                $('#listaPlantillas').html(`<div class="col-12 text-danger">${escapeHtml(rp[0].mensaje)}</div>`);
                return;
            }

            $('#listaPlantillas').html(plantillas.map(p => `
                <div class="col-md-6 col-xl-4">
                    <div class="card h-100">
                        <div class="card-body">
                            <div class="d-flex align-items-start gap-2">
                                <span class="badge text-bg-primary">N°${Number(p.numero)}</span>
                                <div class="fw-semibold">${escapeHtml(p.nombre)}</div>
                            </div>
                            <div class="small text-muted mt-2">${escapeHtml(p.descripcion)}</div>
                            <div class="small mt-2">
                                <span class="text-muted">Se usa en las pautas:</span>
                                ${p.pautas.map(n => {
                                    const pauta = pautas.find(x => x.numero === n);
                                    return `<span class="badge text-bg-light border" title="${escapeAttr(pauta ? pauta.nombre : '')}">${Number(n)}</span>`;
                                }).join(' ')}
                            </div>
                            ${p.repetible ? '<div class="small text-muted mt-1"><i class="bi bi-files"></i> Se puede completar más de una vez por expediente.</div>' : ''}
                        </div>
                    </div>
                </div>`).join(''));
        })
        .fail(() => $('#listaPlantillas').html('<div class="col-12 text-danger">No fue posible cargar las plantillas.</div>'));
});
