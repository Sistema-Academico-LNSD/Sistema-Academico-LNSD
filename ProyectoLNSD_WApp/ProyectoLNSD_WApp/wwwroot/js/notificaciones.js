(() => {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const lista = document.getElementById('listaNotificaciones');
    const estado = document.getElementById('estadoNotificaciones');
    const contador = document.getElementById('contadorNoLeidas');

    const fecha = valor => new Date(valor).toLocaleString('es-CR', { dateStyle: 'medium', timeStyle: 'short' });
    const escapeHtml = valor => String(valor ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

    async function cargar() {
        try {
            const r = await fetch('/Notificacion/ObtenerPanel');
            if (!r.ok) throw new Error('No fue posible consultar sus notificaciones.');
            const data = await r.json();
            contador.textContent = `${data.noLeidas} ${data.noLeidas === 1 ? 'pendiente' : 'pendientes'}`;
            estado.classList.toggle('d-none', data.notificaciones.length > 0);
            estado.textContent = data.notificaciones.length ? '' : 'No tiene notificaciones registradas.';
            lista.innerHTML = data.notificaciones.map(n => `
                <article class="notificacion-item ${n.leida ? 'leida' : 'no-leida'}">
                    <div class="notificacion-indicador" aria-hidden="true"></div>
                    <div class="flex-grow-1 min-w-0">
                        <div class="d-flex flex-wrap justify-content-between gap-2">
                            <h3 class="h6 mb-1">${escapeHtml(n.titulo)}</h3>
                            <small class="text-muted">${fecha(n.fechaCreacion)}</small>
                        </div>
                        <p class="mb-2">${escapeHtml(n.mensaje)}</p>
                        <div class="d-flex align-items-center gap-2">
                            <span class="badge text-bg-light">${escapeHtml(n.tipo)}</span>
                            <span class="small text-muted">${n.leida ? 'Leída' : 'No leída'}</span>
                            ${n.referencia ? `<a class="small ms-auto" href="${escapeHtml(n.referencia)}">Abrir información</a>` : ''}
                            ${!n.leida ? `<button class="btn btn-sm btn-outline-primary" data-leer="${n.idNotificacion}">Marcar como leída</button>` : ''}
                        </div>
                    </div>
                </article>`).join('');
        } catch (e) {
            estado.classList.remove('d-none');
            estado.textContent = e.message;
        }
    }

    lista.addEventListener('click', async e => {
        const btn = e.target.closest('[data-leer]');
        if (!btn) return;
        btn.disabled = true;
        const body = new URLSearchParams({ idNotificacion: btn.dataset.leer });
        const r = await fetch('/Notificacion/MarcarLeida', {
            method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-CSRF-TOKEN': token }, body
        });
        if (r.ok) await cargar();
        else { btn.disabled = false; estado.classList.remove('d-none'); estado.textContent = 'No fue posible actualizar la notificación. Inténtelo nuevamente.'; }
    });

    cargar();
})();
