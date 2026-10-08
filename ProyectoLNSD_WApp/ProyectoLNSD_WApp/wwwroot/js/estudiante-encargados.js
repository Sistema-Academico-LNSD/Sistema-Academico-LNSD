// ============================================================
// Encargados del estudiante (MESF-01-03 y MESF-01-04)
// Requiere estudiante.js (usa estudiantesCache).
// ============================================================

const PARENTESCOS = ["Padre", "Madre", "Abuelo", "Abuela", "Tío", "Tía",
                     "Hermano", "Hermana", "Tutor legal", "Otro"];

let idEncargadoSeleccionado = 0;

$(document).ready(function ()
{
    let opciones = '<option value="">Seleccione...</option>';

    $.each(PARENTESCOS, function (i, p)
    {
        opciones += `<option value="${p}">${p}</option>`;
    });

    $(".enc-parentesco").html(opciones);

    $("#btnEncBuscar").on("click", buscarEncargados);
    $("#btnEncAsociar").on("click", asociarEncargado);
    $("#btnEncCrear").on("click", crearEncargado);

    $("#txtEncBuscar").on("keydown", function (e)
    {
        if (e.key === "Enter")
        {
            buscarEncargados();
        }
    });
});


function abrirModalEncargados(idEstudiante)
{
    let est = estudiantesCache[idEstudiante];

    $("#hdnIdEstudianteEnc").val(idEstudiante);

    $("#lblTituloEncargados").text(
        est
            ? `Encargados de ${est.nombre} ${est.apellidos} (${est.carnet})`
            : "Encargados");

    limpiarFormulariosEncargado();
    cargarEncargados();

    bootstrap.Modal
        .getOrCreateInstance(document.getElementById("modalEncargados"))
        .show();
}


function idEstudianteActual()
{
    return parseInt($("#hdnIdEstudianteEnc").val(), 10) || 0;
}


function limpiarFormulariosEncargado()
{
    idEncargadoSeleccionado = 0;

    $("#txtEncBuscar").val('');
    $("#lstEncResultados").empty();
    $("#ddlEncParentescoExistente").val('');

    $("#txtEncNombre, #txtEncApellidos, #txtEncCorreo, #txtEncTelefono, #txtEncPassword").val('');
    $("#ddlEncParentescoNuevo").val('');
}


function cargarEncargados()
{
    $.ajax({
        url: '/Estudiante/GetEncargados',
        type: 'GET',
        data: { idEstudiante: idEstudianteActual() },

        success: function (response)
        {
            if (!response.esCorrecto)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            renderizarEncargados(response.dato ?? []);
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los encargados.', 'danger');
        }
    });
}


function renderizarEncargados(encargados)
{
    let filas = '';

    $.each(encargados, function (i, e)
    {
        let badgePrincipal = e.esPrincipal
            ? '<span class="badge bg-warning text-dark"><i class="bi bi-star-fill"></i> Principal</span>'
            : '<span class="text-muted">—</span>';

        let botonPrincipal = e.esPrincipal
            ? ''
            : `<button class="btn btn-warning btn-sm me-1" title="Definir como principal"
                       onclick="definirPrincipal(${e.idEncargado})">
                   <i class="bi bi-star"></i>
               </button>`;

        filas += `
            <tr>
                <td>${escapeHtml(e.apellidos)}, ${escapeHtml(e.nombre)}</td>
                <td>${escapeHtml(e.parentesco)}</td>
                <td>${escapeHtml(e.telefono ?? '—')}</td>
                <td>${escapeHtml(e.correo)}</td>
                <td>${badgePrincipal}</td>
                <td class="text-nowrap">
                    ${botonPrincipal}
                    <button class="btn btn-danger btn-sm" title="Quitar encargado"
                            onclick="desvincularEncargado(${e.idEncargado})">
                        <i class="bi bi-person-dash"></i>
                    </button>
                </td>
            </tr>`;
    });

    if (filas === '')
    {
        filas = `<tr>
                    <td colspan="6" class="text-center text-muted">
                        Este estudiante aún no tiene encargados asociados
                    </td>
                </tr>`;
    }

    $("#tblEncargados tbody").html(filas);
}


function buscarEncargados()
{
    idEncargadoSeleccionado = 0;

    $.ajax({
        url: '/Estudiante/BuscarEncargados',
        type: 'GET',
        data: {
            texto: $("#txtEncBuscar").val(),
            idEstudiante: idEstudianteActual()
        },

        success: function (response)
        {
            if (!response.esCorrecto)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let items = '';

            $.each(response.dato ?? [], function (i, e)
            {
                items += `
                    <button type="button" class="list-group-item list-group-item-action"
                            data-id="${e.idEncargado}">
                        <strong>${escapeHtml(e.nombreCompleto)}</strong>
                        <small class="text-muted ms-2">${escapeHtml(e.correo)}</small>
                        <small class="text-muted ms-2">${escapeHtml(e.telefono ?? '')}</small>
                    </button>`;
            });

            if (items === '')
            {
                items = '<div class="list-group-item text-muted">No se encontraron encargados disponibles</div>';
            }

            $("#lstEncResultados").html(items);

            $("#lstEncResultados button").on("click", function ()
            {
                $("#lstEncResultados button").removeClass("active");
                $(this).addClass("active");

                idEncargadoSeleccionado = parseInt($(this).data("id"), 10) || 0;
            });
        },

        error: function ()
        {
            mostrarMensaje('Error al buscar encargados.', 'danger');
        }
    });
}


function asociarEncargado()
{
    if (idEncargadoSeleccionado === 0)
    {
        mostrarMensaje('Busque y seleccione un encargado de la lista.', 'danger');
        return;
    }

    enviarEncargado('/Estudiante/VincularEncargado', {
        idEstudiante: idEstudianteActual(),
        idEncargado: idEncargadoSeleccionado,
        parentesco: $("#ddlEncParentescoExistente").val()
    }, "#btnEncAsociar");
}


function crearEncargado()
{
    enviarEncargado('/Estudiante/CrearEncargado', {
        idEstudiante: idEstudianteActual(),
        nombre: $("#txtEncNombre").val(),
        apellidos: $("#txtEncApellidos").val(),
        correo: $("#txtEncCorreo").val(),
        telefono: $("#txtEncTelefono").val(),
        parentesco: $("#ddlEncParentescoNuevo").val(),
        password: $("#txtEncPassword").val()
    }, "#btnEncCrear");
}


function enviarEncargado(url, datos, selectorBoton)
{
    let $btn = $(selectorBoton);

    $btn.prop("disabled", true);

    $.ajax({
        url: url,
        type: 'POST',
        data: datos,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                limpiarFormulariosEncargado();
                cargarEncargados();
                mostrarMensaje(response.mensaje, 'success');
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al guardar el encargado.', 'danger');
        },

        complete: function ()
        {
            $btn.prop("disabled", false);
        }
    });
}


function definirPrincipal(idEncargado)
{
    $.ajax({
        url: '/Estudiante/DefinirEncargadoPrincipal',
        type: 'POST',
        data: { idEstudiante: idEstudianteActual(), idEncargado: idEncargado },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                cargarEncargados();
                mostrarMensaje(response.mensaje, 'success');
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al definir el encargado principal.', 'danger');
        }
    });
}


async function desvincularEncargado(idEncargado)
{
    if (!(await confirmar(
        "¿Desea quitar este encargado del estudiante? Su cuenta de usuario no se elimina.",
        'Quitar',
        'danger')))
    {
        return;
    }

    $.ajax({
        url: '/Estudiante/DesvincularEncargado',
        type: 'POST',
        data: { idEstudiante: idEstudianteActual(), idEncargado: idEncargado },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                cargarEncargados();
                mostrarMensaje(response.mensaje, 'success');
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al quitar el encargado.', 'danger');
        }
    });
}
