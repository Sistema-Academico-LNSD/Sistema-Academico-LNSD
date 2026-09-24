$(document).ready(function ()
{
    cargarUsuarios();
    cargarRoles();

    $("#btnGuardarUsuario").click(function ()
    {
        crearUsuario();
    });
});

function cargarUsuarios()
{
    $.ajax({
        url: '/Usuario/GetUsuarios',
        type: 'GET',

        success: function (response)
        {
            let filas = '';

            if (response.dato != null)
            {
                $.each(response.dato, function (index, usuario)
                {
                    filas += `
                        <tr>
                            <td>${usuario.idUsuario}</td>
                            <td>${usuario.nombre}</td>
                            <td>${usuario.apellido}</td>
                            <td>${usuario.correo}</td>
                            <td>${usuario.rol ? usuario.rol.nombre : ''}</td>

                            <td>
                                ${usuario.estado
                                    ? '<span class="badge bg-success">Activo</span>'
                                    : '<span class="badge bg-danger">Inactivo</span>'}
                            </td>

                            <td>

                                <button class="btn btn-warning btn-sm">
                                    <i class="bi bi-pencil-square"></i>
                                </button>

                                <button class="btn btn-danger btn-sm">
                                    <i class="bi bi-trash"></i>
                                </button>

                            </td>

                        </tr>`;
                });
            }

            $('#tblUsuarios tbody').html(filas);
        },

        error: function ()
        {
            alert('Error al cargar usuarios');
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

    console.log(usuario);

    $.ajax({
        url: '/Usuario/CreateUsuario',
        type: 'POST',
        data: usuario,

        success: function(response)
        {
            console.log(response);

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
            console.log(xhr);

            alert("Error al crear usuario");
        }
    });
}