(() => {
    'use strict';
    const $ = id => document.getElementById(id);
    const puedeGestionar = $('puedeGestionarInfo')?.value === 'true';
    let tipoActual = 'Todos';
    let mesVisible = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    let registrosActuales = [];
    let modalEdicion = null, modalDetalle = null;
    const token = () => $('tokenInfoForm')?.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
    const fecha = value => value ? new Date(value).toLocaleDateString('es-CR', { year:'numeric', month:'long', day:'numeric' }) : '—';
    const fechaHora = value => value ? new Date(value).toLocaleString('es-CR') : '—';
    const alertar = (mensaje, tipo='success') => { const a=$('alertaInfo'); a.className=`alert alert-${tipo}`; a.textContent=mensaje; a.classList.remove('d-none'); window.scrollTo({top:0,behavior:'smooth'}); };
    const request = async (url, options={}) => {
        const response = await fetch(url, { credentials: 'same-origin', ...options });
        const texto = await response.text();
        let data = null;
        if (texto) {
            try { data = JSON.parse(texto); }
            catch {
                if (response.status === 400) throw new Error('El servidor rechazó la solicitud (HTTP 400). Verifique el token de seguridad y los datos enviados.');
                if (response.status === 401) throw new Error('La sesión expiró. Inicie sesión nuevamente.');
                if (response.status === 403) throw new Error('No tiene permisos para realizar esta operación.');
                throw new Error(`El servidor devolvió una respuesta inesperada (HTTP ${response.status}). Revise el registro de errores de la aplicación.`);
            }
        }
        if (!response.ok) throw new Error(data?.mensaje || `La operación no pudo completarse (HTTP ${response.status}).`);
        return data || {};
    };
    const fechaInput = d => { const x=new Date(d); return `${x.getFullYear()}-${String(x.getMonth()+1).padStart(2,'0')}-${String(x.getDate()).padStart(2,'0')}`; };

    async function cargar() {
        const params = new URLSearchParams({tipo:tipoActual});
        if (tipoActual === 'Actividad') {
            params.set('desde', fechaInput(mesVisible));
            params.set('hasta', fechaInput(new Date(mesVisible.getFullYear(), mesVisible.getMonth()+1, 0)));
        } else {
            if ($('fDesde')?.value) params.set('desde',$('fDesde').value);
            if ($('fHasta')?.value) params.set('hasta',$('fHasta').value);
        }
        const data = await request(`/Informativo/Listar?${params}`);
        registrosActuales = data.dato || [];
        const cont=$('listaInformativo');
        if (tipoActual === 'Actividad') { $('calendarioWrap').classList.remove('d-none'); cont.classList.add('d-none'); renderCalendario(); return; }
        $('calendarioWrap').classList.add('d-none'); cont.classList.remove('d-none');
        if (!data.dato?.length) { cont.innerHTML='<div class="col-12"><div class="alert alert-light border text-center mb-0">No hay información disponible para los filtros seleccionados.</div></div>'; return; }
        cont.innerHTML = data.dato.map(x => {
            const esAnuncio=x.tipo==='Anuncio', esEvento=x.tipo==='Evento';
            const color=esAnuncio?'primary':esEvento?'success':'info';
            const fechaTexto=esAnuncio?`Publicado: ${fechaHora(x.fechaPublicacion)} · Vigencia: ${fecha(x.fechaVigencia)}`:`${fecha(x.fechaInicio)}${x.fechaFin && x.fechaFin!==x.fechaInicio ? ` – ${fecha(x.fechaFin)}`:''}`;
            const extra=[x.categoria,x.horaInicio?String(x.horaInicio).substring(0,5):null,x.ubicacion].filter(Boolean).join(' · ');
            return `<div class="col-12 col-lg-6"><article class="card h-100 border-0 shadow-sm"><div class="card-body"><div class="d-flex justify-content-between gap-2 mb-2"><span class="badge text-bg-${color}">${escapeHtml(esAnuncio?'Anuncio':esEvento?'Evento':'Calendario')}</span>${puedeGestionar?`<span class="badge text-bg-${x.estado==='Publicado'?'success':x.estado==='Programado'?'warning':'secondary'}">${escapeHtml(x.estado)}</span>`:''}</div><h5 class="card-title">${escapeHtml(x.titulo)}</h5><p class="card-text text-muted">${escapeHtml(x.descripcion.length>210?x.descripcion.substring(0,207)+'…':x.descripcion)}</p><div class="small text-secondary mb-3"><div><i class="bi bi-calendar3"></i> ${escapeHtml(fechaTexto)}</div>${extra?`<div class="mt-1"><i class="bi bi-info-circle"></i> ${escapeHtml(extra)}</div>`:''}</div><div class="d-flex gap-2 flex-wrap"><button class="btn btn-sm btn-outline-primary" data-detalle="${x.idInformativo}">Ver detalle</button>${puedeGestionar?`<button class="btn btn-sm btn-outline-secondary" data-editar="${x.idInformativo}">Editar</button><button class="btn btn-sm btn-outline-danger" data-eliminar="${x.idInformativo}">Eliminar</button>`:''}</div></div></article></div>`;
        }).join('');
    }

    function fechaLocal(valor) {
        if (!valor) return null;
        const partes = String(valor).slice(0,10).split('-').map(Number);
        return new Date(partes[0], partes[1]-1, partes[2]);
    }
    function renderCalendario() {
        const titulo = $('tituloMes');
        titulo.textContent = mesVisible.toLocaleDateString('es-CR',{month:'long',year:'numeric'});
        const grilla = $('grillaCalendario');
        if (!registrosActuales.length) { grilla.innerHTML='<div class="col-12"><div class="alert alert-light border text-center">No hay fechas programadas para este mes.</div></div>'; return; }
        const dias = ['Lun','Mar','Mié','Jue','Vie','Sáb','Dom'];
        const primerDia = new Date(mesVisible.getFullYear(),mesVisible.getMonth(),1);
        const offset = (primerDia.getDay()+6)%7;
        const total = new Date(mesVisible.getFullYear(),mesVisible.getMonth()+1,0).getDate();
        let html = dias.map(d=>`<div class="col text-center small fw-bold text-secondary py-2">${d}</div>`).join('');
        for(let i=0;i<offset;i++) html += '<div class="col-12 col-sm" style="min-width:12%;max-width:14.285%"><div class="border rounded bg-light" style="min-height:112px"></div></div>';
        for(let dia=1;dia<=total;dia++) {
            const fechaDia = new Date(mesVisible.getFullYear(),mesVisible.getMonth(),dia);
            const eventos = registrosActuales.filter(x=>{const inicio=fechaLocal(x.fechaInicio), fin=fechaLocal(x.fechaFin)||inicio;return inicio && inicio<=fechaDia && fin>=fechaDia;});
            const chips = eventos.map(x=>`<button type="button" class="btn btn-sm w-100 text-start text-truncate mb-1 ${x.categoria==='Vacaciones'||x.categoria==='Día no lectivo'||x.categoria==='Feriado'?'btn-warning':'btn-outline-primary'}" data-detalle="${x.idInformativo}" title="${escapeHtml(x.titulo)}">${escapeHtml(x.titulo)}</button>`).join('');
            html += `<div class="col-12 col-sm" style="min-width:12%;max-width:14.285%"><div class="border rounded p-1 h-100" style="min-height:112px"><div class="small fw-semibold mb-1">${dia}</div>${chips||'<span class="text-muted small">—</span>'}</div></div>`;
        }
        const celdas=(offset+total)%7; if(celdas) for(let i=celdas;i<7;i++) html += '<div class="col-12 col-sm" style="min-width:12%;max-width:14.285%"><div class="border rounded bg-light" style="min-height:112px"></div></div>';
        grilla.innerHTML=html;
    }

    function cambiarCampos() {
        const tipo=$('tipoInfo').value, anuncio=tipo==='Anuncio', evento=tipo==='Evento', actividad=tipo==='Actividad';
        $('grupoVigencia').classList.toggle('d-none',!anuncio);
        ['grupoFechaInicio','grupoFechaFin','grupoHoraInicio','grupoHoraFin','grupoUbicacion'].forEach(id=>$(id).classList.toggle('d-none',anuncio));
        $('grupoCategoria').classList.toggle('d-none',!actividad);
        $('grupoRecurrencia').classList.toggle('d-none',!actividad || Number($('idInformativo').value)>0);
        $('grupoRepetirHasta').classList.toggle('d-none',!actividad || Number($('idInformativo').value)>0 || $('recurrenciaInfo').value==='Ninguna');
        $('fechaInicioInfo').required=!anuncio;
        $('ubicacionInfo').required=evento;
        $('horaInicioInfo').required=evento;
    }
    function limpiarFormulario() {
        $('formInfo').reset(); $('idInformativo').value='0';
        $('fechaPublicacionInfo').value=new Date(Date.now()-new Date().getTimezoneOffset()*60000).toISOString().slice(0,16);
        $('fechaInicioInfo').value=fechaInput(new Date()); $('fechaFinInfo').value=''; $('fechaVigenciaInfo').value='';
        $('tituloModalInfo').textContent='Nuevo registro'; cambiarCampos();
    }
    function payload() {
        return { idInformativo:Number($('idInformativo').value), tipo:$('tipoInfo').value, titulo:$('tituloInfo').value.trim(), descripcion:$('descripcionInfo').value.trim(),
            fechaPublicacion:$('fechaPublicacionInfo').value, fechaVigencia:$('fechaVigenciaInfo').value||null, fechaInicio:$('fechaInicioInfo').value,
            fechaFin:$('fechaFinInfo').value||null, horaInicio:$('horaInicioInfo').value||null, horaFin:$('horaFinInfo').value||null,
            ubicacion:$('ubicacionInfo').value.trim()||null, categoria:$('categoriaInfo').value||null, estado:$('estadoInfo').value,
            recurrencia:$('recurrenciaInfo').value, repetirHasta:$('repetirHastaInfo').value||null, confirmarDuplicado:false };
    }
    async function guardar(confirmar=false) {
        const dto=payload(); dto.confirmarDuplicado=confirmar;
        const data=await request('/Informativo/Guardar',{method:'POST',headers:{'Content-Type':'application/json','X-CSRF-TOKEN':token()},body:JSON.stringify(dto)});
        if (!data.esCorrecto && data.duplicado && !confirmar) { if (window.confirm(data.mensaje)) return guardar(true); return; }
        if (!data.esCorrecto) throw new Error(data.mensaje||'No se pudo guardar.');
        modalEdicion.hide(); alertar(data.mensaje); await cargar();
    }
    async function detalle(id, vistaPrevia=false) {
        const data=await request(`/Informativo/Obtener?id=${id}`); const x=data.dato;
        $('detalleInfo').innerHTML=`<h4>${escapeHtml(x.titulo)}</h4><p>${escapeHtml(x.descripcion)}</p><hr><dl class="row mb-0"><dt class="col-5">Tipo</dt><dd class="col-7">${escapeHtml(x.tipo)}</dd><dt class="col-5">Publicación</dt><dd class="col-7">${escapeHtml(fechaHora(x.fechaPublicacion))}</dd>${x.fechaVigencia?`<dt class="col-5">Vigencia</dt><dd class="col-7">${escapeHtml(fecha(x.fechaVigencia))}</dd>`:''}${x.fechaInicio?`<dt class="col-5">Fecha</dt><dd class="col-7">${escapeHtml(fecha(x.fechaInicio))}${x.fechaFin?` – ${escapeHtml(fecha(x.fechaFin))}`:''}</dd>`:''}${x.horaInicio?`<dt class="col-5">Horario</dt><dd class="col-7">${escapeHtml(String(x.horaInicio).substring(0,5))}${x.horaFin?` – ${escapeHtml(String(x.horaFin).substring(0,5))}`:''}</dd>`:''}${x.ubicacion?`<dt class="col-5">Ubicación</dt><dd class="col-7">${escapeHtml(x.ubicacion)}</dd>`:''}${x.categoria?`<dt class="col-5">Categoría</dt><dd class="col-7">${escapeHtml(x.categoria)}</dd>`:''}</dl>`;
        if (!vistaPrevia) modalDetalle.show();
    }
    async function editar(id) {
        const data=await request(`/Informativo/Obtener?id=${id}`), x=data.dato;
        limpiarFormulario(); $('idInformativo').value=x.idInformativo; $('tipoInfo').value=x.tipo; $('tituloInfo').value=x.titulo; $('descripcionInfo').value=x.descripcion;
        $('fechaPublicacionInfo').value=x.fechaPublicacion?.slice(0,16)||''; $('fechaVigenciaInfo').value=x.fechaVigencia?.slice(0,10)||'';
        $('fechaInicioInfo').value=x.fechaInicio?.slice(0,10)||''; $('fechaFinInfo').value=x.fechaFin?.slice(0,10)||'';
        $('horaInicioInfo').value=x.horaInicio?.substring(0,5)||''; $('horaFinInfo').value=x.horaFin?.substring(0,5)||'';
        $('ubicacionInfo').value=x.ubicacion||''; $('categoriaInfo').value=x.categoria||''; $('estadoInfo').value=x.estado==='Borrador'?'Borrador':'Publicado';
        $('tituloModalInfo').textContent='Editar registro'; cambiarCampos(); modalEdicion.show();
    }

    document.addEventListener('DOMContentLoaded', async () => {
        modalDetalle=new bootstrap.Modal($('modalDetalleInfo'));
        if (puedeGestionar) modalEdicion=new bootstrap.Modal($('modalInfo'));
        $('tabsInfo').addEventListener('click',async e=>{const b=e.target.closest('[data-tipo]');if(!b)return;tipoActual=b.dataset.tipo;$('tabsInfo').querySelectorAll('.nav-link').forEach(x=>x.classList.toggle('active',x===b));await cargar();});
        $('btnFiltrar').addEventListener('click',cargar);
        $('mesAnterior').addEventListener('click',()=>{mesVisible=new Date(mesVisible.getFullYear(),mesVisible.getMonth()-1,1);cargar();});
        $('mesSiguiente').addEventListener('click',()=>{mesVisible=new Date(mesVisible.getFullYear(),mesVisible.getMonth()+1,1);cargar();});
        $('listaInformativo').addEventListener('click',async e=>{try{const d=e.target.closest('[data-detalle]'), ed=e.target.closest('[data-editar]'), del=e.target.closest('[data-eliminar]');if(d)await detalle(d.dataset.detalle);if(ed)await editar(ed.dataset.editar);if(del&&window.confirm('¿Desea retirar este registro? Esta acción lo ocultará de las consultas públicas.')){const data=await request('/Informativo/Eliminar',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded','X-CSRF-TOKEN':token()},body:new URLSearchParams({id:del.dataset.eliminar})});alertar(data.mensaje);await cargar();}}catch(err){alertar(err.message,'danger');}});
        if(puedeGestionar){$('btnNuevo').addEventListener('click',()=>{limpiarFormulario();modalEdicion.show();});$('tipoInfo').addEventListener('change',cambiarCampos);$('recurrenciaInfo').addEventListener('change',cambiarCampos);$('formInfo').addEventListener('submit',async e=>{e.preventDefault();try{await guardar();}catch(err){alertar(err.message,'danger');}});$('btnVistaPrevia').addEventListener('click',()=>{const p=payload();if(!p.titulo||!p.descripcion){alertar('Complete título y descripción para generar la vista previa.','warning');return;}const preview={...p,idInformativo:0};$('detalleInfo').innerHTML=`<h4>${escapeHtml(preview.titulo)}</h4><p>${escapeHtml(preview.descripcion)}</p><p class="small text-muted">${escapeHtml(preview.tipo)} · Publicación: ${escapeHtml(fechaHora(preview.fechaPublicacion))}</p>`;modalDetalle.show();});}
        try{await cargar();}catch(err){alertar(err.message,'danger');}
    });
})();
