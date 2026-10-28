// Detalle de un grado con sus secciones (MGGF-01-09).
// El id llega en data-id-grado para no mezclar Razor dentro del JavaScript.
$(function () {
    const idGrado = Number($('#detalleGrado').data('id-grado'));

    $.get('/Grado/GetDetalleGrado', { id: idGrado }, function (r) {
        if (!r.esCorrecto || !r.dato) {
            mostrarMensaje(r.mensaje || 'No se encontró el grado', 'danger');
            return;
        }

        const g = r.dato.grado;

        $('#tituloDetalle').text(g.nombre);
        $('#infoGrado').text(
            'Código: ' + g.codigo +
            ' · Nivel: ' + g.nivel +
            ' · Estado: ' + (g.estado ? 'Activo' : 'Inactivo')
        );

        if (!r.dato.secciones || r.dato.secciones.length === 0) {
            $('#sinSecciones').removeClass('d-none');
            return;
        }

        const filas = r.dato.secciones.map(function (s) {
            return `
                <tr>
                    <td>${escapeHtml(s.nombre)}</td>
                    <td>${Number(s.capacidadMaxima)}</td>
                    <td>${Number(s.cuposDisponibles)}</td>
                    <td>${s.estado ? 'Activo' : 'Inactivo'}</td>
                </tr>`;
        }).join('');

        $('#tblDetalleSecciones tbody').html(filas);
    });
});
