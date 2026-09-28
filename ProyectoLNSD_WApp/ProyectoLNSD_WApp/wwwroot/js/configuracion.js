$(document).ready(function () {
    cargarInstitucion();
 
    $("#btnGuardarInstitucion").click(function () {
        guardarInstitucion();
    });
});
 
// ---------------- HU 01: Institución ----------------
 
function cargarInstitucion() {
    $.ajax({
        url: '/Configuracion/GetInstitucion',
        type: 'GET',
 
        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }
 
            let d = response.dato;
            if (!d) return;   // aún no hay información registrada: se deja el formulario vacío
 
            $("#Nombre").val(d.nombre);
            $("#Direccion").val(d.direccion);
            $("#Telefono").val(d.telefono);
            $("#TelefonoSecundario").val(d.telefonoSecundario);
            $("#Correo").val(d.correo);
 
            if (d.rutaLogo) {
                $("#previewLogo").attr("src", d.rutaLogo).removeClass("d-none");
            }
            if (d.fechaActualizacion) {
                $("#lblActualizado").text("Última actualización: " +
                    new Date(d.fechaActualizacion + "Z").toLocaleString());
            }
        },
 
        error: function () {
            mostrarMensaje("Error al cargar la información institucional.", 'danger');
        }
    });
}
 
function guardarInstitucion() {
    // FormData incluye los campos, el archivo "logo" y el token antiforgery.
    let datos = new FormData($("#formInstitucion")[0]);
 
    $.ajax({
        url: '/Configuracion/GuardarInstitucion',
        type: 'POST',
        data: datos,
        processData: false,   // necesario para enviar archivos
        contentType: false,
 
        success: function (response) {
            if (response.esCorrecto) {
                $("#logo").val("");
                cargarInstitucion();
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },
 
        error: function () {
            mostrarMensaje("Error al guardar la información institucional.", 'danger');
        }
    });
}