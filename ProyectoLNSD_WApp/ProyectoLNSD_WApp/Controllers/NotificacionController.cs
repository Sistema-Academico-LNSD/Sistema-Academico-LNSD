using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Notificaciones;

namespace ProyectoLNSD_WApp.Controllers;

[Authorize]
public class NotificacionController(INotificacionService service) : Controller
{
    private int? UsuarioId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> ObtenerPanel()
    {
        if (UsuarioId is not int id) return Unauthorized();
        return Json(await service.ObtenerPanel(id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLeida(long idNotificacion)
    {
        if (UsuarioId is not int id) return Unauthorized();
        bool correcto = await service.MarcarLeida(id, idNotificacion);
        if (!correcto) return NotFound(new { esCorrecto = false, mensaje = "La notificación no existe o no pertenece a su cuenta." });
        return Json(new { esCorrecto = true });
    }

    [HttpGet]
    [Authorize(Roles = RolesSistema.Administrador)]
    public IActionResult Configuracion() => View();

    [HttpGet]
    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> ObtenerTipos() => Json(await service.ObtenerTipos());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> GuardarTipo([FromBody] NotificacionTipoGuardarDTO dto)
    {
        if (dto == null || dto.IdTipo <= 0) return BadRequest(new { esCorrecto = false, mensaje = "Configuración no válida." });
        bool correcto = await service.GuardarTipo(dto);
        return correcto
            ? Json(new { esCorrecto = true, mensaje = "Configuración guardada correctamente." })
            : BadRequest(new { esCorrecto = false, mensaje = "Seleccione al menos un rol destinatario y verifique el tipo." });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> CambiarEstado(int idTipo, bool activo)
    {
        bool correcto = await service.CambiarEstado(idTipo, activo);
        return correcto
            ? Json(new { esCorrecto = true, mensaje = activo ? "Tipo de notificación activado." : "Tipo de notificación desactivado." })
            : NotFound(new { esCorrecto = false, mensaje = "El tipo de notificación no existe." });
    }
}
