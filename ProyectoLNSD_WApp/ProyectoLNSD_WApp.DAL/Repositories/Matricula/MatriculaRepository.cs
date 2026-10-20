using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;
using EstudianteEntity = global::ProyectoLNSD_WApp.DAL.Entities.Estudiante;
using PeriodoLectivoEntity = global::ProyectoLNSD_WApp.DAL.Entities.PeriodoLectivo;
using SeccionEntity = global::ProyectoLNSD_WApp.DAL.Entities.Seccion;
using CursoEntity = global::ProyectoLNSD_WApp.DAL.Entities.Curso;
using MatriculaEntity = global::ProyectoLNSD_WApp.DAL.Entities.Matricula;

namespace ProyectoLNSD_WApp.DAL.Repositories.Matricula;

public class MatriculaRepository(AppDbContext context) : IMatriculaRepository
{
    private readonly AppDbContext _context = context;

    private IQueryable<MatriculaEntity> DetalleQuery() => _context.Matriculas
        .AsNoTracking()
        .Include(m => m.Estudiante).ThenInclude(e => e!.Usuario)
        .Include(m => m.Estudiante).ThenInclude(e => e!.EncargadosEstudiantes).ThenInclude(ee => ee.Encargado).ThenInclude(e => e!.Usuario)
        .Include(m => m.PeriodoLectivo)
        .Include(m => m.Grupo)
        .Include(m => m.Seccion).ThenInclude(s => s!.Grado)
        .Include(m => m.MatriculasCursos).ThenInclude(mc => mc.Curso);

    public Task<EstudianteEntity?> GetEstudiante(int id) => _context.Estudiantes.Include(e => e.Usuario).FirstOrDefaultAsync(e => e.IdEstudiante == id);
    public Task<PeriodoLectivoEntity?> GetPeriodo(int id) => _context.PeriodosLectivos.FirstOrDefaultAsync(p => p.IdPeriodo == id);
    public Task<Grupo?> GetGrupo(int id) => _context.Grupos.FirstOrDefaultAsync(g => g.IdGrupo == id);
    public Task<SeccionEntity?> GetSeccion(int id) => _context.Secciones.Include(s => s.Grado).FirstOrDefaultAsync(s => s.IdSeccion == id);
    public Task<bool> ExisteMatriculaPeriodo(int idEstudiante, int idPeriodo, int? excluirId = null) => _context.Matriculas.AnyAsync(m => m.IdEstudiante == idEstudiante && m.IdPeriodo == idPeriodo && m.Estado != "Anulada" && (!excluirId.HasValue || m.IdMatricula != excluirId.Value));
    public Task<int> ContarMatriculasActivasEnSeccion(int idSeccion, int idPeriodo, int? excluirId = null) => _context.Matriculas.CountAsync(m => m.IdSeccion == idSeccion && m.IdPeriodo == idPeriodo && m.Estado == "Activa" && (!excluirId.HasValue || m.IdMatricula != excluirId.Value));

    public Task<List<CursoEntity>> GetCursosValidosParaGrado(int idGrado, IEnumerable<int> idsCursos)
    {
        var ids = idsCursos.Distinct().ToArray();
        if (ids.Length == 0) return Task.FromResult(new List<CursoEntity>());
        return _context.CursosGrados.Where(cg => cg.IdGrado == idGrado && ids.Contains(cg.IdCurso))
            .Select(cg => cg.Curso!).Where(c => c.Estado).ToListAsync();
    }

    public Task<MatriculaEntity?> GetById(int id) => DetalleQuery().FirstOrDefaultAsync(m => m.IdMatricula == id);

    public async Task<List<MatriculaEntity>> Buscar(int? idEstudiante, string? identificacion, int? idGrado, int? idGrupo, int? idSeccion, int? idPeriodo, string? estado, string? texto, int limite)
    {
        var q = DetalleQuery();
        if (idEstudiante.HasValue) q = q.Where(m => m.IdEstudiante == idEstudiante.Value);
        if (!string.IsNullOrWhiteSpace(identificacion)) q = q.Where(m => m.Estudiante!.Identificacion.Contains(identificacion));
        if (idGrado.HasValue) q = q.Where(m => m.Seccion!.IdGrado == idGrado.Value);
        if (idGrupo.HasValue) q = q.Where(m => m.IdGrupo == idGrupo.Value);
        if (idSeccion.HasValue) q = q.Where(m => m.IdSeccion == idSeccion.Value);
        if (idPeriodo.HasValue) q = q.Where(m => m.IdPeriodo == idPeriodo.Value);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(m => m.Estado == estado);
        if (!string.IsNullOrWhiteSpace(texto)) q = q.Where(m => m.Estudiante!.Carnet.Contains(texto) || m.Estudiante.Identificacion.Contains(texto) || m.Estudiante.Usuario!.Nombre.Contains(texto) || m.Estudiante.Usuario.Apellido.Contains(texto));
        return await q.OrderByDescending(m => m.FechaMatricula).ThenByDescending(m => m.IdMatricula).Take(Math.Clamp(limite, 1, 1000)).ToListAsync();
    }

    public Task<List<MatriculaEntity>> HistorialEstudiante(int idEstudiante) => DetalleQuery().Where(m => m.IdEstudiante == idEstudiante).OrderByDescending(m => m.PeriodoLectivo!.FechaInicio).ThenByDescending(m => m.FechaMatricula).ToListAsync();
    public Task<List<MatriculaHistorial>> HistorialCambios(int idMatricula) => _context.MatriculasHistorial.AsNoTracking().Where(h => h.IdMatricula == idMatricula).OrderByDescending(h => h.FechaCambio).ToListAsync();
    public Task<List<MatriculaObservacion>> Observaciones(int idMatricula) => _context.MatriculasObservaciones.AsNoTracking().Where(o => o.IdMatricula == idMatricula).OrderByDescending(o => o.FechaRegistro).ThenByDescending(o => o.IdObservacion).ToListAsync();

    public Task<List<MatriculaEntity>> MatriculasDeUsuario(int idUsuario, bool esEncargado)
    {
        var q = DetalleQuery();
        return (esEncargado
            ? q.Where(m => m.Estudiante!.EncargadosEstudiantes.Any(ee => ee.Encargado!.IdUsuario == idUsuario))
            : q.Where(m => m.Estudiante!.IdUsuario == idUsuario))
            .OrderByDescending(m => m.FechaMatricula).ToListAsync();
    }

    public async Task GuardarNueva(MatriculaEntity matricula, IEnumerable<int> idsCursos, MatriculaHistorial historial)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();
        foreach (var idCurso in idsCursos.Distinct()) _context.MatriculasCursos.Add(new MatriculaCurso { IdMatricula = matricula.IdMatricula, IdCurso = idCurso });
        historial.IdMatricula = matricula.IdMatricula;
        _context.MatriculasHistorial.Add(historial);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task GuardarCambios(MatriculaEntity matricula, IEnumerable<int>? idsCursos, MatriculaHistorial historial)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        if (idsCursos != null)
        {
            var existentes = await _context.MatriculasCursos.Where(mc => mc.IdMatricula == matricula.IdMatricula).ToListAsync();
            _context.MatriculasCursos.RemoveRange(existentes);
            foreach (var idCurso in idsCursos.Distinct()) _context.MatriculasCursos.Add(new MatriculaCurso { IdMatricula = matricula.IdMatricula, IdCurso = idCurso });
        }
        // Adjuntar solo las columnas escalares evita marcar como modificadas las navegaciones incluidas en la consulta.
        var tracked = new MatriculaEntity
        {
            IdMatricula = matricula.IdMatricula,
            IdEstudiante = matricula.IdEstudiante,
            IdPeriodo = matricula.IdPeriodo,
            IdGrupo = matricula.IdGrupo,
            IdSeccion = matricula.IdSeccion,
            FechaMatricula = matricula.FechaMatricula,
            Estado = matricula.Estado,
            Observaciones = matricula.Observaciones
        };
        _context.Matriculas.Attach(tracked);
        _context.Entry(tracked).Property(x => x.IdPeriodo).IsModified = true;
        _context.Entry(tracked).Property(x => x.IdGrupo).IsModified = true;
        _context.Entry(tracked).Property(x => x.IdSeccion).IsModified = true;
        _context.Entry(tracked).Property(x => x.FechaMatricula).IsModified = true;
        _context.Entry(tracked).Property(x => x.Estado).IsModified = true;
        _context.Entry(tracked).Property(x => x.Observaciones).IsModified = true;
        historial.IdMatricula = matricula.IdMatricula;
        _context.MatriculasHistorial.Add(historial);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task AgregarObservacion(MatriculaObservacion observacion)
    {
        _context.MatriculasObservaciones.Add(observacion);
        await _context.SaveChangesAsync();
    }
}
