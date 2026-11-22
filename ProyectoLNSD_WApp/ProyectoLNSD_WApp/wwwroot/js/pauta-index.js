// Consulta de expedientes de pautas (MPPAF-01-10).

let temporizador = null;

$(function () {
    cargarPautas().then(buscar);

    $('#fEstudiante').on('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(buscar, 300);
    });

    $('#fPauta, #fEstado, #fDesde, #fHasta').on('change', buscar);

    $('#btnLimpiar').on('click', function () {
        $('#fPauta, #fEstado, #fEstudiante, #fDesde, #fHasta').val('');
        buscar();
    });
});

function cargarPautas() {
    return $.get('/Pauta/GetPautas')
        .done(function (r) {
            const pautas = r.esCorrecto ? (r.dato || []) : [];
            $('#fPauta').html('<option value="">Todas</option>' +
                pautas.map(p => `<option value="${Number(p.numero)}">${Number(p.numero)}. ${escapeHtml(p.nombre)}</option>`).join(''));
        })
        .fail(() => mostrarMensaje('No fue posible cargar las pautas.', 'danger'));
}

function buscar() {
    const $cuerpo = $('#tblExpedientes tbody');
    $cuerpo.html('<tr><td colspan="8" class="text-center text-muted">Cargando...</td></tr>');

    $.get('/Pauta/GetExpedientes', {
        numeroPauta: $('#fPauta').val(),
        estado: $('#fEstado').val(),
        textoEstudiante: $('#fEstudiante').val().trim(),
        desde: $('#fDesde').val(),
        hasta: $('#fHasta').val()
    })
        .done(function (r) {
            const lista = r.esCorrecto ? (r.dato || []) : [];

            if (!lista.length) {
                const clase = r.esCorrecto ? 'text-muted' : 'text-danger';
                const texto = r.esCorrecto ? 'No hay expedientes con esos filtros.' : r.mensaje;
                $cuerpo.html(`<tr><td colspan="8" class="text-center ${clase}">${escapeHtml(texto)}</td></tr>`);
                return;
            }

            $cuerpo.html(lista.map(e => `
                <tr>
                    <td class="fw-semibold">${escapeHtml(e.codigo)}</td>
                    <td>${Number(e.numeroPauta)}. ${escapeHtml(e.nombrePauta)}</td>
                    <td>${e.estudiantes.length ? e.estudiantes.map(escapeHtml).join('<br>') : '<span class="text-muted">Sin registrar</span>'}</td>
                    <td>${escapeHtml(e.responsable)}</td>
                    <td>${fechaCorta(e.fechaActivacion)}</td>
                    <td>${Number(e.formulariosFinalizados)} de ${Number(e.totalFormularios)} finalizados</td>
                    <td>${badgeEstado(e.estado)}</td>
                    <td class="text-end">
                        <a class="btn btn-sm btn-outline-primary" href="/Pauta/Expediente/${Number(e.idExpediente)}">
                            <i class="bi bi-folder2-open"></i> Abrir
                        </a>
                    </td>
                </tr>`).join(''));
        })
        .fail(() => $cuerpo.html('<tr><td colspan="8" class="text-center text-danger">No fue posible cargar los expedientes.</td></tr>'));
}
