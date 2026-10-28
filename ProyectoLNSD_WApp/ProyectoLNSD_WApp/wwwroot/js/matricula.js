(() => {
    'use strict';
    const api = '/api/matriculas';
    const $ = id => document.getElementById(id);
    const puedeCrear = $('puedeCrearMatricula')?.value === 'true';
    const puedeEditar = $('puedeEditarMatricula')?.value === 'true';
    const modalFormEl = $('modalMatricula');
    const modalDetalleEl = $('modalDetalle');
    const modalForm = modalFormEl && window.bootstrap ? bootstrap.Modal.getOrCreateInstance(modalFormEl) : null;
    const modalDetalle = modalDetalleEl && window.bootstrap ? bootstrap.Modal.getOrCreateInstance(modalDetalleEl) : null;
    const modalTrasladoEl = $('modalTraslado');
    const modalTraslado = modalTrasladoEl && window.bootstrap ? bootstrap.Modal.getOrCreateInstance(modalTrasladoEl) : null;
    const modalAccionEl = $('modalAccionMatricula');
    const modalAccion = modalAccionEl && window.bootstrap ? bootstrap.Modal.getOrCreateInstance(modalAccionEl) : null;
    const toastEl = $('toastMatricula');
    const toast = toastEl && window.bootstrap ? bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 4500 }) : null;
    let catalogos = { estudiantes: [], grados: [], secciones: [], grupos: [], cursos: [] };
    let periodosCache = [];

    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const showAlert = (message, ok = false) => {
        const el = $('alertaMatricula');
        if (!el) return;
        el.className = `alert ${ok ? 'alert-success' : 'alert-danger'}`;
        el.textContent = message || (ok ? 'Operación realizada.' : 'Ocurrió un error.');
        el.classList.remove('d-none');
    };
    const clearAlert = () => $('alertaMatricula')?.classList.add('d-none');
    function notify(message, ok = true) {
        const el = $('toastMatricula');
        if (!el || !window.bootstrap) { showAlert(message, ok); return; }
        el.classList.remove('text-bg-success', 'text-bg-danger', 'text-bg-warning');
        el.classList.add(ok ? 'text-bg-success' : 'text-bg-danger');
        $('toastMatriculaMensaje').textContent = message || (ok ? 'Operación realizada.' : 'Ocurrió un error.');
        toast?.show();
    }
    function openAction(type, id) {
        $('formAccionMatricula').reset();
        $('motivoAccionMatricula').required = false;
        $('textoObservacionMatricula').required = false;
        $('guardarAccionMatricula').className = 'btn btn-primary';
        $('accionIdMatricula').value = String(id);
        $('tipoAccionMatricula').value = type;
        $('errorAccionMatricula').classList.add('d-none');
        ['campoEstadoMatricula','campoMotivoMatricula','campoObservacionMatricula','avisoAnulacionMatricula'].forEach(k => $(k).classList.add('d-none'));
        const title = $('tituloAccionMatricula'), save = $('guardarAccionMatricula'), head = $('cabeceraAccionMatricula');
        if (type === 'estado') {
            title.textContent = 'Cambiar estado de matrícula'; save.textContent = 'Actualizar estado';
            head.className = 'modal-header'; $('campoEstadoMatricula').classList.remove('d-none');
            $('campoMotivoMatricula').classList.remove('d-none'); $('motivoAccionMatricula').required = false;
            document.querySelector('label[for="motivoAccionMatricula"]').textContent = 'Motivo o detalle (opcional)'; $('motivoAccionMatricula').placeholder = 'Detalle del cambio de estado, si corresponde';
        } else if (type === 'anular') {
            title.textContent = 'Anular matrícula'; save.textContent = 'Confirmar anulación'; save.className = 'btn btn-danger';
            head.className = 'modal-header'; $('campoMotivoMatricula').classList.remove('d-none');
            document.querySelector('label[for="motivoAccionMatricula"]').textContent = 'Motivo de anulación *';
            $('motivoAccionMatricula').required = true; $('motivoAccionMatricula').placeholder = 'Indique por qué se anula esta matrícula';
            $('avisoAnulacionMatricula').classList.remove('d-none');
        } else {
            title.textContent = 'Agregar observación'; save.textContent = 'Guardar observación'; save.className = 'btn btn-primary';
            head.className = 'modal-header'; $('campoObservacionMatricula').classList.remove('d-none'); $('textoObservacionMatricula').required = true;
        }
        modalAccion?.show();
    }

    async function request(url, options = {}) {
        // El token antiforgery viaja en el encabezado X-CSRF-TOKEN (lo provee el formulario de cierre de sesión del layout).
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const metodo = (options.method || 'GET').toUpperCase();
        const response = await fetch(url, { credentials: 'same-origin', ...options, headers: { ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...(metodo !== 'GET' && token ? { 'X-CSRF-TOKEN': token } : {}), ...(options.headers || {}) } });
        let data;
        try { data = await response.json(); } catch { throw new Error(`Respuesta inesperada del servidor (${response.status}).`); }
        if (!response.ok || data?.esCorrecto === false) throw new Error(data?.mensaje || `Error ${response.status}.`);
        return data?.dato ?? data;
    }

    function queryFromFilters() {
        const map = { IdEstudiante: 'fIdEstudiante', Identificacion: 'fIdentificacion', IdGrado: 'fIdGrado', IdGrupo: 'fIdGrupo', IdSeccion: 'fIdSeccion', IdPeriodo: 'fIdPeriodo', Estado: 'fEstado', Texto: 'fTexto', Limite: 'fLimite' };
        const params = new URLSearchParams();
        Object.entries(map).forEach(([key, id]) => { const v = $(id)?.value?.trim(); if (v) params.set(key, v); });
        return params.toString();
    }

    function actionButton(label, action, id, css = 'btn-outline-primary') {
        return `<button type="button" class="btn btn-sm ${css} me-1 mb-1" data-action="${action}" data-id="${id}">${label}</button>`;
    }

    function renderRows(rows) {
        const tbody = document.querySelector('#tblMatriculas tbody');
        if (!tbody) return;
        if (!rows?.length) { tbody.innerHTML = '<tr><td colspan="11" class="text-center text-muted">No se encontraron matrículas con esos filtros.</td></tr>'; $('lblConteo').textContent = '0 matrículas'; return; }
        tbody.innerHTML = rows.map(m => {
            const cursos = (m.cursos || []).map(c => c.nombre).join(', ') || 'Sin cursos';
            const estadoClass = m.estado === 'Activa' ? 'success' : m.estado === 'Anulada' ? 'danger' : m.estado === 'Pendiente' ? 'warning text-dark' : 'secondary';
            let actions = actionButton('Detalle', 'detalle', m.idMatricula);
            actions += actionButton('Historial', 'historial', m.idMatricula, 'btn-outline-secondary');
            actions += actionButton('Observaciones', 'observaciones', m.idMatricula, 'btn-outline-secondary');
            if (puedeEditar && m.estado !== 'Anulada') {
                actions += actionButton('Editar', 'editar', m.idMatricula, 'btn-outline-primary');
                actions += actionButton('Estado', 'estado', m.idMatricula, 'btn-outline-warning');
                actions += actionButton('Trasladar', 'trasladar', m.idMatricula, 'btn-outline-info');
                actions += actionButton('Anular', 'anular', m.idMatricula, 'btn-outline-danger');
            }
            return `<tr><td>${m.idMatricula}</td><td>${escapeHtml(m.nombreEstudiante)}</td><td>${escapeHtml(m.identificacion)}</td><td>${escapeHtml(m.nombrePeriodo)}</td><td>${escapeHtml(m.nombreGrado)}</td><td>${escapeHtml(m.nombreGrupo)}</td><td>${escapeHtml(m.nombreSeccion)}</td><td title="${escapeHtml(cursos)}">${escapeHtml(cursos)}</td><td><span class="badge text-bg-${estadoClass}">${escapeHtml(m.estado)}</span></td><td>${escapeHtml(m.fechaMatricula)}</td><td class="text-nowrap">${actions}</td></tr>`;
        }).join('');
        $('lblConteo').textContent = `${rows.length} matrícula(s) encontradas`;
    }

    async function loadPeriods() {
        const selectFilter = $('fIdPeriodo');
        const selectForm = $('mIdPeriodo');
        if (!selectFilter || !selectForm) return;
        try {
            const periods = await request(`${api}/periodos`);
            const list = Array.isArray(periods) ? periods : [];
            periodosCache = list;
            const previousFilter = selectFilter.value;
            const previousForm = selectForm.value;
            selectFilter.innerHTML = '<option value="">Todos los períodos</option>' + list.map(p =>
                `<option value="${Number(p.idPeriodo)}">${escapeHtml(p.nombre)}${p.activo ? ' (Activo)' : ' (Inactivo)'}</option>`).join('');
            selectForm.innerHTML = '<option value="">Seleccione un período lectivo</option>' + list.map(p =>
                `<option value="${Number(p.idPeriodo)}" ${p.activo ? '' : 'disabled'}>${escapeHtml(p.nombre)}${p.activo ? ' — Activo' : ' — Inactivo'}</option>`).join('');
            if ([...selectFilter.options].some(o => o.value === previousFilter)) selectFilter.value = previousFilter;
            if ([...selectForm.options].some(o => o.value === previousForm && !o.disabled)) selectForm.value = previousForm;
            const activos = list.filter(p => p.activo);
            $('ayudaPeriodos').textContent = activos.length
                ? `Períodos activos disponibles: ${activos.map(p => p.nombre).join(', ')}. El año del grupo debe coincidir con el año de inicio del período.`
                : 'No hay períodos lectivos activos. Vaya a Configuración > Períodos lectivos para crear uno o activar uno existente.';
            const save = $('formMatricula')?.querySelector('button[type="submit"]');
            if (save) save.disabled = activos.length === 0;
            return activos.length > 0;
        } catch (e) {
            selectForm.innerHTML = '<option value="">No se pudieron cargar los períodos</option>';
            $('ayudaPeriodos').textContent = 'No se pudieron consultar los períodos. Verifique permisos de Matrícula y vuelva a cargar la página.';
            return false;
        }
    }

    function setOptions(select, placeholder, items, valueOf, labelOf, selectedValue = '') {
        if (!select) return;
        select.innerHTML = `<option value="">${escapeHtml(placeholder)}</option>` + items.map(item =>
            `<option value="${Number(valueOf(item))}" ${String(valueOf(item)) === String(selectedValue) ? 'selected' : ''}>${escapeHtml(labelOf(item))}</option>`).join('');
    }

    function refreshFilterSections() {
        const gradeId = $('fIdGrado')?.value || '';
        const items = catalogos.secciones.filter(s => !gradeId || String(s.idGrado) === gradeId);
        setOptions($('fIdSeccion'), 'Todas las secciones', items, s => s.idSeccion, s => `${s.nombreGrado} — ${s.nombre}`);
    }

    function refreshFormSections(selected = '') {
        const gradeId = $('mIdSeccion')?.dataset.gradeId || '';
        // No se requiere elegir grado por separado: se muestran las secciones activas con su grado.
        const items = catalogos.secciones;
        setOptions($('mIdSeccion'), 'Seleccione una sección', items, s => s.idSeccion, s => `${s.nombreGrado} — ${s.nombre}`, selected);
    }

    function refreshGroups(selected = '') {
        const periodId = $('mIdPeriodo')?.value || '';
        const periodo = periodosCache.find(p => String(p.idPeriodo) === String(periodId));
        const section = catalogos.secciones.find(s => String(s.idSeccion) === String($('mIdSeccion')?.value || selectedSectionId || ''));
        let items = catalogos.grupos;
        if (periodo?.fechaInicio) {
            const year = Number(String(periodo.fechaInicio).slice(0, 4));
            if (year) items = items.filter(g => Number(g.anio) === year);
        } else if (periodo?.nombre) {
            const year = Number(String(periodo.nombre).match(/20\d{2}/)?.[0]);
            if (year) items = items.filter(g => Number(g.anio) === year);
        }
        if (section?.nivel) items = items.filter(g => String(g.nivel).toLocaleLowerCase() === String(section.nivel).toLocaleLowerCase());
        setOptions($('mIdGrupo'), items.length ? 'Seleccione un grupo' : 'No hay grupos compatibles con período/sección', items, g => g.idGrupo, g => `${g.nombre} — ${g.nivel} — ${g.anio}`, selected);
    }

    let selectedSectionId = '';
    function refreshCourses(selectedIds = []) {
        const sectionId = $('mIdSeccion')?.value || selectedSectionId;
        const section = catalogos.secciones.find(s => String(s.idSeccion) === String(sectionId));
        const items = catalogos.cursos.filter(c => !section || (c.idsGrados || []).some(id => Number(id) === Number(section.idGrado)));
        const select = $('mCursos');
        if (!select) return;
        select.innerHTML = items.map(c => `<option value="${Number(c.idCurso)}" ${(selectedIds || []).map(Number).includes(Number(c.idCurso)) ? 'selected' : ''}>${escapeHtml(c.codigo)} — ${escapeHtml(c.nombre)}</option>`).join('');
        if (!items.length) select.innerHTML = '<option value="" disabled>No hay cursos activos asignados a este grado</option>';
    }

    async function loadCatalogs() {
        try {
            catalogos = await request(`${api}/catalogos`);
            catalogos.estudiantes ||= []; catalogos.grados ||= []; catalogos.secciones ||= []; catalogos.grupos ||= []; catalogos.cursos ||= [];
            setOptions($('mIdEstudiante'), 'Seleccione un estudiante', catalogos.estudiantes, x => x.idEstudiante, x => `${x.nombreCompleto} — ${x.identificacion} — ${x.carnet}`);
            setOptions($('fIdEstudiante'), 'Todos los estudiantes', catalogos.estudiantes, x => x.idEstudiante, x => `${x.nombreCompleto} — ${x.identificacion}`);
            setOptions($('fIdGrado'), 'Todos los grados', catalogos.grados, x => x.idGrado, x => `${x.nombre} (${x.nivel})`);
            setOptions($('fIdGrupo'), 'Todos los grupos', catalogos.grupos, x => x.idGrupo, x => `${x.nombre} — ${x.nivel} — ${x.anio}`);
            refreshFilterSections();
            refreshFormSections();
            refreshGroups();
            refreshCourses();
        } catch (e) {
            showAlert(`No se pudieron cargar las opciones académicas: ${e.message}`);
        }
    }

    async function load() {
        clearAlert();
        const tbody = document.querySelector('#tblMatriculas tbody');
        if (tbody) tbody.innerHTML = '<tr><td colspan="11" class="text-center">Consultando...</td></tr>';
        try { const rows = await request(`${api}?${queryFromFilters()}`); renderRows(rows); }
        catch (e) { renderRows([]); showAlert(e.message); }
    }


    function openNew() {
        $('formMatricula').reset(); $('mIdMatricula').value = ''; $('tituloModal').textContent = 'Nueva matrícula';
        selectedSectionId = ''; refreshFormSections(); refreshGroups(); refreshCourses();
        $('grupoEstudiante').classList.remove('d-none'); $('mIdEstudiante').disabled = false;
        $('errorFormulario').classList.add('d-none'); modalForm?.show();
    }

    async function openEdit(id) {
        try {
            const m = await request(`${api}/${id}`);
            $('formMatricula').reset(); $('mIdMatricula').value = m.idMatricula; $('tituloModal').textContent = `Editar matrícula #${m.idMatricula}`;
            $('grupoEstudiante').classList.add('d-none'); $('mIdEstudiante').value = m.idEstudiante; $('mIdEstudiante').disabled = true;
            $('mIdPeriodo').value = m.idPeriodo; selectedSectionId = String(m.idSeccion); refreshFormSections(selectedSectionId); $('mIdSeccion').value = String(m.idSeccion); refreshGroups(String(m.idGrupo)); $('mIdGrupo').value = String(m.idGrupo);
            $('mFecha').value = m.fechaMatricula || ''; refreshCourses((m.cursos || []).map(c => c.idCurso)); $('mObservaciones').value = m.observaciones || '';
            $('errorFormulario').classList.add('d-none'); modalForm?.show();
        } catch (e) { showAlert(e.message); }
    }

    $('fIdGrado')?.addEventListener('change', refreshFilterSections);
    $('mIdPeriodo')?.addEventListener('change', () => refreshGroups());
    $('mIdSeccion')?.addEventListener('change', () => { selectedSectionId = $('mIdSeccion').value; refreshGroups(); refreshCourses(); });
    $('formFiltros')?.addEventListener('submit', e => { e.preventDefault(); load(); });
    $('btnLimpiar')?.addEventListener('click', () => { $('formFiltros').reset(); load(); });
    $('btnNueva')?.addEventListener('click', openNew);
    $('formMatricula')?.addEventListener('submit', async e => {
        e.preventDefault();
        const error = $('errorFormulario'); error.classList.add('d-none');
        try {
            const id = $('mIdMatricula').value;
            const payload = {
                IdPeriodo: Number($('mIdPeriodo').value), IdGrupo: Number($('mIdGrupo').value), IdSeccion: Number($('mIdSeccion').value),
                FechaMatricula: $('mFecha').value || null, IdCursos: [...$('mCursos').selectedOptions].filter(o => o.value !== '').map(o => Number(o.value)).filter(Number.isFinite), Observaciones: $('mObservaciones').value.trim() || null
            };
            let url = api, method = 'POST';
            if (id) { url += `/${id}`; method = 'PUT'; }
            else { payload.IdEstudiante = Number($('mIdEstudiante').value); }
            await request(url, { method, body: JSON.stringify(payload) });
            modalForm?.hide(); notify(id ? 'Matrícula actualizada correctamente.' : 'Matrícula registrada correctamente.', true); await load();
        } catch (err) { error.textContent = err.message; error.classList.remove('d-none'); }
    });

    document.querySelector('#tblMatriculas tbody')?.addEventListener('click', async e => {
        const button = e.target.closest('button[data-action]'); if (!button) return;
        const id = Number(button.dataset.id); const action = button.dataset.action;
        try {
            if (action === 'editar') return await openEdit(id);
            if (action === 'detalle') {
                const m = await request(`${api}/${id}`);
                $('contenidoDetalle').innerHTML = `<dl class="row"><dt class="col-sm-4">Estudiante</dt><dd class="col-sm-8">${escapeHtml(m.nombreEstudiante)} (${escapeHtml(m.carnet)})</dd><dt class="col-sm-4">Identificación</dt><dd class="col-sm-8">${escapeHtml(m.identificacion)}</dd><dt class="col-sm-4">Encargado principal</dt><dd class="col-sm-8">${escapeHtml(m.encargadoPrincipal)}</dd><dt class="col-sm-4">Período</dt><dd class="col-sm-8">${escapeHtml(m.nombrePeriodo)}</dd><dt class="col-sm-4">Grado / grupo / sección</dt><dd class="col-sm-8">${escapeHtml(m.nombreGrado)} / ${escapeHtml(m.nombreGrupo)} / ${escapeHtml(m.nombreSeccion)}</dd><dt class="col-sm-4">Estado</dt><dd class="col-sm-8">${escapeHtml(m.estado)}</dd><dt class="col-sm-4">Fecha</dt><dd class="col-sm-8">${escapeHtml(m.fechaMatricula)}</dd><dt class="col-sm-4">Cursos</dt><dd class="col-sm-8">${(m.cursos || []).map(c => escapeHtml(c.nombre)).join(', ') || 'Sin cursos'}</dd><dt class="col-sm-4">Observaciones generales</dt><dd class="col-sm-8">${escapeHtml(m.observaciones || '—')}</dd></dl>`;
                modalDetalle?.show(); return;
            }
            if (action === 'historial') {
                const cambios = await request(`${api}/${id}/historial-cambios`);
                const list = Array.isArray(cambios) ? cambios : [];
                $('contenidoDetalle').innerHTML = `<h6>Historial de cambios de matrícula #${id}</h6>${list.length ? `<div class="table-responsive"><table class="table table-sm"><thead><tr><th>Fecha</th><th>Cambio</th><th>Estado</th><th>Detalle</th></tr></thead><tbody>${list.map(h => `<tr><td>${escapeHtml(h.fechaCambio)}</td><td>${escapeHtml(h.tipoCambio)}</td><td>${escapeHtml(`${h.estadoAnterior || '—'} → ${h.estadoNuevo || '—'}`)}</td><td>${escapeHtml(h.detalle)}</td></tr>`).join('')}</tbody></table></div>` : '<p>No hay cambios registrados.</p>'}`;
                modalDetalle?.show(); return;
            }
            if (action === 'observaciones') {
                const list = await request(`${api}/${id}/observaciones`);
                const add = puedeEditar ? `<button class="btn btn-sm btn-primary" id="btnAgregarObservacion" data-id="${id}">Agregar observación</button>` : '';
                $('contenidoDetalle').innerHTML = `${add}<ul class="list-group mt-3">${(list || []).map(o => `<li class="list-group-item"><small class="text-muted">${escapeHtml(o.fechaRegistro)}</small><br>${escapeHtml(o.observacion)}</li>`).join('') || '<li class="list-group-item text-muted">Sin observaciones.</li>'}</ul>`;
                modalDetalle?.show(); return;
            }
            if (action === 'estado' || action === 'anular') { openAction(action, id); return; }
            if (action === 'trasladar') {
                const matricula = await request(`${api}/${id}`);
                $('tIdMatricula').value = String(id); $('tMotivo').value = ''; $('errorTraslado').classList.add('d-none');
                setOptions($('tIdSeccion'), 'Seleccione la sección destino', catalogos.secciones, x => x.idSeccion, x => `${x.nombreGrado} — ${x.nombre}`);
                const periodo = periodosCache.find(p => Number(p.idPeriodo) === Number(matricula.idPeriodo));
                const year = periodo?.fechaInicio ? Number(String(periodo.fechaInicio).slice(0, 4)) : Number(String(periodo?.nombre || '').match(/20\d{2}/)?.[0]);
                let gruposDestino = catalogos.grupos.filter(g => !year || Number(g.anio) === year);
                setOptions($('tIdGrupo'), 'Seleccione el grupo destino', gruposDestino, x => x.idGrupo, x => `${x.nombre} — ${x.nivel} — ${x.anio}`);
                $('tIdSeccion').onchange = () => {
                    const sec = catalogos.secciones.find(x => String(x.idSeccion) === $('tIdSeccion').value);
                    let gs = catalogos.grupos.filter(g => (!year || Number(g.anio) === year) && (!sec?.nivel || String(g.nivel).toLocaleLowerCase() === String(sec.nivel).toLocaleLowerCase()));
                    setOptions($('tIdGrupo'), gs.length ? 'Seleccione el grupo destino' : 'No hay grupos compatibles', gs, x => x.idGrupo, x => `${x.nombre} — ${x.nivel} — ${x.anio}`);
                };
                modalTraslado?.show(); return;
            }
        } catch (err) { showAlert(err.message); }
    });

    $('formTraslado')?.addEventListener('submit', async e => {
        e.preventDefault();
        const error = $('errorTraslado'); error.classList.add('d-none');
        const id = Number($('tIdMatricula').value), grupo = Number($('tIdGrupo').value), seccion = Number($('tIdSeccion').value), motivo = $('tMotivo').value.trim();
        if (!id || !grupo || !seccion || !motivo) { error.textContent = 'Seleccione una sección, un grupo compatible e indique el motivo.'; error.classList.remove('d-none'); return; }
        try {
            await request(`${api}/${id}/traslados`, { method: 'POST', body: JSON.stringify({ IdGrupo: grupo, IdSeccion: seccion, Motivo: motivo }) });
            modalTraslado?.hide(); notify('Traslado registrado correctamente.', true); await load();
        } catch (err) { error.textContent = err.message; error.classList.remove('d-none'); }
    });

    modalDetalleEl?.addEventListener('click', async e => {
        const btn = e.target.closest('#btnAgregarObservacion'); if (!btn) return;
        openAction('observacion', Number(btn.dataset.id));
    });

    $('formAccionMatricula')?.addEventListener('submit', async e => {
        e.preventDefault();
        const id = Number($('accionIdMatricula').value), type = $('tipoAccionMatricula').value;
        const error = $('errorAccionMatricula'); error.classList.add('d-none');
        let url, method, payload, success;
        if (type === 'estado') {
            url = `${api}/${id}/estado`; method = 'PATCH';
            payload = { Estado: $('nuevoEstadoMatricula').value, Motivo: $('motivoAccionMatricula').value.trim() || null };
            success = 'Estado de matrícula actualizado correctamente.';
        } else if (type === 'anular') {
            const motivo = $('motivoAccionMatricula').value.trim();
            if (!motivo) { error.textContent = 'Indique el motivo de la anulación.'; error.classList.remove('d-none'); return; }
            url = `${api}/${id}/estado`; method = 'PATCH'; payload = { Estado: 'Anulada', Motivo: motivo };
            success = 'Matrícula anulada correctamente. Se conserva su historial y se podrá registrar otra matrícula para este período.';
        } else {
            const observacion = $('textoObservacionMatricula').value.trim();
            if (!observacion) { error.textContent = 'Escriba la observación antes de guardar.'; error.classList.remove('d-none'); return; }
            url = `${api}/${id}/observaciones`; method = 'POST'; payload = { Observacion: observacion };
            success = 'Observación registrada correctamente.';
        }
        const btn = $('guardarAccionMatricula'); btn.disabled = true;
        try {
            await request(url, { method, body: JSON.stringify(payload) });
            modalAccion?.hide();
            if (type === 'observacion') {
                const list = await request(`${api}/${id}/observaciones`);
                const add = puedeEditar ? `<button class="btn btn-sm btn-primary" id="btnAgregarObservacion" data-id="${id}">Agregar observación</button>` : '';
                $('contenidoDetalle').innerHTML = `${add}<ul class="list-group mt-3">${(list || []).map(o => `<li class="list-group-item"><small class="text-muted">${escapeHtml(o.fechaRegistro)}</small><br>${escapeHtml(o.observacion)}</li>`).join('') || '<li class="list-group-item text-muted">Sin observaciones.</li>'}</ul>`;
            } else { await load(); }
            notify(success, true);
        } catch (err) { error.textContent = err.message; error.classList.remove('d-none'); }
        finally { btn.disabled = false; }
    });

    $('btnReporte')?.addEventListener('click', async () => {
        try {
            const r = await request(`${api}/reporte?${queryFromFilters()}`);
            $('contenidoDetalle').innerHTML = `<h5>Reporte de matrículas</h5><div class="row g-3">${[['Total',r.total],['Activas',r.activas],['Inactivas',r.inactivas],['Pendientes',r.pendientes],['Anuladas',r.anuladas]].map(([label,n]) => `<div class="col-6 col-md-4"><div class="card card-body"><small>${label}</small><strong class="fs-4">${n}</strong></div></div>`).join('')}</div>`;
            modalDetalle?.show();
        } catch (err) { showAlert(err.message); }
    });

    loadPeriods().then(async hayActivos => {
        await loadCatalogs();
        await load();
        if (!hayActivos) showAlert('No hay períodos lectivos activos o no se pudieron cargar. Vaya a Configuración > Períodos lectivos para crear uno o activar uno existente.');
    });
})();
