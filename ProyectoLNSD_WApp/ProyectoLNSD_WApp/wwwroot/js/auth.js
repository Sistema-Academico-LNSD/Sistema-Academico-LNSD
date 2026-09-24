$(document).ready(function ()
{
    $("#formLogin").on("submit", function (e)
    {
        e.preventDefault();
        iniciarSesion();
    });

    $("#btnLogout").on("click", function (e)
    {
        e.preventDefault();
        cerrarSesion();
    });

    $("#formRecuperar").on("submit", function (e)
    {
        e.preventDefault();
        solicitarRecuperacion();
    });

    $("#formRestablecer").on("submit", function (e)
    {
        e.preventDefault();
        restablecerPassword();
    });
});

function iniciarSesion()
{
    let $btn = $("#btnLogin");
    let $alerta = $("#alertLogin");

    $alerta.addClass("d-none").text("");
    $btn.prop("disabled", true).text("Ingresando...");

    let datos = {
        Correo: $("#Correo").val(),
        Password: $("#Password").val(),
        returnUrl: $("input[name='returnUrl']").val(),
        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
    };

    $.ajax({
        url: '/Auth/Login',
        type: 'POST',
        data: datos,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                window.location.href = response.redirectUrl || '/';
            }
            else
            {
                $alerta.removeClass("d-none").text(response.mensaje || "No se pudo iniciar sesión.");
            }
        },

        error: function (xhr)
        {
            let mensaje = "Ocurrió un error al iniciar sesión.";

            if (xhr.status === 400)
            {
                mensaje = "Verifique los datos ingresados.";
            }

            $alerta.removeClass("d-none").text(mensaje);
        },

        complete: function ()
        {
            $btn.prop("disabled", false).text("Ingresar");
        }
    });
}

function cerrarSesion()
{
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        url: '/Auth/Logout',
        type: 'POST',
        data: { __RequestVerificationToken: token },

        complete: function ()
        {
            window.location.href = '/Auth/Login';
        }
    });
}

function solicitarRecuperacion()
{
    let $btn = $("#btnRecuperar");
    let $alerta = $("#alertRecuperar");

    $alerta.addClass("d-none").removeClass("alert-success alert-danger").text("");
    $btn.prop("disabled", true).text("Enviando...");

    let datos = {
        Correo: $("#Correo").val(),
        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
    };

    $.ajax({
        url: '/Auth/RecuperarPassword',
        type: 'POST',
        data: datos,

        success: function (response)
        {
            $alerta
                .removeClass("d-none")
                .addClass(response.esCorrecto ? "alert-success" : "alert-danger")
                .text(response.mensaje);

            if (response.esCorrecto)
            {
                $("#formRecuperar")[0].reset();
            }
        },

        error: function ()
        {
            $alerta
                .removeClass("d-none")
                .addClass("alert-danger")
                .text("Ocurrió un error al procesar la solicitud.");
        },

        complete: function ()
        {
            $btn.prop("disabled", false).text("Enviar enlace de recuperación");
        }
    });
}

function restablecerPassword()
{
    let $btn = $("#btnRestablecer");
    let $alerta = $("#alertRestablecer");

    $alerta.addClass("d-none").removeClass("alert-success alert-danger").text("");
    $btn.prop("disabled", true).text("Guardando...");

    let datos = {
        Token: $("#Token").val(),
        NuevaPassword: $("#NuevaPassword").val(),
        ConfirmarPassword: $("#ConfirmarPassword").val(),
        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
    };

    $.ajax({
        url: '/Auth/RestablecerPassword',
        type: 'POST',
        data: datos,

        success: function (response)
        {
            $alerta
                .removeClass("d-none")
                .addClass(response.esCorrecto ? "alert-success" : "alert-danger")
                .text(response.mensaje);

            if (response.esCorrecto)
            {
                setTimeout(function ()
                {
                    window.location.href = '/Auth/Login';
                }, 1800);
            }
        },

        error: function ()
        {
            $alerta
                .removeClass("d-none")
                .addClass("alert-danger")
                .text("Ocurrió un error al restablecer la contraseña.");
        },

        complete: function ()
        {
            $btn.prop("disabled", false).text("Restablecer contraseña");
        }
    });
}