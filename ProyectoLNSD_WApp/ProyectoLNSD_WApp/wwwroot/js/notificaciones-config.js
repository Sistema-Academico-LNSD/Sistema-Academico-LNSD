(() => {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const contenedor = document.getElementById('tiposNotificacion');
    const estado = document.getElementById('estadoConfiguracion');
    const roles = ['Estudiante', 'Encargado', 'Docente', 'Administrador', 'Director', 'Personal administrativo'];
    const esc = v => String(v ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

    async function cargar() {
        try {
            const r = await fetch('/Notificacion/ObtenerTipos');
            if (!r.ok) throw new Error('No fue posible cargar la configuración.');
            const tipos = await r.json();
            estado.classList.add('d-none');
            contenedor.innerHTML = tipos.map(t => `
                <div class="col-12 col-xl-6">
                    <section class="card h-100 shadow-sm notificacion-tipo-card" data-id="${t.idTipo}">
                        <div class="card-body">
                            <div class="d-flex justify-content-between gap-3">
                                <div><h2 class="h6 mb-1">${esc(t.nombre)}</h2><small class="text-muted">${esc(t.modulo)} · ${esc(t.codigo)}</small></div>
                                <div class="form-check form-switch">
                                    <input class="form-check-input tipo-activo" type="checkbox" role="switch" ${t.activo ? 'checked' : ''} aria-label="Tipo activo">
                                    <label class="form-check-label">Activo</label>
                                </div>
                            </div>
                            <p class="small text-muted mt-2">${esc(t.descripcion)}</p>
                            <div class="small fw-semibold mb-2">Roles destinatarios</div>
                            <div class="d-flex flex-wrap gap-2">
                                ${roles.map(rol => `<label class="form-check form-check-inline small"><input class="form-check-input rol-destino" type="checkbox" value="${esc(rol)}" ${t.roles.includes(rol) ? 'checked' : ''}> ${esc(rol)}</label>`).join('')}
                            </div>
                            <button class="btn btn-primary btn-sm mt-3 guardar-tipo">Guardar configuración</button>
                        </div>
                    </section>
                </div>`).join('');
        } catch (e) { estado.textContent = e.message; }
    }

    contenedor.addEventListener('click', async e => {
        const card = e.target.closest('.notificacion-tipo-card');
        if (!card) return;
        const idTipo = Number(card.dataset.id);
        if (e.target.closest('.guardar-tipo')) {
            const payload = {
                idTipo,
                activo: card.querySelector('.tipo-activo').checked,
                roles: [...card.querySelectorAll('.rol-destino:checked')].map(x => x.value)
            };
            const r = await fetch('/Notificacion/GuardarTipo', {
                method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': token },
                body: JSON.stringify(payload)
            });
            const data = await r.json();
            estado.classList.remove('d-none');
            estado.textContent = data.mensaje || (r.ok ? 'Guardado.' : 'No fue posible guardar.');
            estado.className = `alert ${r.ok ? 'alert-success' : 'alert-danger'}`;
        }
    });

    cargar();
})();
