$(document).ready(function ()
{
    if ($("#tblRolesPermiso").length)
    {
        cargarRolesPermiso();
    }

    if ($("#tblMatrizPermisos").length)
    {
        cargarMatriz();

        $("#btnGuardarPermisos").on("click", function ()
        {
            guardarPermisos();
        });
    }
});

function cargarRolesPermiso()
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
                            <td>${rol.nombre}</td>
                            <td>${rol.descripcion ?? ''}</td>
                            <td>
                                <a class="btn btn-primary btn-sm"
                                   href="/Permiso/Matriz?idRol=${rol.idRol}&nombreRol=${encodeURIComponent(rol.nombre)}">
                                    <i class="bi bi-shield-lock"></i>
                                    Gestionar permisos
                                </a>
                            </td>
                        </tr>`;
                });
            }

            $('#tblRolesPermiso tbody').html(filas);
        },

        error: function ()
        {
            alert('Error al cargar los roles.');
        }
    });
}

function cargarMatriz()
{
    let idRol = $("#hdnIdRol").val();

    $.ajax({
        url: '/Permiso/GetMatriz',
        type: 'GET',
        data: { idRol: idRol },

        success: function (response)
        {
            let filas = '';

            if (response.dato != null)
            {
                $.each(response.dato, function (index, m)
                {
                    filas += `
                        <tr data-idmodulo="${m.idModulo}">
                            <td>${m.nombreModulo}</td>
                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chk-ver" ${m.puedeVer ? 'checked' : ''} />
                            </td>
                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chk-crear" ${m.puedeCrear ? 'checked' : ''} />
                            </td>
                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chk-editar" ${m.puedeEditar ? 'checked' : ''} />
                            </td>
                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chk-eliminar" ${m.puedeEliminar ? 'checked' : ''} />
                            </td>
                        </tr>`;
                });
            }

            $('#tblMatrizPermisos tbody').html(filas);
        },

        error: function ()
        {
            mostrarAlertaPermisos(false, 'Error al cargar la matriz de permisos.');
        }
    });
}

function guardarPermisos()
{
    let idRol = parseInt($("#hdnIdRol").val(), 10);
    let token = $("input[name='__RequestVerificationToken']").val();

    let permisos = [];

    $("#tblMatrizPermisos tbody tr").each(function ()
    {
        let $fila = $(this);

        permisos.push({
            IdModulo: parseInt($fila.data("idmodulo"), 10),
            PuedeVer: $fila.find(".chk-ver").is(":checked"),
            PuedeCrear: $fila.find(".chk-crear").is(":checked"),
            PuedeEditar: $fila.find(".chk-editar").is(":checked"),
            PuedeEliminar: $fila.find(".chk-eliminar").is(":checked")
        });
    });

    let $btn = $("#btnGuardarPermisos");
    $btn.prop("disabled", true).text("Guardando...");

    $.ajax({
        url: '/Permiso/GuardarPermisos',
        type: 'POST',
        contentType: 'application/json',
        headers: { 'X-CSRF-TOKEN': token },
        data: JSON.stringify({ IdRol: idRol, Permisos: permisos }),

        success: function (response)
        {
            mostrarAlertaPermisos(response.esCorrecto, response.mensaje);
        },

        error: function ()
        {
            mostrarAlertaPermisos(false, 'Error al guardar los permisos.');
        },

        complete: function ()
        {
            $btn.prop("disabled", false).html('<i class="bi bi-save"></i> Guardar cambios');
        }
    });
}

function mostrarAlertaPermisos(esCorrecto, mensaje)
{
    let $alerta = $("#alertPermisos");

    $alerta
        .removeClass("d-none alert-success alert-danger")
        .addClass(esCorrecto ? "alert-success" : "alert-danger")
        .text(mensaje);
}