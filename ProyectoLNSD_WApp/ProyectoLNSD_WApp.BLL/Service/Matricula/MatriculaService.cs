using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.Matricula;
using ProyectoLNSD_WApp.DAL.Entities;
using MatriculaEntity = global::ProyectoLNSD_WApp.DAL.Entities.Matricula;
using ProyectoLNSD_WApp.DAL.Repositories.Matricula;

namespace ProyectoLNSD_WApp.BLL.Service.Matricula;

public class MatriculaService(IMatriculaRepository repository) : IMatriculaService
{
    private readonly IMatriculaRepository _repo = repository;
    private static readonly string[] Estados = ["Activa", "Inactiva", "Anulada", "Pendiente"];

    public async Task<RespuestaDTO<MatriculaDetalleDTO>> Crear(MatriculaCrearDTO dto, int? idUsuario)
    {
        var ids = NormalizarCursos(dto.IdCursos);
        var validacion = await ValidarDestino(dto.IdPeriodo, dto.IdGrupo, dto.IdSeccion, dto.IdEstudiante, null, ids, comprobarCapacidad: true);
        if (validacion.Error != null) return RespuestaDTO<MatriculaDetalleDTO>.Error(validacion.Error, validacion.Codigo);
        var m = new MatriculaEntity { IdEstudiante = dto.IdEstudiante, IdPeriodo = dto.IdPeriodo, IdGrupo = dto.IdGrupo, IdSeccion = dto.IdSeccion,
            FechaMatricula = dto.FechaMatricula ?? DateOnly.FromDateTime(DateTime.Today), Estado = "Activa", Observaciones = Limpiar(dto.Observaciones) };
        var h = NuevoHistorial(idUsuario, "Registro", null, "Activa", null, dto.IdGrupo, null, dto.IdSeccion, "Matrícula registrada.");
        try { await _repo.GuardarNueva(m, ids, h); }
        catch (DbUpdateException) { return RespuestaDTO<MatriculaDetalleDTO>.Error("Ya existe una matrícula para ese estudiante y período, o los datos incumplen una restricción de la base de datos.", 409); }
        var creada = await _repo.GetById(m.IdMatricula);
        return creada == null ? RespuestaDTO<MatriculaDetalleDTO>.Error("La matrícula se guardó, pero no se pudo recuperar.", 500) : RespuestaDTO<MatriculaDetalleDTO>.Exito(Mapear(creada), "Matrícula registrada.");
    }

    public async Task<RespuestaDTO<MatriculaDetalleDTO?>> GetById(int idMatricula)
    {
        var m = await _repo.GetById(idMatricula);
        return m == null ? RespuestaDTO<MatriculaDetalleDTO?>.Error("Matrícula no encontrada.", 404) : RespuestaDTO<MatriculaDetalleDTO?>.Exito(Mapear(m));
    }

    public async Task<RespuestaDTO<List<MatriculaDetalleDTO>>> Buscar(MatriculaFiltroDTO f)
    {
        if (!string.IsNullOrWhiteSpace(f.Estado) && !Estados.Contains(f.Estado, StringComparer.OrdinalIgnoreCase)) return RespuestaDTO<List<MatriculaDetalleDTO>>.Error("Estado de matrícula no válido.", 400);
        var lista = await _repo.Buscar(f.IdEstudiante, Limpiar(f.Identificacion), f.IdGrado, f.IdGrupo, f.IdSeccion, f.IdPeriodo, Limpiar(f.Estado), Limpiar(f.Texto), f.Limite);
        return RespuestaDTO<List<MatriculaDetalleDTO>>.Exito(lista.Select(Mapear).ToList());
    }

    public async Task<RespuestaDTO<MatriculaDetalleDTO>> Actualizar(int id, MatriculaActualizarDTO dto, int? idUsuario)
    {
        var m = await _repo.GetById(id);
        if (m == null) return RespuestaDTO<MatriculaDetalleDTO>.Error("Matrícula no encontrada.", 404);
        if (m.Estado is "Anulada") return RespuestaDTO<MatriculaDetalleDTO>.Error("No se puede editar una matrícula anulada.", 409);
        var ids = NormalizarCursos(dto.IdCursos);
        var v = await ValidarDestino(dto.IdPeriodo, dto.IdGrupo, dto.IdSeccion, m.IdEstudiante, id, ids, comprobarCapacidad: true);
        if (v.Error != null) return RespuestaDTO<MatriculaDetalleDTO>.Error(v.Error, v.Codigo);
        var anteriorEstado = m.Estado; var grupoAnterior = m.IdGrupo; var seccionAnterior = m.IdSeccion;
        m.IdPeriodo = dto.IdPeriodo; m.IdGrupo = dto.IdGrupo; m.IdSeccion = dto.IdSeccion;
        if (dto.FechaMatricula.HasValue) m.FechaMatricula = dto.FechaMatricula.Value;
        m.Observaciones = Limpiar(dto.Observaciones);
        var h = NuevoHistorial(idUsuario, "Edicion", anteriorEstado, m.Estado, grupoAnterior, m.IdGrupo, seccionAnterior, m.IdSeccion, "Se actualizó la matrícula y la asignación de cursos.");
        try { await _repo.GuardarCambios(m, ids, h); }
        catch (DbUpdateException) { return RespuestaDTO<MatriculaDetalleDTO>.Error("No se pudo actualizar: revise duplicidad por período y restricciones de datos.", 409); }
        return await ObtenerActualizada(id);
    }

    public async Task<RespuestaDTO<MatriculaDetalleDTO>> AsignarCursos(int id, List<int> idsCursos, int? idUsuario)
    {
        var m = await _repo.GetById(id);
        if (m == null) return RespuestaDTO<MatriculaDetalleDTO>.Error("Matrícula no encontrada.", 404);
        if (m.Estado != "Activa") return RespuestaDTO<MatriculaDetalleDTO>.Error("Solo se asignan cursos a matrículas activas.", 409);
        var ids = NormalizarCursos(idsCursos);
        var cursos = await _repo.GetCursosValidosParaGrado(m.Seccion!.IdGrado, ids);
        if (cursos.Count != ids.Count) return RespuestaDTO<MatriculaDetalleDTO>.Error("Hay cursos inexistentes, inactivos o incompatibles con el grado.", 400);
        var h = NuevoHistorial(idUsuario, "Edicion", m.Estado, m.Estado, m.IdGrupo, m.IdGrupo, m.IdSeccion, m.IdSeccion, $"Asignación de cursos actualizada. Total: {ids.Count}.");
        await _repo.GuardarCambios(m, ids, h);
        return await ObtenerActualizada(id);
    }

    public async Task<RespuestaDTO<MatriculaDetalleDTO>> CambiarEstado(int id, MatriculaEstadoDTO dto, int? idUsuario)
    {
        var m = await _repo.GetById(id);
        if (m == null) return RespuestaDTO<MatriculaDetalleDTO>.Error("Matrícula no encontrada.", 404);
        var nuevo = Estados.FirstOrDefault(e => e.Equals(dto.Estado, StringComparison.OrdinalIgnoreCase));
        if (nuevo == null) return RespuestaDTO<MatriculaDetalleDTO>.Error("Estado no válido.", 400);
        if (m.Estado == "Anulada") return RespuestaDTO<MatriculaDetalleDTO>.Error("Una matrícula anulada es terminal y no puede reactivarse.", 409);
        if (nuevo == m.Estado) return RespuestaDTO<MatriculaDetalleDTO>.Exito(Mapear(m), "La matrícula ya se encuentra en ese estado.");
        if (nuevo == "Anulada" && string.IsNullOrWhiteSpace(dto.Motivo)) return RespuestaDTO<MatriculaDetalleDTO>.Error("Debe indicar el motivo de anulación.", 400);
        if (nuevo == "Activa")
        {
            if (await _repo.ExisteMatriculaPeriodo(m.IdEstudiante, m.IdPeriodo, id)) return RespuestaDTO<MatriculaDetalleDTO>.Error("Ya existe otra matrícula para el estudiante y período.", 409);
            var seccion = await _repo.GetSeccion(m.IdSeccion);
            if (seccion == null || await _repo.ContarMatriculasActivasEnSeccion(m.IdSeccion, m.IdPeriodo, id) >= seccion.CapacidadMaxima) return RespuestaDTO<MatriculaDetalleDTO>.Error("No hay capacidad disponible en la sección.", 409);
        }
        var old = m.Estado; m.Estado = nuevo;
        var h = NuevoHistorial(idUsuario, "Estado", old, nuevo, m.IdGrupo, m.IdGrupo, m.IdSeccion, m.IdSeccion, Limpiar(dto.Motivo) ?? $"Estado cambiado de {old} a {nuevo}.");
        await _repo.GuardarCambios(m, null, h);
        return await ObtenerActualizada(id);
    }

    public async Task<RespuestaDTO<MatriculaDetalleDTO>> Trasladar(int id, MatriculaTrasladoDTO dto, int? idUsuario)
    {
        var m = await _repo.GetById(id);
        if (m == null) return RespuestaDTO<MatriculaDetalleDTO>.Error("Matrícula no encontrada.", 404);
        if (m.Estado != "Activa") return RespuestaDTO<MatriculaDetalleDTO>.Error("Solo se trasladan matrículas activas.", 409);
        if (string.IsNullOrWhiteSpace(dto.Motivo)) return RespuestaDTO<MatriculaDetalleDTO>.Error("Debe indicar el motivo del traslado.", 400);
        var destino = await _repo.GetSeccion(dto.IdSeccion); var grupo = await _repo.GetGrupo(dto.IdGrupo);
        if (destino == null || !destino.Estado || destino.Grado == null || !destino.Grado.Estado || grupo == null) return RespuestaDTO<MatriculaDetalleDTO>.Error("El grupo o sección destino no existe o está inactivo.", 404);
        if (!string.Equals(grupo.Nivel.Trim(), destino.Grado.Nivel.Trim(), StringComparison.OrdinalIgnoreCase)) return RespuestaDTO<MatriculaDetalleDTO>.Error("El grupo no corresponde al nivel de la sección destino.", 400);
        if (grupo.Anio != m.PeriodoLectivo!.FechaInicio.Year) return RespuestaDTO<MatriculaDetalleDTO>.Error("El año del grupo no coincide con el período lectivo.", 400);
        if (await _repo.ContarMatriculasActivasEnSeccion(dto.IdSeccion, m.IdPeriodo, id) >= destino.CapacidadMaxima) return RespuestaDTO<MatriculaDetalleDTO>.Error("La sección destino alcanzó su capacidad máxima.", 409);
        var cursos = m.MatriculasCursos.Select(c => c.IdCurso).ToList();
        if ((await _repo.GetCursosValidosParaGrado(destino.IdGrado, cursos)).Count != cursos.Count) return RespuestaDTO<MatriculaDetalleDTO>.Error("El traslado no es válido porque hay cursos incompatibles con el grado destino. Actualice los cursos primero.", 409);
        var ga = m.IdGrupo; var sa = m.IdSeccion; m.IdGrupo = dto.IdGrupo; m.IdSeccion = dto.IdSeccion;
        var h = NuevoHistorial(idUsuario, "Traslado", m.Estado, m.Estado, ga, m.IdGrupo, sa, m.IdSeccion, dto.Motivo.Trim());
        await _repo.GuardarCambios(m, null, h);
        return await ObtenerActualizada(id);
    }

    public async Task<RespuestaDTO<List<MatriculaDetalleDTO>>> HistorialEstudiante(int idEstudiante)
    {
        if (await _repo.GetEstudiante(idEstudiante) == null) return RespuestaDTO<List<MatriculaDetalleDTO>>.Error("Estudiante no encontrado.", 404);
        return RespuestaDTO<List<MatriculaDetalleDTO>>.Exito((await _repo.HistorialEstudiante(idEstudiante)).Select(Mapear).ToList());
    }

    public async Task<RespuestaDTO<List<MatriculaHistorialDTO>>> HistorialCambios(int id)
    {
        if (await _repo.GetById(id) == null) return RespuestaDTO<List<MatriculaHistorialDTO>>.Error("Matrícula no encontrada.", 404);
        var lista = (await _repo.HistorialCambios(id)).Select(h => new MatriculaHistorialDTO { IdHistorial=h.IdHistorial, IdMatricula=h.IdMatricula, IdUsuario=h.IdUsuario, TipoCambio=h.TipoCambio, EstadoAnterior=h.EstadoAnterior, EstadoNuevo=h.EstadoNuevo, GrupoAnterior=h.GrupoAnterior, GrupoNuevo=h.GrupoNuevo, SeccionAnterior=h.SeccionAnterior, SeccionNueva=h.SeccionNueva, Detalle=h.Detalle, FechaCambio=h.FechaCambio }).ToList();
        return RespuestaDTO<List<MatriculaHistorialDTO>>.Exito(lista);
    }

    public async Task<RespuestaDTO<MatriculaObservacionDTO>> AgregarObservacion(int id, MatriculaObservacionCrearDTO dto)
    {
        if (await _repo.GetById(id) == null) return RespuestaDTO<MatriculaObservacionDTO>.Error("Matrícula no encontrada.", 404);
        if (string.IsNullOrWhiteSpace(dto.Observacion)) return RespuestaDTO<MatriculaObservacionDTO>.Error("La observación no puede estar vacía.", 400);
        var o = new MatriculaObservacion { IdMatricula=id, Observacion=dto.Observacion.Trim(), FechaRegistro=DateOnly.FromDateTime(DateTime.Today) };
        await _repo.AgregarObservacion(o);
        return RespuestaDTO<MatriculaObservacionDTO>.Exito(new MatriculaObservacionDTO { IdObservacion=o.IdObservacion, IdMatricula=o.IdMatricula, Observacion=o.Observacion, FechaRegistro=o.FechaRegistro }, "Observación registrada.");
    }

    public async Task<RespuestaDTO<List<MatriculaObservacionDTO>>> Observaciones(int id)
    {
        if (await _repo.GetById(id) == null) return RespuestaDTO<List<MatriculaObservacionDTO>>.Error("Matrícula no encontrada.", 404);
        var lista=(await _repo.Observaciones(id)).Select(o=>new MatriculaObservacionDTO { IdObservacion=o.IdObservacion, IdMatricula=o.IdMatricula, Observacion=o.Observacion, FechaRegistro=o.FechaRegistro }).ToList();
        return RespuestaDTO<List<MatriculaObservacionDTO>>.Exito(lista);
    }

    public async Task<RespuestaDTO<MatriculaReporteDTO>> Reporte(MatriculaFiltroDTO f)
    {
        var filtro = await _repo.Buscar(f.IdEstudiante, Limpiar(f.Identificacion), f.IdGrado, f.IdGrupo, f.IdSeccion, f.IdPeriodo, Limpiar(f.Estado), Limpiar(f.Texto), 1000);
        return RespuestaDTO<MatriculaReporteDTO>.Exito(new MatriculaReporteDTO { Total=filtro.Count, Activas=filtro.Count(m=>m.Estado=="Activa"), Inactivas=filtro.Count(m=>m.Estado=="Inactiva"), Anuladas=filtro.Count(m=>m.Estado=="Anulada"), Pendientes=filtro.Count(m=>m.Estado=="Pendiente"), Matriculas=filtro.Select(Mapear).ToList() });
    }

    public async Task<RespuestaDTO<List<MatriculaDetalleDTO>>> ConsultarPropias(int idUsuario, bool esEncargado)
    {
        var lista = await _repo.MatriculasDeUsuario(idUsuario, esEncargado);
        return RespuestaDTO<List<MatriculaDetalleDTO>>.Exito(lista.Select(Mapear).ToList());
    }

    private async Task<(string? Error, int Codigo)> ValidarDestino(int idPeriodo, int idGrupo, int idSeccion, int idEstudiante, int? excluirId, List<int> cursos, bool comprobarCapacidad)
    {
        var estudiante = await _repo.GetEstudiante(idEstudiante);
        if (estudiante == null || !estudiante.Estado || estudiante.EstadoAcademico is "Retirado" or "Egresado") return ("El estudiante no existe, está inactivo o no puede matricularse por su estado académico.", 400);
        var periodo = await _repo.GetPeriodo(idPeriodo);
        if (periodo == null || !periodo.Activo) return ("El período lectivo no existe o no está activo.", 400);
        if (await _repo.ExisteMatriculaPeriodo(idEstudiante, idPeriodo, excluirId)) return ("El estudiante ya tiene matrícula para ese período lectivo.", 409);
        var grupo = await _repo.GetGrupo(idGrupo); var seccion = await _repo.GetSeccion(idSeccion);
        if (grupo == null || seccion == null || !seccion.Estado || seccion.Grado == null || !seccion.Grado.Estado) return ("El grupo o la sección no existe o está inactivo.", 404);
        if (!string.Equals(grupo.Nivel.Trim(), seccion.Grado.Nivel.Trim(), StringComparison.OrdinalIgnoreCase)) return ("El grupo no corresponde al nivel de la sección.", 400);
        if (grupo.Anio != periodo.FechaInicio.Year) return ("El año del grupo no coincide con el período lectivo.", 400);
        if (comprobarCapacidad && await _repo.ContarMatriculasActivasEnSeccion(idSeccion, idPeriodo, excluirId) >= seccion.CapacidadMaxima) return ("La sección alcanzó su capacidad máxima.", 409);
        if ((await _repo.GetCursosValidosParaGrado(seccion.IdGrado, cursos)).Count != cursos.Count) return ("Hay cursos inexistentes, inactivos o incompatibles con el grado.", 400);
        return (null, 200);
    }

    private async Task<RespuestaDTO<MatriculaDetalleDTO>> ObtenerActualizada(int id)
    {
        var m = await _repo.GetById(id);
        return m == null ? RespuestaDTO<MatriculaDetalleDTO>.Error("No se pudo recuperar la matrícula actualizada.", 500) : RespuestaDTO<MatriculaDetalleDTO>.Exito(Mapear(m));
    }

    private static List<int> NormalizarCursos(IEnumerable<int>? ids) => (ids ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();
    private static string? Limpiar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static MatriculaHistorial NuevoHistorial(int? user, string tipo, string? estadoA, string? estadoN, int? grupoA, int? grupoN, int? seccionA, int? seccionN, string detalle) => new() { IdUsuario=user, TipoCambio=tipo, EstadoAnterior=estadoA, EstadoNuevo=estadoN, GrupoAnterior=grupoA, GrupoNuevo=grupoN, SeccionAnterior=seccionA, SeccionNueva=seccionN, Detalle=detalle.Length > 1000 ? detalle[..1000] : detalle };

    private static MatriculaDetalleDTO Mapear(MatriculaEntity m)
    {
        var principal = m.Estudiante?.EncargadosEstudiantes?.FirstOrDefault(e => e.EsPrincipal)?.Encargado?.Usuario;
        return new MatriculaDetalleDTO { IdMatricula=m.IdMatricula, IdEstudiante=m.IdEstudiante,
            NombreEstudiante=string.Join(" ", new[] {m.Estudiante?.Usuario?.Nombre,m.Estudiante?.Usuario?.Apellido}.Where(s=>!string.IsNullOrWhiteSpace(s))),
            Identificacion=m.Estudiante?.Identificacion ?? "", Carnet=m.Estudiante?.Carnet ?? "", IdPeriodo=m.IdPeriodo, NombrePeriodo=m.PeriodoLectivo?.Nombre ?? "",
            IdGrupo=m.IdGrupo, NombreGrupo=m.Grupo?.Nombre ?? "", IdSeccion=m.IdSeccion, NombreSeccion=m.Seccion?.Nombre ?? "", IdGrado=m.Seccion?.IdGrado,
            NombreGrado=m.Seccion?.Grado?.Nombre ?? "", EncargadoPrincipal=principal == null ? "No asignado" : $"{principal.Nombre} {principal.Apellido}".Trim(),
            Estado=m.Estado, FechaMatricula=m.FechaMatricula, Observaciones=m.Observaciones,
            Cursos=m.MatriculasCursos.Select(mc=>new MatriculaCursoDTO { IdCurso=mc.IdCurso, Codigo=mc.Curso?.Codigo ?? "", Nombre=mc.Curso?.Nombre ?? "" }).OrderBy(c=>c.Nombre).ToList() };
    }
}
