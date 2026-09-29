$(document).ready(function () {
    cargarPeriodos();
 
    $("#btnNuevoPeriodo").click(function () {
        abrirModalPeriodo();
    });
 
    $("#btnGuardarPeriodo").click(function () {
        guardarPeriodo(false);
    });
});
 
function formatearFecha(iso) {
    // "2026-02-01" -> "01/02/2026" (sin pasar por Date para evitar desfases de zona horaria)
    return iso ? iso.split('-').reverse().join('/') : '';
}
 
function cargarPeriodos() {
    $.ajax({
        url: '/Configuracion/GetPeriodos',
        type: 'GET',
 
        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }
 
            let filas = '';
 
            if (!response.dato || response.dato.length === 0) {
                filas = '<tr><td colspan="5" class="text-center text-muted">No hay períodos lectivos registrados.</td></tr>';
            }
            else {
                $.each(response.dato, function (index, p) {
                    let estado = p.activo
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-secondary">Inactivo</span>';
 
                    filas += `
                        <tr>
                            <td>${escapeHtml(p.nombre)}</td>
                            <td>${formatearFecha(p.fechaInicio)}</td>
                            <td>${formatearFecha(p.fechaFin)}</td>
                            <td>${estado}</td>
                            <td class="text-end">
                                <button class="btn btn-sm btn-outline-primary" onclick="editarPeriodo(${p.idPeriodo})">
                                    <i class="bi bi-pencil"></i> Editar
                                </button>
                            </td>
                        </tr>`;
                });
            }
 
            $("#tbodyPeriodos").html(filas);
        },
 
        error: function () {
            mostrarMensaje("Error al cargar los períodos lectivos.", 'danger');
        }
    });
}
 
function abrirModalPeriodo() {
    $("#formPeriodo")[0].reset();
    $("#periodoId").val(0);
    $("#tituloModalPeriodo").text("Nuevo período lectivo");
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalPeriodo')).show();
}
 
function editarPeriodo(id) {
    $.ajax({
        url: '/Configuracion/GetPeriodo',
        type: 'GET',
        data: { id: id },
 
        success: function (response) {
            if (!response.esCorrecto || !response.dato) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }
 
            let p = response.dato;
            $("#periodoId").val(p.idPeriodo);
            $("#periodoNombre").val(p.nombre);
            $("#periodoInicio").val(p.fechaInicio);
            $("#periodoFin").val(p.fechaFin);
            $("#periodoActivo").prop("checked", p.activo);
            $("#tituloModalPeriodo").text("Editar período lectivo");
 
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalPeriodo')).show();
        },
 
        error: function () {
            mostrarMensaje("Error al cargar el período lectivo.", 'danger');
        }
    });
}
 
// confirmado = true cuando el usuario ya aceptó reemplazar el período activo actual
function guardarPeriodo(confirmado) {
    let datos = new FormData($("#formPeriodo")[0]);
    if (confirmado) {
        datos.append("confirmarCambio", "true");
    }
 
    $.ajax({
        url: '/Configuracion/GuardarPeriodo',
        type: 'POST',
        data: datos,
        processData: false,
        contentType: false,
 
        success: function (response) {
            if (response.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalPeriodo')).hide();
                cargarPeriodos();
                mostrarMensaje(response.mensaje, 'success');
            }
            else if (response.codigo === 3104) {
                // Ya hay otro período activo: se pide confirmar el cambio (HU escenario 4)
                if (confirm(response.mensaje)) {
                    guardarPeriodo(true);
                }
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },
 
        error: function () {
            mostrarMensaje("Error al guardar el período lectivo.", 'danger');
        }
    });
}