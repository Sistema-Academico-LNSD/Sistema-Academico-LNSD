using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;
using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.Controllers;

[Authorize]
public class InformativoController(AppDbContext db) : Controller
{
    private bool EsGestor => User.IsInRole(RolesSistema.Administrador) || User.IsInRole(RolesSistema.Director);
    private int? UsuarioId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string tipo = "Todos", DateTime? desde = null, DateTime? hasta = null)
    {
        // Activa automáticamente publicaciones/eventos programados al llegar su fecha.
        var programados = await db.InformativoRegistros.Where(x => !x.Eliminado && x.Estado == "Programado" && x.FechaPublicacion <= DateTime.Now).ToListAsync();
        if (programados.Count > 0)
        {
            foreach (var registro in programados) registro.Estado = "Publicado";
            await db.SaveChangesAsync();
        }
        var hoy = DateTime.Today;
        var q = db.InformativoRegistros.AsNoTracking().Where(x => !x.Eliminado);
        if (!EsGestor)
            q = q.Where(x => x.Estado == "Publicado" && x.FechaPublicacion <= DateTime.Now &&
                (x.Tipo == "Anuncio" ? x.FechaVigencia == null || x.FechaVigencia >= hoy :
                 x.Tipo == "Evento" ? (x.FechaFin ?? x.FechaInicio) >= hoy : true));
        if (tipo != "Todos") q = q.Where(x => x.Tipo == tipo);
        if (desde.HasValue) q = q.Where(x => x.Tipo == "Anuncio" ? x.FechaPublicacion >= desde.Value.Date : (x.FechaFin ?? x.FechaInicio) >= desde.Value.Date);
        if (hasta.HasValue) q = q.Where(x => x.Tipo == "Anuncio" ? x.FechaPublicacion < hasta.Value.Date.AddDays(1) : x.FechaInicio < hasta.Value.Date.AddDays(1));
        var datos = tipo == "Evento"
            ? await q.OrderBy(x => x.FechaInicio).ThenBy(x => x.HoraInicio).ToListAsync()
            : await q.OrderByDescending(x => x.Tipo == "Anuncio" ? x.FechaPublicacion : x.FechaInicio).ThenBy(x => x.HoraInicio).ToListAsync();
        return Json(new { esCorrecto = true, dato = datos, puedeGestionar = EsGestor, puedeGestionarCalendario = EsGestor });
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(int id)
    {
        var item = await db.InformativoRegistros.AsNoTracking().FirstOrDefaultAsync(x => x.IdInformativo == id && !x.Eliminado);
        if (item == null) return NotFound(new { esCorrecto = false, mensaje = "El registro no fue encontrado." });
        if (!EsGestor && (item.Estado != "Publicado" || item.FechaPublicacion > DateTime.Now ||
            item.Tipo == "Anuncio" && item.FechaVigencia.HasValue && item.FechaVigencia.Value.Date < DateTime.Today ||
            item.Tipo == "Evento" && (item.FechaFin ?? item.FechaInicio).Date < DateTime.Today)) return Forbid();
        return Json(new { esCorrecto = true, dato = item });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar([FromBody] InformativoGuardarDTO dto)
    {
        if (!EsGestor) return Forbid();
        var tipo = (dto.Tipo ?? "").Trim();
        if (tipo is not ("Anuncio" or "Actividad" or "Evento")) return BadRequest(new { esCorrecto = false, mensaje = "Tipo de registro no válido." });
        if (string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Descripcion)) return BadRequest(new { esCorrecto = false, mensaje = "El título y la descripción son obligatorios." });
        if (dto.Titulo.Length > 180 || dto.Descripcion.Length > 4000) return BadRequest(new { esCorrecto = false, mensaje = "El título o la descripción exceden la longitud permitida." });
        if (dto.FechaPublicacion == default) return BadRequest(new { esCorrecto = false, mensaje = "La fecha de publicación es obligatoria." });
        if (tipo != "Anuncio" && dto.FechaInicio == default) return BadRequest(new { esCorrecto = false, mensaje = "La fecha de inicio es obligatoria." });
        if (dto.FechaFin.HasValue && dto.FechaFin.Value.Date < dto.FechaInicio.Date) return BadRequest(new { esCorrecto = false, mensaje = "La fecha final no puede ser anterior a la fecha inicial." });
        if (tipo == "Anuncio" && dto.FechaVigencia.HasValue && dto.FechaVigencia.Value.Date < dto.FechaPublicacion.Date) return BadRequest(new { esCorrecto = false, mensaje = "La vigencia no puede ser anterior a la fecha de publicación." });
        if (tipo == "Evento" && string.IsNullOrWhiteSpace(dto.Ubicacion)) return BadRequest(new { esCorrecto = false, mensaje = "La ubicación es obligatoria para los eventos." });
        if (tipo == "Evento" && string.IsNullOrWhiteSpace(dto.HoraInicio)) return BadRequest(new { esCorrecto = false, mensaje = "La hora del evento es obligatoria." });
        if (tipo == "Evento" && !TimeSpan.TryParse(dto.HoraInicio, out _)) return BadRequest(new { esCorrecto = false, mensaje = "La hora de inicio no tiene un formato válido." });
        if (!string.IsNullOrWhiteSpace(dto.HoraFin) && !TimeSpan.TryParse(dto.HoraFin, out _)) return BadRequest(new { esCorrecto = false, mensaje = "La hora final no tiene un formato válido." });
        if (TimeSpan.TryParse(dto.HoraInicio, out var horaInicioValidada) && TimeSpan.TryParse(dto.HoraFin, out var horaFinValidada) && horaFinValidada < horaInicioValidada)
            return BadRequest(new { esCorrecto = false, mensaje = "La hora final no puede ser anterior a la hora de inicio." });

        TimeSpan? horaEvento = TimeSpan.TryParse(dto.HoraInicio, out var horaCoincidente) ? horaCoincidente : null;
        var ubicacionEvento = (dto.Ubicacion ?? "").Trim().ToLower();
        var tituloNormalizado = dto.Titulo.Trim().ToLower();
        var descripcionNormalizada = dto.Descripcion.Trim().ToLower();
        var duplicado = await db.InformativoRegistros.AnyAsync(x => !x.Eliminado && x.IdInformativo != dto.IdInformativo && x.Tipo == tipo &&
            ((x.Titulo.ToLower() == tituloNormalizado && x.Descripcion.ToLower() == descripcionNormalizada &&
              (tipo == "Anuncio" ? x.FechaVigencia == null || x.FechaVigencia >= DateTime.Today : x.FechaInicio.Date == dto.FechaInicio.Date)) ||
             (tipo == "Evento" && x.FechaInicio.Date == dto.FechaInicio.Date && x.HoraInicio == horaEvento &&
              x.Ubicacion != null && x.Ubicacion.ToLower() == ubicacionEvento)));
        if (duplicado && !dto.ConfirmarDuplicado) return Json(new { esCorrecto = false, duplicado = true, mensaje = "Ya existe un registro similar vigente. ¿Desea continuar?" });

        var estado = dto.Estado == "Borrador" ? "Borrador" : dto.FechaPublicacion > DateTime.Now ? "Programado" : "Publicado";
        var fechas = new List<DateTime> { dto.FechaInicio };
        if (dto.IdInformativo == 0 && tipo == "Actividad" && dto.Recurrencia != "Ninguna" && dto.RepetirHasta.HasValue)
        {
            var cursor = dto.FechaInicio.Date;
            var limite = dto.RepetirHasta.Value.Date;
            if (limite < cursor || (limite - cursor).TotalDays > 730) return BadRequest(new { esCorrecto = false, mensaje = "El rango de recurrencia no es válido (máximo dos años)." });
            while (fechas.Count < 100)
            {
                cursor = dto.Recurrencia switch { "Diaria" => cursor.AddDays(1), "Semanal" => cursor.AddDays(7), "Mensual" => cursor.AddMonths(1), _ => limite.AddDays(1) };
                if (cursor > limite) break;
                fechas.Add(cursor);
            }
            var siguiente = dto.Recurrencia switch { "Diaria" => cursor.AddDays(1), "Semanal" => cursor.AddDays(7), "Mensual" => cursor.AddMonths(1), _ => limite.AddDays(1) };
            if (fechas.Count == 100 && siguiente <= limite) return BadRequest(new { esCorrecto = false, mensaje = "La recurrencia supera el máximo de 100 ocurrencias." });
        }
        if (dto.IdInformativo > 0)
        {
            var item = await db.InformativoRegistros.FirstOrDefaultAsync(x => x.IdInformativo == dto.IdInformativo && !x.Eliminado);
            if (item == null) return NotFound(new { esCorrecto = false, mensaje = "El registro ya no existe." });
            Aplicar(item, dto, tipo, estado);
        }
        else
        {
            foreach (var fecha in fechas)
            {
                var item = new InformativoRegistro();
                var duracion = dto.FechaFin.HasValue ? dto.FechaFin.Value.Date - dto.FechaInicio.Date : TimeSpan.Zero;
                var fechaFinOcurrencia = dto.FechaFin.HasValue ? fecha.AddDays(duracion.TotalDays) : (DateTime?)null;
                var copia = new InformativoGuardarDTO { Tipo = dto.Tipo, Titulo = dto.Titulo, Descripcion = dto.Descripcion, FechaPublicacion = dto.FechaPublicacion, FechaVigencia = dto.FechaVigencia, FechaInicio = fecha, FechaFin = fechaFinOcurrencia, HoraInicio = dto.HoraInicio, HoraFin = dto.HoraFin, Ubicacion = dto.Ubicacion, Categoria = dto.Categoria, Estado = dto.Estado };
                Aplicar(item, copia, tipo, estado); item.IdUsuarioCrea = UsuarioId; db.InformativoRegistros.Add(item);
            }
        }
        await db.SaveChangesAsync();
        return Json(new { esCorrecto = true, mensaje = dto.IdInformativo > 0 ? "Registro actualizado correctamente." : fechas.Count > 1 ? $"Se registraron {fechas.Count} ocurrencias." : estado == "Programado" ? "Publicación programada correctamente." : "Registro guardado correctamente." });
    }

    private static void Aplicar(InformativoRegistro item, InformativoGuardarDTO dto, string tipo, string estado)
    {
        item.Tipo = tipo; item.Titulo = dto.Titulo.Trim(); item.Descripcion = dto.Descripcion.Trim();
        item.FechaPublicacion = dto.FechaPublicacion; item.FechaVigencia = dto.FechaVigencia;
        item.FechaInicio = dto.FechaInicio; item.FechaFin = dto.FechaFin;
        item.HoraInicio = TimeSpan.TryParse(dto.HoraInicio, CultureInfo.InvariantCulture, out var hi) ? hi : null;
        item.HoraFin = TimeSpan.TryParse(dto.HoraFin, CultureInfo.InvariantCulture, out var hf) ? hf : null;
        item.Ubicacion = string.IsNullOrWhiteSpace(dto.Ubicacion) ? null : dto.Ubicacion.Trim();
        item.Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? null : dto.Categoria.Trim();
        item.Estado = estado;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (!EsGestor) return Forbid();
        var item = await db.InformativoRegistros.FirstOrDefaultAsync(x => x.IdInformativo == id && !x.Eliminado);
        if (item == null) return NotFound(new { esCorrecto = false, mensaje = "El registro no fue encontrado." });
        item.Eliminado = true; await db.SaveChangesAsync();
        return Json(new { esCorrecto = true, mensaje = "Registro retirado correctamente." });
    }
}
