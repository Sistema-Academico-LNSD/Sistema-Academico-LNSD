// Grupos académicos A/B (antes era un <script> dentro de la vista).
(function () {
    const alerta = document.getElementById('alertaGrupos');
    const tabla = document.getElementById('tablaGrupos');
    const filtroAnio = document.getElementById('fAnio');

    function mostrarAlerta(mensaje, tipo) {
        alerta.className = 'alert alert-' + tipo;
        alerta.textContent = mensaje;
    }

    function ocultarAlerta() {
        alerta.className = 'alert d-none';
        alerta.textContent = '';
    }

    async function cargarNiveles() {
        const select = document.getElementById('nivelGrupo');
        if (!select) return;   // sin permiso de crear no existe el modal

        const r = await fetch('/Grupos/Niveles');
        const j = await r.json();

        if (!j.esCorrecto || !Array.isArray(j.dato) || j.dato.length === 0) {
            select.innerHTML = '<option value="">Primero cree un grado activo</option>';
            return;
        }

        select.innerHTML = '<option value="">Seleccione nivel</option>' +
            j.dato.map(n => `<option value="${escapeHtml(n)}">${escapeHtml(n)}</option>`).join('');
    }

    async function cargarGrupos() {
        ocultarAlerta();
        tabla.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Cargando...</td></tr>';

        try {
            const q = filtroAnio.value ? '?anio=' + encodeURIComponent(filtroAnio.value) : '';
            const r = await fetch('/Grupos/Listar' + q);
            const j = await r.json();

            if (!r.ok || !j.esCorrecto) throw new Error(j.mensaje || 'No se pudieron cargar los grupos.');

            const grupos = j.dato || [];

            if (!grupos.length) {
                tabla.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No hay grupos registrados para este filtro. Use “Crear grupo” para agregar A y B.</td></tr>';
            } else {
                tabla.innerHTML = grupos.map(g => `
                    <tr>
                        <td><strong>${escapeHtml(g.nombre)}</strong></td>
                        <td>${escapeHtml(g.nivel)}</td>
                        <td>${Number(g.anio)}</td>
                        <td>Subgrupo para las materias que lo requieran</td>
                    </tr>`).join('');
            }

            const anios = [...new Set(grupos.map(g => g.anio))].sort((a, b) => b - a);
            const actual = filtroAnio.value;
            filtroAnio.innerHTML = '<option value="">Todos los años</option>' +
                anios.map(a => `<option value="${Number(a)}">${Number(a)}</option>`).join('');
            if (actual && anios.includes(Number(actual))) filtroAnio.value = actual;
        } catch (e) {
            tabla.innerHTML = '<tr><td colspan="4" class="text-center text-danger">No fue posible cargar los grupos.</td></tr>';
            mostrarAlerta(e.message, 'danger');
        }
    }

    filtroAnio.addEventListener('change', cargarGrupos);
    document.getElementById('btnActualizar').addEventListener('click', cargarGrupos);

    const form = document.getElementById('formGrupo');

    if (form) {
        form.addEventListener('submit', async function (ev) {
            ev.preventDefault();

            const err = document.getElementById('errorGrupo');
            err.classList.add('d-none');

            const data = new URLSearchParams(new FormData(form));

            try {
                const r = await fetch('/Grupos/Crear', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
                    body: data.toString()
                });
                const j = await r.json();

                if (!r.ok || !j.esCorrecto) throw new Error(j.mensaje || 'No se pudo crear el grupo.');

                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalGrupo')).hide();
                form.reset();
                document.getElementById('anioGrupo').value = new Date().getFullYear();

                mostrarAlerta(j.mensaje || 'Grupo creado correctamente.', 'success');
                await cargarGrupos();
            } catch (e) {
                err.textContent = e.message;
                err.classList.remove('d-none');
            }
        });
    }

    cargarNiveles();
    cargarGrupos();
})();
