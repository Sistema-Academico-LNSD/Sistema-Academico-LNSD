$(document).ready(function () {
    cargarRoles();

    $("#btnGuardarRol").click(function () {
        crearRol();
    });

    $("#btnActualizarRol").click(function () {
        actualizarRol();
    });
});

function cargarRoles() {
    $.ajax({
        url: '/Rol/GetRoles',
        type: 'GET',

        success: function (response) {
            let filas = '';

            if (response.dato != null) {
                $.each(response.dato, function (index, rol) {
                    filas += `
                        <tr>
                            <td>${rol.idRol}</td>
                            <td>${escapeHtml(rol.nombre)}</td>
                            <td>${escapeHtml(rol.descripcion)}</td>

                            <td>

                                <button
                                    class="btn btn-warning btn-sm me-1"
                                    onclick="obtenerRol(${rol.idRol})">

                                    <i class="bi bi-pencil-square"></i>

                                </button>

                                <button
                                    class="btn btn-danger btn-sm"
                                    onclick="eliminarRol(${rol.idRol})">

                                    <i class="bi bi-trash"></i>

                                </button>

                            </td>
                        </tr>`;
                });
            }

            $("#tblRoles tbody").html(filas);
        },

        error: function () {
            mostrarMensaje("Error al cargar los roles.", 'danger');
        }
    });
}

function crearRol() {
    let rol =
    {
        nombre: $("#Nombre").val(),
        descripcion: $("#Descripcion").val()
    };

    $.ajax({
        url: '/Rol/CreateRol',
        type: 'POST',
        data: rol,

        success: function (response) {
            if (response.esCorrecto) {
                $("#modalCrearRol").modal('hide');

                $("#formCrearRol")[0].reset();

                cargarRoles();

                mostrarMensaje("Rol creado correctamente.", 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error al crear el rol.", 'danger');
        }
    });
}

function obtenerRol(id) {
    $.ajax({
        url: '/Rol/GetRolById',
        type: 'GET',
        data: { id: id },

        success: function (response) {
            if (response.esCorrecto && response.dato != null) {
                $("#editIdRol").val(response.dato.idRol);

                $("#editNombre").val(response.dato.nombre);

                $("#editDescripcion").val(response.dato.descripcion);

                let modal =
                    new bootstrap.Modal(
                        document.getElementById("modalEditarRol")
                    );

                modal.show();
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error obteniendo el rol.", 'danger');
        }
    });
}

function actualizarRol() {
    let rol =
    {
        idRol: $("#editIdRol").val(),
        nombre: $("#editNombre").val(),
        descripcion: $("#editDescripcion").val()
    };

    $.ajax({
        url: '/Rol/UpdateRol',
        type: 'POST',
        data: rol,

        success: function (response) {
            if (response.esCorrecto) {
                bootstrap.Modal.getInstance(
                    document.getElementById("modalEditarRol")
                ).hide();

                cargarRoles();

                mostrarMensaje("Rol actualizado correctamente.", 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error actualizando el rol.", 'danger');
        }
    });
}

async function eliminarRol(id) {
    if (!(await confirmar("¿Desea eliminar este rol? Sus permisos también se eliminarán.", "Eliminar"))) {
        return;
    }

    $.ajax({
        url: '/Rol/DeleteRol',
        type: 'POST',
        data: { id: id },

        success: function (response) {
            if (response.esCorrecto) {
                cargarRoles();

                mostrarMensaje("Rol eliminado correctamente.", 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function () {
            mostrarMensaje("Error eliminando el rol.", 'danger');
        }
    });
}