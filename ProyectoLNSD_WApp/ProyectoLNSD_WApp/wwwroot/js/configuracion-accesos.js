// HU 06: accesos rápidos (asignados por rol)

$(document).ready(function () {
    cargarAccesos();
    cargarRolesAcceso();

    $("#btnNuevoAcceso").click(function () {
        abrirModalAcceso();
    });

    $("#btnGuardarAcceso").click(function () {
        guardarAcceso();
    });
});

// Casillas con los roles existentes
function cargarRolesAcceso() {
    $.ajax({
        url: '/Configuracion/GetRolesAcceso',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) return;

            let casillas = '';
            $.each(response.dato, function (index, r) {
                casillas += `
                    <div class="form-check">
                        <input class="form-check-input rol-acceso" type="checkbox" name="IdsRoles"
                               value="${r.idRol}" id="accesoRol${r.idRol}" />
                        <label class="form-check-label" for="accesoRol${r.idRol}">${escapeHtml(r.nombre)}</label>
                    </div>`;
            });

            $("#accesoRoles").html(casillas);
        }
    });
}

function cargarAccesos() {
    $.ajax({
        url: '/Configuracion/GetAccesosRapidos',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let filas = '';

            if (!response.dato || response.dato.length === 0) {
                filas = '<tr><td colspan="6" class="text-center text-muted">No hay accesos rápidos registrados.</td></tr>';
            }
            else {
                $.each(response.dato, function (index, a) {
                    let estado = a.activo
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-secondary">Inactivo</span>';

                    let roles = (a.nombresRoles && a.nombresRoles.length > 0)
                        ? a.nombresRoles.map(function (n) { return escapeHtml(n); }).join(', ')
                        : '<span class="text-danger">Sin roles (nadie lo ve)</span>';

                    filas += `
                        <tr>
                            <td>${a.orden}</td>
                            <td><i class="bi ${escapeHtml(a.icono || 'bi-link-45deg')}"></i> ${escapeHtml(a.nombre)}</td>
                            <td class="text-break">${escapeHtml(a.enlace)}</td>
                            <td>${roles}</td>
                            <td>${estado}</td>
                            <td class="text-end">
                                <button class="btn btn-sm btn-outline-primary" onclick="editarAcceso(${a.idAcceso})">
                                    <i class="bi bi-pencil"></i> Editar
                                </button>
                                <button class="btn btn-sm btn-outline-danger" onclick="eliminarAcceso(${a.idAcceso})">
                                    <i class="bi bi-trash"></i> Eliminar
                                </button>
                            </td>
                        </tr>`;
                });
            }

            $("#tbodyAccesos").html(filas);
        },

        error: function () {
            mostrarMensaje("Error al cargar los accesos rápidos.", 'danger');
        }
    });
}

function abrirModalAcceso() {
    $("#formAcceso")[0].reset();          // también desmarca las casillas de roles
    $("#accesoId").val(0);
    $("#accesoActivo").val("true");
    $("#tituloModalAcceso").text("Nuevo acceso rápido");
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalAcceso')).show();
}

function editarAcceso(id) {
    $.ajax({
        url: '/Configuracion/GetAccesoRapido',
        type: 'GET',
        data: { id: id },

        success: function (response) {
            if (!response.esCorrecto || !response.dato) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let a = response.dato;
            $("#formAcceso")[0].reset();
            $("#accesoId").val(a.idAcceso);
            $("#accesoNombre").val(a.nombre);
            $("#accesoDescripcion").val(a.descripcion);
            $("#accesoIcono").val(a.icono);
            $("#accesoEnlace").val(a.enlace);
            $("#accesoOrden").val(a.orden);
            $("#accesoActivo").val(a.activo ? "true" : "false");

            // Marca los roles que ya ven este acceso
            $(".rol-acceso").each(function () {
                $(this).prop("checked", a.idsRoles.indexOf(parseInt($(this).val())) >= 0);
            });

            $("#tituloModalAcceso").text("Editar acceso rápido");
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalAcceso')).show();
        },

        error: function () {
            mostrarMensaje("Error al cargar el acceso rápido.", 'danger');
        }
    });
}

function guardarAcceso() {
    // FormData incluye un "IdsRoles" por cada casilla marcada y el token antiforgery
    let datos = new FormData($("#formAcceso")[0]);

    $.ajax({
        url: '/Configuracion/GuardarAccesoRapido',
        type: 'POST',
        data: datos,
        processData: false,
        contentType: false,

        success: function (response) {
            if (response.esCorrecto) {
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalAcceso')).hide();
                cargarAccesos();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al guardar el acceso rápido.", 'danger');
        }
    });
}

function eliminarAcceso(id) {
    if (!confirm("¿Seguro que desea eliminar este acceso rápido?")) {
        return;
    }

    $.ajax({
        url: '/Configuracion/EliminarAccesoRapido',
        type: 'POST',
        data: { id: id },

        success: function (response) {
            if (response.esCorrecto) {
                cargarAccesos();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al eliminar el acceso rápido.", 'danger');
        }
    });
}
