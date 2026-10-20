using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO.Matricula;
using ProyectoLNSD_WApp.BLL.Service.Matricula;
using ProyectoLNSD_WApp.BLL.Service.PeriodoLectivo;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.Controllers;

[ApiController]
[Route("api/matriculas")]
[Authorize]
public class MatriculaController(IMatriculaService service, IPeriodoLectivoService periodoService, AppDbContext db) : ControllerBase
{
    private readonly IMatriculaService _service = service;
    private readonly IPeriodoLectivoService _periodoService = periodoService;
    private readonly AppDbContext _db = db;
    private int? IdUsuarioActual => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet("catalogos")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> Catalogos()
    {
        var estudiantes = await _db.Estudiantes.AsNoTracking()
            .Where(e => e.Estado && e.Usuario != null && e.Usuario.Estado)
            .OrderBy(e => e.Usuario!.Apellido).ThenBy(e => e.Usuario!.Nombre)
            .Select(e => new
            {
                idEstudiante = e.IdEstudiante,
                nombreCompleto = e.Usuario!.Nombre + " " + e.Usuario.Apellido,
                identificacion = e.Identificacion,
                carnet = e.Carnet,
                idGrado = e.IdGrado
            }).ToListAsync();

        var grados = await _db.Grados.AsNoTracking().Where(g => g.Estado)
            .OrderBy(g => g.Nombre)
            .Select(g => new { idGrado = g.IdGrado, nombre = g.Nombre, codigo = g.Codigo, nivel = g.Nivel })
            .ToListAsync();

        var secciones = await _db.Secciones.AsNoTracking()
            .Where(s => s.Estado && s.Grado != null && s.Grado.Estado)
            .OrderBy(s => s.Grado!.Nombre).ThenBy(s => s.Nombre)
            .Select(s => new
            {
                idSeccion = s.IdSeccion,
                nombre = s.Nombre,
                idGrado = s.IdGrado,
                nombreGrado = s.Grado!.Nombre,
                codigoGrado = s.Grado.Codigo,
                nivel = s.Grado.Nivel,
                capacidadMaxima = s.CapacidadMaxima
            }).ToListAsync();

        var grupos = await _db.Grupos.AsNoTracking()
            .OrderByDescending(g => g.Anio).ThenBy(g => g.Nivel).ThenBy(g => g.Nombre)
            .Select(g => new { idGrupo = g.IdGrupo, nombre = g.Nombre, nivel = g.Nivel, anio = g.Anio })
            .ToListAsync();

        var cursos = await _db.Cursos.AsNoTracking().Where(c => c.Estado)
            .OrderBy(c => c.Nombre)
            .Select(c => new
            {
                idCurso = c.IdCurso,
                codigo = c.Codigo,
                nombre = c.Nombre,
                idsGrados = c.CursosGrados.Select(cg => cg.IdGrado).ToList()
            }).ToListAsync();

        return Ok(new { esCorrecto = true, mensaje = "Catálogos cargados", dato = new { estudiantes, grados, secciones, grupos, cursos } });
    }

    [HttpGet("periodos")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> PeriodosDisponibles()
    {
        var r = await _periodoService.GetPeriodos();
        return StatusCode(r.Codigo, r);
    }

    [HttpPost]
    [RequierePermiso("Matricula", "Crear")]
    public async Task<IActionResult> Crear([FromBody] MatriculaCrearDTO dto)
    {
        var r = await _service.Crear(dto, IdUsuarioActual);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet("{id:int}")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> Obtener(int id)
    {
        var r = await _service.GetById(id);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> Buscar([FromQuery] MatriculaFiltroDTO filtro)
    {
        var r = await _service.Buscar(filtro);
        return StatusCode(r.Codigo, r);
    }

    [HttpPut("{id:int}")]
    [RequierePermiso("Matricula", "Editar")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] MatriculaActualizarDTO dto)
    {
        var r = await _service.Actualizar(id, dto, IdUsuarioActual);
        return StatusCode(r.Codigo, r);
    }

    [HttpPut("{id:int}/cursos")]
    [RequierePermiso("Matricula", "Editar")]
    public async Task<IActionResult> AsignarCursos(int id, [FromBody] List<int> idsCursos)
    {
        var r = await _service.AsignarCursos(id, idsCursos, IdUsuarioActual);
        return StatusCode(r.Codigo, r);
    }

    [HttpPatch("{id:int}/estado")]
    [RequierePermiso("Matricula", "Editar")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] MatriculaEstadoDTO dto)
    {
        var r = await _service.CambiarEstado(id, dto, IdUsuarioActual);
        return StatusCode(r.Codigo, r);
    }

    [HttpPost("{id:int}/traslados")]
    [RequierePermiso("Matricula", "Editar")]
    public async Task<IActionResult> Trasladar(int id, [FromBody] MatriculaTrasladoDTO dto)
    {
        var r = await _service.Trasladar(id, dto, IdUsuarioActual);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet("estudiante/{idEstudiante:int}/historial")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> HistorialEstudiante(int idEstudiante)
    {
        var r = await _service.HistorialEstudiante(idEstudiante);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet("{id:int}/historial-cambios")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> HistorialCambios(int id)
    {
        var r = await _service.HistorialCambios(id);
        return StatusCode(r.Codigo, r);
    }

    [HttpPost("{id:int}/observaciones")]
    [RequierePermiso("Matricula", "Editar")]
    public async Task<IActionResult> AgregarObservacion(int id, [FromBody] MatriculaObservacionCrearDTO dto)
    {
        var r = await _service.AgregarObservacion(id, dto);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet("{id:int}/observaciones")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> Observaciones(int id)
    {
        var r = await _service.Observaciones(id);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet("reporte")]
    [RequierePermiso("Matricula", "Ver")]
    public async Task<IActionResult> Reporte([FromQuery] MatriculaFiltroDTO filtro)
    {
        var r = await _service.Reporte(filtro);
        return StatusCode(r.Codigo, r);
    }

    // El estudiante y el encargado consultan únicamente matrículas relacionadas con su cuenta autenticada.
    [HttpGet("mi-estado")]
    [Authorize(Roles = "Estudiante")]
    public async Task<IActionResult> MiEstado()
    {
        if (!IdUsuarioActual.HasValue) return Unauthorized();
        var r = await _service.ConsultarPropias(IdUsuarioActual.Value, false);
        return StatusCode(r.Codigo, r);
    }

    [HttpGet("mis-estudiantes")]
    [Authorize(Roles = "Encargado")]
    public async Task<IActionResult> MisEstudiantes()
    {
        if (!IdUsuarioActual.HasValue) return Unauthorized();
        var r = await _service.ConsultarPropias(IdUsuarioActual.Value, true);
        return StatusCode(r.Codigo, r);
    }
}
