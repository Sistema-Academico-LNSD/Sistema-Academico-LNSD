// Utilidades de Pautas de actuación (MPPAF-01): textos, estados y el selector de personas estudiantes.

const TEXTO_ROL = {
    Recibe: 'Recibe la violencia',
    Ejerce: 'Ejerce la violencia',
    Observador: 'Observador / testigo',
    Identificada: 'Persona estudiante identificada'
};

function textoRol(rol) {
    return TEXTO_ROL[rol] || rol;
}

function badgeEstado(estado) {
    const mapa = {
        Borrador: ['secondary', 'Borrador'],
        EnElaboracion: ['warning text-dark', 'En elaboración'],
        Finalizado: ['success', 'Finalizado'],
        Abierto: ['primary', 'Abierto'],
        Cerrado: ['dark', 'Cerrado']
    };
    const [clase, texto] = mapa[estado] || ['secondary', estado];
    return `<span class="badge text-bg-${clase}">${escapeHtml(texto)}</span>`;
}

// Los mensajes largos del servidor separan los puntos con " •": se muestran uno por línea.
function mensajeEnLineas(texto) {
    return String(texto || '').replace(/ •/g, '\n•');
}

/**
 * Selector de personas estudiantes: busca en las matriculadas o permite escribirlas a mano.
 * opciones.roles = roles que admite la pauta elegida.
 */
function crearSelectorEstudiantes($raiz, opciones) {
    let roles = (opciones && opciones.roles) || [];
    let lista = [];
    let temporizador = null;

    $raiz.html(`
        <div class="row g-2 mb-2">
            <div class="col-md-8 position-relative">
                <input type="search" class="form-control sel-buscar" maxlength="100" autocomplete="off"
                       placeholder="Buscar persona estudiante (nombre o identificación)..." />
                <div class="list-group position-absolute w-100 shadow sel-resultados d-none"
                     style="z-index:20;max-height:240px;overflow:auto"></div>
            </div>
            <div class="col-md-4 d-grid">
                <button type="button" class="btn btn-outline-secondary sel-manual">
                    <i class="bi bi-pencil"></i> Agregar manualmente
                </button>
            </div>
        </div>
        <div class="table-responsive">
            <table class="table table-sm align-middle mb-0">
                <thead>
                    <tr><th>Nombre completo</th><th>Sección</th><th>Rol en la situación</th><th></th></tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
        <div class="form-text">Si la persona no está matriculada, use "Agregar manualmente".</div>`);

    const $buscar = $raiz.find('.sel-buscar');
    const $resultados = $raiz.find('.sel-resultados');
    const $cuerpo = $raiz.find('tbody');

    function dibujar() {
        if (!lista.length) {
            $cuerpo.html('<tr><td colspan="4" class="text-center text-muted">Aún no hay personas estudiantes agregadas.</td></tr>');
            return;
        }

        $cuerpo.html(lista.map((e, i) => `
            <tr data-i="${i}">
                <td>${e.idEstudiante
                    ? escapeHtml(e.nombreCompleto)
                    : `<input class="form-control form-control-sm sel-nombre" maxlength="200" placeholder="Nombre completo" value="${escapeAttr(e.nombreCompleto)}" />`}</td>
                <td style="width:140px">
                    <input class="form-control form-control-sm sel-seccion" maxlength="50" value="${escapeAttr(e.seccion || '')}" />
                </td>
                <td style="width:250px">
                    <select class="form-select form-select-sm sel-rol">
                        ${roles.map(r => `<option value="${escapeAttr(r)}" ${r === e.rol ? 'selected' : ''}>${escapeHtml(textoRol(r))}</option>`).join('')}
                    </select>
                </td>
                <td class="text-end">
                    <button type="button" class="btn btn-sm btn-outline-danger sel-quitar" title="Quitar">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>`).join(''));
    }

    function agregar(e) {
        lista.push({ idEstudiante: e.idEstudiante || null, nombreCompleto: e.nombreCompleto || '', seccion: e.seccion || '', rol: roles[0] || '' });
        dibujar();
    }

    function buscar(texto) {
        $.get('/Pauta/BuscarEstudiantes', { texto })
            .done(function (r) {
                const items = r.esCorrecto ? (r.dato || []) : [];

                if (!items.length) {
                    $resultados.html('<div class="list-group-item text-muted small">Sin resultados. Puede agregarla manualmente.</div>').removeClass('d-none');
                    return;
                }

                $resultados.html(items.map(e => `
                    <button type="button" class="list-group-item list-group-item-action sel-item"
                            data-id="${Number(e.idEstudiante)}" data-nombre="${escapeAttr(e.nombreCompleto)}">
                        <div class="fw-semibold">${escapeHtml(e.nombreCompleto)}</div>
                        <div class="small text-muted">${escapeHtml(e.identificacion)}${e.nivel ? ' · ' + escapeHtml(e.nivel) : ''}</div>
                    </button>`).join('')).removeClass('d-none');
            })
            .fail(() => mostrarMensaje('No fue posible buscar personas estudiantes.', 'danger'));
    }

    $buscar.on('input', function () {
        clearTimeout(temporizador);
        const texto = $buscar.val().trim();

        if (texto.length < 2) {
            $resultados.addClass('d-none').empty();
            return;
        }

        temporizador = setTimeout(() => buscar(texto), 300);
    });

    $raiz.on('click', '.sel-item', function () {
        const id = Number($(this).data('id'));

        if (lista.some(e => e.idEstudiante === id)) {
            mostrarMensaje('Esa persona estudiante ya fue agregada.', 'info');
        } else {
            agregar({ idEstudiante: id, nombreCompleto: String($(this).data('nombre')) });
        }

        $resultados.addClass('d-none').empty();
        $buscar.val('');
    });

    $raiz.on('click', '.sel-manual', () => agregar({}));

    $raiz.on('click', '.sel-quitar', function () {
        lista.splice(Number($(this).closest('tr').data('i')), 1);
        dibujar();
    });

    $raiz.on('input', '.sel-nombre', function () {
        lista[Number($(this).closest('tr').data('i'))].nombreCompleto = $(this).val();
    });

    $raiz.on('input', '.sel-seccion', function () {
        lista[Number($(this).closest('tr').data('i'))].seccion = $(this).val();
    });

    $raiz.on('change', '.sel-rol', function () {
        lista[Number($(this).closest('tr').data('i'))].rol = $(this).val();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest($raiz).length) $resultados.addClass('d-none');
    });

    dibujar();

    return {
        setRoles(nuevos) {
            roles = nuevos || [];
            lista.forEach(e => { if (!roles.includes(e.rol)) e.rol = roles[0] || ''; });
            dibujar();
        },
        setLista(datos) {
            lista = (datos || []).map(e => ({
                idEstudiante: e.idEstudiante || null,
                nombreCompleto: e.nombreCompleto || '',
                seccion: e.seccion || '',
                rol: e.rol
            }));
            dibujar();
        },
        getLista() {
            return lista.map(e => ({
                idEstudiante: e.idEstudiante,
                nombreCompleto: (e.nombreCompleto || '').trim(),
                seccion: (e.seccion || '').trim() || null,
                rol: e.rol
            }));
        }
    };
}
