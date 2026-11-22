// Elegir la pauta y crear el expediente (MPPAF-01-02 y 01-04).

let pautas = [];
let pautaElegida = null;
let selector = null;

$(function () {
    selector = crearSelectorEstudiantes($('#selectorEstudiantes'), { roles: [] });

    $.get('/Pauta/GetPautas')
        .done(function (r) {
            pautas = r.esCorrecto ? (r.dato || []) : [];
            dibujarPautas();
        })
        .fail(() => $('#listaPautas').html('<div class="col-12 text-danger">No fue posible cargar las pautas.</div>'));

    $(document).on('click', '.pauta-pauta-opcion', function () {
        elegir(Number($(this).data('numero')));
    });

    $('#btnCrear').on('click', crear);
});

function dibujarPautas() {
    const grupos = [
        { titulo: 'Violencia y bullying (formulario de inicio N°1)', inicial: 1 },
        { titulo: 'Vulnerabilidad y riesgo (formulario de inicio N°2)', inicial: 2 }
    ];

    $('#listaPautas').html(grupos.map(g => `
        <div class="col-12"><h6 class="text-muted mb-0">${escapeHtml(g.titulo)}</h6></div>
        ${pautas.filter(p => p.formularioInicial === g.inicial).map(p => `
            <div class="col-md-6 col-xl-4">
                <div class="card h-100 pauta-pauta-opcion" data-numero="${Number(p.numero)}" tabindex="0" role="button">
                    <div class="card-body">
                        <div class="fw-semibold">${Number(p.numero)}. ${escapeHtml(p.nombre)}</div>
                        <div class="small text-muted mt-1">${escapeHtml(p.descripcion)}</div>
                    </div>
                </div>
            </div>`).join('')}`).join(''));
}

function elegir(numero) {
    pautaElegida = pautas.find(p => p.numero === numero) || null;

    $('.pauta-pauta-opcion').removeClass('activa');
    $(`.pauta-pauta-opcion[data-numero="${numero}"]`).addClass('activa');

    if (pautaElegida) {
        selector.setRoles(pautaElegida.rolesEstudiante);
        $('#btnCrear').prop('disabled', false);
    }
}

function crear() {
    if (!pautaElegida) {
        mostrarMensaje('Seleccione la pauta que se activa.', 'warning');
        return;
    }

    const estudiantes = selector.getLista();

    if (estudiantes.some(e => !e.nombreCompleto)) {
        mostrarMensaje('Escriba el nombre de todas las personas estudiantes o quite las filas vacías.', 'warning');
        return;
    }

    const $boton = $('#btnCrear').prop('disabled', true);

    $.ajax({
        url: '/Pauta/CrearExpediente',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            numeroPauta: pautaElegida.numero,
            idExpedienteOrigen: window.pautaOrigen,
            estudiantes
        })
    })
        .done(function (r) {
            if (r.esCorrecto && r.dato) {
                window.location.href = `/Pauta/Expediente/${Number(r.dato.idExpediente)}`;
                return;
            }

            mostrarMensaje(r.mensaje, 'danger');
            $boton.prop('disabled', false);
        })
        .fail(function () {
            mostrarMensaje('No se pudo crear el expediente.', 'danger');
            $boton.prop('disabled', false);
        });
}
