$(document).ready(function ()
{
    cargarRoles();

    $("#btnGuardarRol").click(function ()
    {
        crearRol();
    });

    $("#btnActualizarRol").click(function ()
    {
        actualizarRol();
    });
});

function cargarRoles()
{
    $.ajax({
        url: '/Rol/GetRoles',
        type: 'GET',

        success: function (response)
        {
            let filas = '';

            if (response.dato != null)
            {
                $.each(response.dato, function (index, rol)
                {
                    filas += `
                        <tr>
                            <td>${rol.idRol}</td>
                            <td>${rol.nombre}</td>
                            <td>${rol.descripcion ?? ''}</td>

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

        error: function ()
        {
            alert("Error al cargar los roles.");
        }
    });
}

function crearRol()
{
    let rol =
    {
        nombre: $("#Nombre").val(),
        descripcion: $("#Descripcion").val()
    };

    $.ajax({
        url: '/Rol/CreateRol',
        type: 'POST',
        data: rol,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                $("#modalCrearRol").modal('hide');

                $("#formCrearRol")[0].reset();

                cargarRoles();

                alert("Rol creado correctamente.");
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function ()
        {
            alert("Error al crear el rol.");
        }
    });
}

function obtenerRol(id)
{
    $.ajax({
        url: '/Rol/GetRolById',
        type: 'GET',
        data: { id: id },

        success: function (response)
        {
            if (response.esCorrecto && response.dato != null)
            {
                $("#editIdRol").val(response.dato.idRol);

                $("#editNombre").val(response.dato.nombre);

                $("#editDescripcion").val(response.dato.descripcion);

                let modal =
                    new bootstrap.Modal(
                        document.getElementById("modalEditarRol")
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
            alert("Error obteniendo el rol.");
        }
    });
}

function actualizarRol()
{
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

        success: function (response)
        {
            if (response.esCorrecto)
            {
                bootstrap.Modal.getInstance(
                    document.getElementById("modalEditarRol")
                ).hide();

                cargarRoles();

                alert("Rol actualizado correctamente.");
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function ()
        {
            alert("Error actualizando el rol.");
        }
    });
}

function eliminarRol(id)
{
    if (!confirm("¿Desea eliminar este rol?"))
    {
        return;
    }

    $.ajax({
        url: '/Rol/DeleteRol',
        type: 'POST',
        data: { id: id },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                cargarRoles();

                alert("Rol eliminado correctamente.");
            }
            else
            {
                alert(response.mensaje);
            }
        },

        error: function ()
        {
            alert("Error eliminando el rol.");
        }
    });
}