// Pantalla de un formulario del MEP. Dibuja los campos a partir de la plantilla que envía el servidor
// (MPPAF-01-03, 01-05, 01-07, 01-11 y 01-13). El servidor vuelve a limpiar y validar todo al guardar.

let formulario = null;
let datos = {};
let sucio = false;
let contador = 0;

$(function () {
    cargar();

    $('#btnGuardar').on('click', () => guardar(false));
    $('#btnFinalizar').on('click', () => guardar(true));
    $('#btnEliminar').on('click', eliminar);
    $('#btnReabrir').on('click', () => {
        $('#rMotivo').val('');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('modalReabrir')).show();
    });
    $('#btnConfirmarReabrir').on('click', reabrir);

    window.addEventListener('beforeunload', function (e) {
        if (sucio) {
            e.preventDefault();
            e.returnValue = '';
        }
    });
});

// =====================  Carga y dibujo  =====================

function cargar() {
    $.get('/Pauta/GetFormulario', { idFormulario: window.pautaFormularioId })
        .done(function (r) {
            if (!r.esCorrecto) {
                $('#mensajeFormulario').html(`
                    <div class="alert alert-danger">${escapeHtml(r.mensaje)}</div>
                    <a class="btn btn-outline-secondary" href="/Pauta/Index">Volver a los expedientes</a>`);
                return;
            }
            aplicar(r.dato);
        })
        .fail(() => $('#mensajeFormulario').html('<div class="alert alert-danger">No fue posible cargar el formulario.</div>'));
}

function aplicar(f) {
    formulario = f;

    try { datos = JSON.parse(f.datos || '{}') || {}; }
    catch (e) { datos = {}; }

    sucio = false;
    dibujar();
}

function editable() {
    return formulario.puedeEditar && window.pautaPermisos.editar;
}

function dibujar() {
    const p = formulario.plantilla;

    $('#contenidoFormulario').removeClass('d-none');
    $('#frmTitulo').text(`Formulario N°${p.numero}`);
    $('#frmNombre').text(p.nombre);
    $('#frmDescripcion').text(p.descripcion);
    $('#frmExpediente').text(formulario.codigoExpediente + (formulario.secuencia > 1 ? ` (#${formulario.secuencia})` : ''));
    $('#frmEstado').html(badgeEstado(formulario.estado));
    $('#btnVolver').attr('href', `/Pauta/Expediente/${Number(formulario.idExpediente)}`);

    const $contenedor = $('#seccionesFormulario').empty();
    p.secciones.forEach(s => $contenedor.append(crearSeccion(s)));

    const puedeEditar = editable();

    if (!puedeEditar) {
        $contenedor.find('input, select, textarea, button').prop('disabled', true);
    }

    $('#btnGuardar, #btnFinalizar').toggleClass('d-none', !puedeEditar);
    $('#btnEliminar').toggleClass('d-none', !(puedeEditar && formulario.estado === 'Borrador'));
    $('#btnReabrir').toggleClass('d-none', !(formulario.puedeReabrir && window.pautaPermisos.editar));

    $('#mensajeFormulario').html(formulario.estado === 'Finalizado'
        ? '<div class="alert alert-info py-2 small"><i class="bi bi-lock"></i> Este formulario está finalizado y no se puede modificar.</div>'
        : '');

    actualizarVisibilidad();
}

function crearSeccion(s) {
    const $tarjeta = $('<div class="card mb-3 pauta-seccion"><div class="card-header"></div><div class="card-body"></div></div>');
    const $cuerpo = $tarjeta.find('.card-body');

    $tarjeta.find('.card-header').text(s.titulo);
    if (s.visibleSi) $tarjeta.attr('data-visible', s.visibleSi);
    if (s.descripcion) $cuerpo.append($('<p class="text-muted small">').text(s.descripcion));

    if (s.presentacion === 'fila') {
        const $fila = $('<div class="row g-2">');
        s.campos.forEach(c => $fila.append($('<div class="col-md">').append(crearCampo(c, contexto(c), false))));
        $cuerpo.append($fila);
    } else {
        s.campos.forEach(c => $cuerpo.append(crearCampo(c, contexto(c), false)));
    }

    return $tarjeta;
}

function contexto(c) {
    return {
        get: () => datos[c.clave],
        set: v => {
            datos[c.clave] = v;
            sucio = true;
            actualizarVisibilidad();
        }
    };
}

function etiqueta(c, id) {
    const $e = $('<label class="form-label">').attr('for', id).text(c.etiqueta);
    if (c.requerido) $e.append($('<span class="pauta-requerido"> *</span>'));
    return $e;
}

function crearCampo(c, ctx, enTabla) {
    const id = `c${++contador}`;
    const $envoltura = $('<div>').addClass(enTabla ? '' : 'mb-3');
    const valor = ctx.get();

    if (!enTabla && c.visibleSi) $envoltura.attr('data-visible', c.visibleSi);

    switch (c.tipo) {
        case 'textoFijo':
            return $envoltura.append($('<div class="pauta-fijo">').text(c.etiqueta));

        case 'firma':
            return $envoltura.append($('<div class="pauta-firma">').text(`${c.etiqueta} (se firma en el documento impreso)`));

        case 'sello':
            return $envoltura.append($('<div class="pauta-sello">').text(`${c.etiqueta} (se coloca en el documento impreso)`));

        case 'tabla':
            return $envoltura.append(etiqueta(c, id), crearTabla(c));

        case 'textoLargo': {
            const $t = $('<textarea class="form-control" rows="3">').attr({ id, maxlength: c.maxLongitud || 4000 }).val(valor ?? '');
            $t.on('input', () => ctx.set($t.val()));
            if (!enTabla) $envoltura.append(etiqueta(c, id));
            else $t.attr({ rows: 2, 'aria-label': c.etiqueta });
            if (c.ayuda) $envoltura.append($('<div class="form-text">').text(c.ayuda));
            return $envoltura.append($t);
        }

        case 'siNo': {
            const nombre = `sn${contador}`;
            const $grupo = $('<div class="btn-group" role="group">');

            ['Si', 'No'].forEach(v => {
                const rid = `${id}_${v}`;
                $grupo.append(
                    $('<input type="radio" class="btn-check" autocomplete="off">').attr({ name: nombre, id: rid })
                        .prop('checked', valor === v).on('change', () => ctx.set(v)),
                    $('<label class="btn btn-outline-secondary btn-sm">').attr('for', rid).text(v === 'Si' ? 'Sí' : 'No'));
            });

            if (!enTabla) $envoltura.append(etiqueta(c, id));
            return $envoltura.append($grupo);
        }

        case 'casilla': {
            const $c = $('<input type="checkbox" class="form-check-input">').attr('id', id).prop('checked', valor === true);
            $c.on('change', () => ctx.set($c.is(':checked')));

            if (enTabla) return $envoltura.addClass('text-center').append($c.attr('aria-label', c.etiqueta));

            return $envoltura.append($('<div class="form-check">').append($c, $('<label class="form-check-label">').attr('for', id).text(c.etiqueta)));
        }

        case 'seleccion': {
            const $s = $('<select class="form-select">').attr('id', id);
            $s.append('<option value="">Seleccione...</option>');
            (c.opciones || []).forEach(o => $s.append($('<option>').val(o.valor).text(o.texto)));
            $s.val(valor ?? '').on('change', () => ctx.set($s.val()));

            if (!enTabla) $envoltura.append(etiqueta(c, id));
            return $envoltura.append($s);
        }

        case 'opciones': {
            const elegidas = Array.isArray(valor) ? valor.slice() : [];
            const $lista = $('<div>');

            (c.opciones || []).forEach((o, i) => {
                const oid = `${id}_${i}`;
                const $i = $('<input type="checkbox" class="form-check-input">').attr('id', oid).val(o.valor).prop('checked', elegidas.includes(o.valor));
                $i.on('change', () => ctx.set($lista.find('input:checked').map((_, x) => x.value).get()));
                $lista.append($('<div class="form-check">').append($i, $('<label class="form-check-label">').attr('for', oid).text(o.texto)));
            });

            return $envoltura.append(etiqueta(c, id), $lista);
        }

        default: {
            const tipos = { texto: 'text', correo: 'email', telefono: 'tel', fecha: 'date', hora: 'time', numero: 'number' };
            const $i = $('<input class="form-control">').attr({ type: tipos[c.tipo] || 'text', id }).val(valor ?? '');

            if (c.tipo === 'numero') $i.attr({ min: 0, step: 'any' });
            if (c.tipo === 'texto' || c.tipo === 'correo' || c.tipo === 'telefono') $i.attr('maxlength', c.maxLongitud || (c.tipo === 'texto' ? 300 : 150));
            if (enTabla) $i.addClass('form-control-sm').attr('aria-label', c.etiqueta);

            $i.on('input change', () => ctx.set($i.val()));

            if (!enTabla) $envoltura.append(etiqueta(c, id));
            if (c.ayuda) $envoltura.append($('<div class="form-text">').text(c.ayuda));
            return $envoltura.append($i);
        }
    }
}

function crearTabla(c) {
    if (!Array.isArray(datos[c.clave])) datos[c.clave] = [];
    const filas = datos[c.clave];

    if (c.minFilas && !filas.length) filas.push({});

    const maxFilas = c.maxFilas || 50;
    const $caja = $('<div>');
    const $tabla = $(`
        <div class="table-responsive">
            <table class="table table-sm align-middle pauta-tabla">
                <thead><tr></tr></thead>
                <tbody></tbody>
            </table>
        </div>`);
    const $agregar = $('<button type="button" class="btn btn-sm btn-outline-primary agregar-fila"><i class="bi bi-plus-lg"></i> Agregar fila</button>');

    c.columnas.forEach(col => {
        const $th = $('<th>').text(col.etiqueta);
        if (col.tipo === 'casilla') $th.addClass('text-center');
        if (col.requerido) $th.append($('<span class="pauta-requerido"> *</span>'));
        $tabla.find('thead tr').append($th);
    });
    $tabla.find('thead tr').append('<th></th>');

    function dibujarFilas() {
        const $cuerpo = $tabla.find('tbody').empty();

        filas.forEach((fila, i) => {
            const $tr = $('<tr>');

            c.columnas.forEach(col => {
                const ctx = {
                    get: () => fila[col.clave],
                    set: v => { fila[col.clave] = v; sucio = true; actualizarVisibilidad(); }
                };
                $tr.append($('<td>').css('min-width', col.tipo === 'casilla' ? '70px' : '140px').append(crearCampo(col, ctx, true)));
            });

            const $quitar = $('<button type="button" class="btn btn-sm btn-outline-danger quitar-fila" title="Quitar fila"><i class="bi bi-trash"></i></button>');
            $quitar.on('click', () => {
                filas.splice(i, 1);
                sucio = true;
                dibujarFilas();
            });

            $tr.append($('<td class="text-end">').append($quitar));
            $cuerpo.append($tr);
        });

        $agregar.prop('disabled', !editable() || filas.length >= maxFilas);
        if (!editable()) $tabla.find('input, select, textarea, button').prop('disabled', true);
    }

    $agregar.on('click', () => {
        if (filas.length >= maxFilas) return;
        filas.push({});
        sucio = true;
        dibujarFilas();
    });

    dibujarFilas();
    return $caja.append($tabla, $agregar);
}

// =====================  Visibilidad según las respuestas  =====================

function valores(v) {
    if (Array.isArray(v)) return v.map(String).filter(x => x);
    if (v === true) return ['true'];
    if (v === false || v === null || v === undefined || v === '') return [];
    return [String(v)];
}

// Misma regla que el servidor: "clave=valor", "clave!=valor", "clave=" (vacío); varias separadas por ";".
function esVisible(condicion) {
    if (!condicion) return true;

    return condicion.split(';').map(x => x.trim()).filter(Boolean).every(parte => {
        const negada = parte.includes('!=');
        const [clave, esperado = ''] = parte.split(negada ? '!=' : '=').map(x => x.trim());
        const actuales = valores(datos[clave]);
        const cumple = esperado === '' ? actuales.length === 0 : actuales.includes(esperado);
        return negada ? !cumple : cumple;
    });
}

function actualizarVisibilidad() {
    $('#seccionesFormulario [data-visible]').each(function () {
        $(this).toggleClass('d-none', !esVisible($(this).attr('data-visible')));
    });
}

// =====================  Acciones  =====================

function mostrarAlerta(texto) {
    $('#alertaFormulario').text(mensajeEnLineas(texto)).removeClass('d-none');
    document.getElementById('alertaFormulario').scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function ocultarAlerta() {
    $('#alertaFormulario').addClass('d-none').empty();
}

async function guardar(finalizar) {
    if (finalizar) {
        const ok = await confirmar(
            '¿Finalizar el formulario? Una vez finalizado solo la dirección o el administrador podrán reabrirlo.',
            'Finalizar', 'success');
        if (!ok) return;
    }

    const $botones = $('#barraAcciones button').prop('disabled', true);

    $.ajax({
        url: '/Pauta/GuardarFormulario',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            idFormulario: formulario.idFormulario,
            datos: JSON.stringify(datos),
            finalizar
        })
    })
        .done(function (r) {
            if (r.dato) aplicar(r.dato);   // se muestra lo que realmente quedó guardado

            if (r.esCorrecto) {
                ocultarAlerta();
                mostrarMensaje(r.mensaje, 'success');
            } else {
                mostrarAlerta(r.mensaje);
            }
        })
        .fail(() => mostrarMensaje('No se pudo guardar el formulario.', 'danger'))
        .always(() => $botones.prop('disabled', false));
}

function reabrir() {
    const motivo = $('#rMotivo').val().trim();

    if (motivo.length < 10) {
        mostrarMensaje('Escriba el motivo de la reapertura (mínimo 10 caracteres).', 'warning');
        return;
    }

    const $boton = $('#btnConfirmarReabrir').prop('disabled', true);

    $.post('/Pauta/ReabrirFormulario', { idFormulario: formulario.idFormulario, motivo })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.esCorrecto ? 'success' : 'danger');

            if (r.esCorrecto && r.dato) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalReabrir')).hide();
                ocultarAlerta();
                aplicar(r.dato);
            }
        })
        .fail(() => mostrarMensaje('No se pudo reabrir el formulario.', 'danger'))
        .always(() => $boton.prop('disabled', false));
}

async function eliminar() {
    const ok = await confirmar('¿Quitar este formulario del expediente? Todavía no tiene datos guardados.', 'Quitar');
    if (!ok) return;

    $.post('/Pauta/EliminarFormulario', { idFormulario: formulario.idFormulario })
        .done(function (r) {
            if (r.esCorrecto) {
                sucio = false;
                window.location.href = `/Pauta/Expediente/${Number(formulario.idExpediente)}`;
                return;
            }
            mostrarMensaje(r.mensaje, 'danger');
        })
        .fail(() => mostrarMensaje('No se pudo quitar el formulario.', 'danger'));
}
