// Funciones compartidas por las pantallas de recursos (docente, estudiante y supervisores)

// La fecha de publicación llega como "2026-10-12T00:00:00": se muestra sin convertir zonas horarias
function formatearFechaPublicacion(valor) {
    if (!valor) {
        return '—';
    }

    let [anio, mes, dia] = valor.substring(0, 10).split('-');

    return `${dia}/${mes}/${anio}`;
}

function formatearTamano(bytes) {
    if (bytes == null) {
        return '';
    }

    if (bytes < 1024 * 1024) {
        return (bytes / 1024).toFixed(1) + ' KB';
    }

    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
}

// La fecha de modificación viene en UTC con "Z", así que sí se convierte a la hora local
function formatearFechaHora(valor) {
    return new Date(valor).toLocaleString('es-CR', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}

// Descarga protegida (HU 01-09): se pide el archivo por JavaScript para poder mostrar
// los errores como mensajes en lugar de dejar al usuario frente a una página en blanco.

    // Pide un archivo al servidor y lo descarga. Los errores de negocio (sin permisos, archivo
// no disponible, sin registros para exportar...) vuelven como JSON y se muestran como mensaje.
function descargarDesde(url, nombreSugerido) {
    fetch(url)
        .then(function (respuesta) {
            let tipo = respuesta.headers.get('Content-Type') || '';

            if (tipo.includes('application/json')) {
                return respuesta.json().then(function (r) {
                    mostrarMensaje(r.mensaje, 'danger');
                });
            }

            // Si la sesión venció, el servidor responde con la página de inicio de sesión
            if (tipo.includes('text/html')) {
                mostrarMensaje("Su sesión expiró. Inicie sesión nuevamente.", 'warning');
                return;
            }

            if (!respuesta.ok) {
                throw new Error('descarga');
            }

            return respuesta.blob().then(function (archivo) {
                let enlaceTemporal = URL.createObjectURL(archivo);
                let enlace = document.createElement('a');

                enlace.href = enlaceTemporal;
                enlace.download = nombreSugerido || 'archivo';

                document.body.appendChild(enlace);
                enlace.click();
                enlace.remove();

                setTimeout(function () { URL.revokeObjectURL(enlaceTemporal); }, 1000);
            });
        })
        .catch(function () {
            // HU 01-09, esc. 5: la descarga se interrumpió y se puede reintentar
            mostrarMensaje("La descarga se interrumpió. Intente nuevamente.", 'danger');
        });
}

function descargarRecurso(idRecurso, nombreSugerido) {
    descargarDesde('/Recurso/Descargar?idRecurso=' + idRecurso, nombreSugerido);
}