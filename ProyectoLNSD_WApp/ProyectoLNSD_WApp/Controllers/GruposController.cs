using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.Controllers;

[Authorize]
public class GruposController(AppDbContext db) : Controller
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    [RequierePermiso("Grados", "Ver")]
    public IActionResult Index() => View();

    [HttpGet]
    [RequierePermiso("Grados", "Ver")]
    public async Task<IActionResult> Listar(int? anio)
    {
        var query = _db.Grupos.AsNoTracking();
        if (anio.HasValue) query = query.Where(g => g.Anio == anio.Value);
        var grupos = await query.OrderByDescending(g => g.Anio).ThenBy(g => g.Nivel).ThenBy(g => g.Nombre)
            .Select(g => new { idGrupo = g.IdGrupo, nombre = g.Nombre, nivel = g.Nivel, anio = g.Anio })
            .ToListAsync();
        return Json(new { esCorrecto = true, dato = grupos });
    }

    [HttpGet]
    [RequierePermiso("Grados", "Ver")]
    public async Task<IActionResult> Niveles()
    {
        var niveles = await _db.Grados.AsNoTracking().Where(g => g.Estado)
            .Select(g => g.Nivel).Distinct().OrderBy(n => n).ToListAsync();
        return Json(new { esCorrecto = true, dato = niveles });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequierePermiso("Grados", "Crear")]
    public async Task<IActionResult> Crear([FromForm] string nombre, [FromForm] string nivel, [FromForm] int anio)
    {
        nombre = (nombre ?? string.Empty).Trim();
        nivel = (nivel ?? string.Empty).Trim();
        if (nombre.Length is < 1 or > 50)
            return BadRequest(new { esCorrecto = false, mensaje = "El nombre debe tener entre 1 y 50 caracteres." });
        if (nivel.Length is < 1 or > 50)
            return BadRequest(new { esCorrecto = false, mensaje = "Seleccione un nivel académico válido." });
        if (anio < 2000 || anio > 2100)
            return BadRequest(new { esCorrecto = false, mensaje = "El año debe estar entre 2000 y 2100." });

        var nivelExiste = await _db.Grados.AsNoTracking().AnyAsync(g => g.Estado && g.Nivel.ToLower() == nivel.ToLower());
        if (!nivelExiste)
            return BadRequest(new { esCorrecto = false, mensaje = "Ese nivel no existe en los grados activos. Revise la configuración de Grados." });

        var duplicado = await _db.Grupos.AnyAsync(g => g.Anio == anio && g.Nombre.ToLower() == nombre.ToLower() && g.Nivel.ToLower() == nivel.ToLower());
        if (duplicado)
            return Conflict(new { esCorrecto = false, mensaje = $"Ya existe un grupo '{nombre}' para el nivel {nivel} y año {anio}. Los grupos A/B se reutilizan en las secciones de ese mismo nivel y año." });

        var grupo = new Grupo { Nombre = nombre, Nivel = nivel, Anio = anio };
        _db.Grupos.Add(grupo);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { esCorrecto = false, mensaje = "No se pudo guardar el grupo; compruebe que no exista ya otro con el mismo nombre y año." });
        }

        return Ok(new { esCorrecto = true, mensaje = "Grupo creado correctamente.", dato = new { idGrupo = grupo.IdGrupo, nombre = grupo.Nombre, nivel = grupo.Nivel, anio = grupo.Anio } });
    }
}
