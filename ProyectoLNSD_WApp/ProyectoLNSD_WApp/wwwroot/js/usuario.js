$(document).ready(function ()
{
    cargarUsuarios();
    cargarRoles();

    $("#btnGuardarUsuario").click(function ()
    {
        crearUsuario();
    });

    $("#btnActualizarUsuario").click(function ()
    {
        actualizarUsuario();
    });

    $("#btnBuscar").click(function ()
    {
        buscarUsuarios();
    });
});

function renderizarUsuarios(response)
{
    let filas = '';

    if (response.dato != null)
    {
        $.each(response.dato, function (index, usuario)
        {
            let estadoBadge = usuario.estado
                ? '<span class="badge bg-success">Activo</span>'
                : '<span class="badge bg-danger">Inactivo</span>';

            let botonEstado = usuario.estado
                ? `<button class="btn btn-danger btn-sm me-1"
                           title="Desactivar"
                           onclick="cambiarEstadoUsuario(${usuario.idUsuario}, false)">
                        <i class="bi bi-x-circle"></i>
                   </button>`
                : `<button class="btn btn-success btn-sm me-1"
                           title="Activar"
                           onclick="cambiarEstadoUsuario(${usuario.idUsuario}, true)">
                        <i class="bi bi-check-circle"></i>
                   </button>`;

            filas += `
                <tr>
                    <td>${usuario.idUsuario}</td>
                    <td>${usuario.nombre}</td>
                    <td>${usuario.apellido}</td>
                    <td>${usuario.correo}</td>
                    <td>${usuario.rol ? usuario.rol.nombre : ''}</td>
                    <td>${estadoBadge}</td>

                    <td>

                        <button class="btn btn-warning btn-sm me-1"
                                title="Editar"
                                onclick="obtenerUsuario(${usuario.idUsuario})">
                            <i class="bi bi-pencil-square"></i>
                        </button>

                        ${botonEstado}

                    </td>

                </tr>`;
        });
    }

    $('#tblUsuarios tbody').html(filas);
}

function cargarUsuarios()
{
    $.ajax({
        url: '/Usuario/GetUsuarios',
        type: 'GET',

        success: function (response)
        {
            renderizarUsuarios(response);
        },

        error: function ()
        {
            alert('Error al cargar usuarios');
        }
    });
}

function buscarUsuarios()
{
    let filtros = {
        nombre: $("#txtNombre").val(),
        correo: $("#txtCorreo").val(),
        idRol: $("#ddlFiltroRol").val(),
        estado: $("#ddlEstado").val()
    };

    $.ajax({
        url: '/Usuario/BuscarUsuarios',
        type: 'GET',
        data: filtros,

        success: function (response)
        {
            renderizarUsuarios(response);
        },

        error: function ()
        {
            alert('Error al buscar usuarios');
        }
    });
}

function cargarRoles()
{
    $.ajax({
        url: '/Rol/GetRoles',
        type: 'GET',

        success: function (response)
        {
            let opciones =
                '<option value="">Seleccione...</option>';

            if (response.dato != null)
            {
                $.each(response.dato, function (index, rol)
                {
                    opciones += `
                        <option value="${rol.idRol}">
                            ${rol.nombre}
                        </option>`;
                });
            }

            $('#IdRol').html(opciones);
            $('#editIdRol').html(opciones);

            let opcionesFiltro =
                '<option value="">Todos los roles</option>';

            if (response.dato != null)
            {
                $.each(response.dato, function (index, rol)
                {
                    opcionesFiltro += `
                        <option value="${rol.idRol}">
                            ${rol.nombre}
                        </option>`;
                });
            }

            $('#ddlFiltroRol').html(opcionesFiltro);
        },

        error: function ()
        {
            alert('Error al cargar roles');
        }
    });
}

function crearUsuario()
{
    let usuario =
    {
        nombre: $("#Nombre").val(),
        apellido: $("#Apellido").val(),
        correo: $("#Correo").val(),
        password: $("#Password").val(),
        idRol: $("#IdRol").val()
    };

    $.ajax({
        url: '/Usuario/CreateUsuario',
        type: 'POST',
        data: usuario,

        success: function(response)
        {
            if(response.esCorrecto)
            {
                alert("Usuario creado correctamente");

                $("#formCrearUsuario")[0].reset();

                cargarUsuarios();

                bootstrap.Modal.getInstance(
                    document.getElementById("modalCrearUsuario")
                ).hide();
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function(xhr)
        {
            alert("Error al crear usuario");
        }
    });
}

function obtenerUsuario(id)
{
    $.ajax({
        url: '/Usuario/GetUsuarioById',
        type: 'GET',
        data: { id: id },

        success: function (response)
        {
            if (response.esCorrecto && response.dato != null)
            {
                $("#editIdUsuario").val(response.dato.idUsuario);
                $("#editNombre").val(response.dato.nombre);
                $("#editApellido").val(response.dato.apellido);
                $("#editCorreo").val(response.dato.correo);
                $("#editIdRol").val(response.dato.idRol);
                $("#editEstado").val(response.dato.estado ? "true" : "false");

                let modal =
                    new bootstrap.Modal(
                        document.getElementById("modalEditarUsuario")
                    );

                modal.show();
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function ()
        {
            alert("Error obteniendo el usuario.");
        }
    });
}

function actualizarUsuario()
{
    let usuario =
    {
        idUsuario: $("#editIdUsuario").val(),
        nombre: $("#editNombre").val(),
        apellido: $("#editApellido").val(),
        correo: $("#editCorreo").val(),
        idRol: $("#editIdRol").val(),
        estado: $("#editEstado").val()
    };

    $.ajax({
        url: '/Usuario/UpdateUsuario',
        type: 'POST',
        data: usuario,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                bootstrap.Modal.getInstance(
                    document.getElementById("modalEditarUsuario")
                ).hide();

                cargarUsuarios();

                alert("Usuario actualizado correctamente.");
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function ()
        {
            alert("Error actualizando el usuario.");
        }
    });
}

function cambiarEstadoUsuario(id, nuevoEstado)
{
    let mensajeConfirmacion = nuevoEstado
        ? "¿Desea activar este usuario?"
        : "¿Desea desactivar este usuario?";

    if (!confirm(mensajeConfirmacion))
    {
        return;
    }

    $.ajax({
        url: '/Usuario/CambiarEstado',
        type: 'POST',
        data: { id: id, estado: nuevoEstado },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                cargarUsuarios();
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function ()
        {
            alert("Error cambiando el estado del usuario.");
        }
    });
}