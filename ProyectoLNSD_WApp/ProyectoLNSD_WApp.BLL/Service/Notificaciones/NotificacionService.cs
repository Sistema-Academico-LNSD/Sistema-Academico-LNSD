using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL.Service.Notificaciones;

public class NotificacionService(AppDbContext db, ILogger<NotificacionService> logger) : INotificacionService
{
    private static readonly string[] RolesPermitidos =
        ["Administrador", "Director", "Docente", "Estudiante", "Encargado", "Personal administrativo"];

    public async Task<NotificacionPanelDTO> ObtenerPanel(int idUsuario)
    {
        var query = db.Notificaciones.AsNoTracking().Where(n => n.IdUsuario == idUsuario);
        var pendientes = await query.CountAsync(n => !n.Leida);
        var lista = await query.OrderByDescending(n => n.FechaCreacion).ThenByDescending(n => n.IdNotificacion)
            .Select(n => new NotificacionListaDTO
            {
                IdNotificacion = n.IdNotificacion,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo!.Nombre,
                Codigo = n.Tipo!.Codigo,
                Referencia = n.Referencia,
                FechaCreacion = n.FechaCreacion,
                Leida = n.Leida,
                FechaLectura = n.FechaLectura
            }).ToListAsync();
        return new NotificacionPanelDTO { NoLeidas = pendientes, Notificaciones = lista };
    }

    public async Task<bool> MarcarLeida(int idUsuario, long idNotificacion)
    {
        var item = await db.Notificaciones.FirstOrDefaultAsync(n =>
            n.IdNotificacion == idNotificacion && n.IdUsuario == idUsuario);
        if (item == null) return false;
        if (!item.Leida)
        {
            item.Leida = true;
            item.FechaLectura = DateTime.Now;
            await db.SaveChangesAsync();
        }
        return true;
    }

    public async Task<List<NotificacionTipoDTO>> ObtenerTipos() =>
        await db.NotificacionTipos.AsNoTracking().Include(t => t.RolesDestinatarios)
            .OrderBy(t => t.Modulo).ThenBy(t => t.Nombre)
            .Select(t => new NotificacionTipoDTO
            {
                IdTipo = t.IdTipo, Codigo = t.Codigo, Nombre = t.Nombre,
                Modulo = t.Modulo, Descripcion = t.Descripcion, Activo = t.Activo,
                Roles = t.RolesDestinatarios.Select(r => r.Rol).ToList()
            }).ToListAsync();

    public async Task<bool> GuardarTipo(NotificacionTipoGuardarDTO dto)
    {
        var tipo = await db.NotificacionTipos.Include(t => t.RolesDestinatarios)
            .FirstOrDefaultAsync(t => t.IdTipo == dto.IdTipo);
        if (tipo == null) return false;
        var roles = (dto.Roles ?? new()).Where(RolesPermitidos.Contains).Distinct().ToList();
        if (roles.Count == 0) return false;
        tipo.Activo = dto.Activo;
        var actuales = tipo.RolesDestinatarios.ToList();
        foreach (var actual in actuales.Where(r => !roles.Contains(r.Rol)))
            db.NotificacionTipoRoles.Remove(actual);
        foreach (var rol in roles.Where(r => actuales.All(a => a.Rol != r)))
            tipo.RolesDestinatarios.Add(new NotificacionTipoRol { IdTipo = tipo.IdTipo, Rol = rol });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CambiarEstado(int idTipo, bool activo)
    {
        var tipo = await db.NotificacionTipos.FindAsync(idTipo);
        if (tipo == null) return false;
        tipo.Activo = activo;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> NotificarUsuarios(
        string codigo, string claveEvento, string titulo, string mensaje, string? referencia, IEnumerable<int> idsUsuarios)
    {
        if (string.IsNullOrWhiteSpace(claveEvento) || string.IsNullOrWhiteSpace(codigo)) return 0;
        if (mensaje.Length > 2000) mensaje = mensaje[..1997] + "...";
        if (titulo.Length > 180) titulo = titulo[..177] + "...";
        if (referencia != null && referencia.Length > 250) referencia = referencia[..250];
        try
        {
            string hashEvento = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(claveEvento)));
            string claveNormalizada = $"{codigo}:{hashEvento}";
            var tipo = await db.NotificacionTipos.Include(t => t.RolesDestinatarios)
                .FirstOrDefaultAsync(t => t.Codigo == codigo && t.Activo);
            if (tipo == null || tipo.RolesDestinatarios.Count == 0) return 0;

            var ids = idsUsuarios.Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0) return 0;
            var roles = tipo.RolesDestinatarios.Select(r => r.Rol).ToList();
            var destinatarios = await db.Usuarios.Where(u => ids.Contains(u.IdUsuario) && u.Estado
                    && u.Rol != null && roles.Contains(u.Rol.Nombre))
                .Select(u => u.IdUsuario).ToListAsync();

            // La misma clave estable se almacena para todos los destinatarios.
            // La restricción UNIQUE (id_usuario, clave_evento) evita duplicados por usuario.
            var existentes = (await db.Notificaciones
                .Where(n => destinatarios.Contains(n.IdUsuario) && n.ClaveEvento == claveNormalizada)
                .Select(n => n.IdUsuario)
                .ToListAsync()).ToHashSet();

            int creadas = 0;
            foreach (var id in destinatarios)
            {
                if (existentes.Contains(id)) continue;
                db.Notificaciones.Add(new Notificacion
                {
                    IdUsuario = id, IdTipo = tipo.IdTipo, Titulo = titulo,
                    Mensaje = mensaje, Referencia = referencia, ClaveEvento = claveNormalizada,
                    FechaCreacion = DateTime.Now
                });
                creadas++;
            }
            if (creadas > 0) await db.SaveChangesAsync();
            return creadas;
        }
        catch (Exception ex)
        {
            foreach (var entry in db.ChangeTracker.Entries<Notificacion>().Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;
            logger.LogError(ex, "No fue posible generar notificaciones del evento {Codigo}.", codigo);
            return 0;
        }
    }

    public Task NotificarCalificacion(int idNota, bool actualizada) =>
        EjecutarEventoSeguro("calificación", () => NotificarCalificacionInterno(idNota, actualizada));

    private async Task NotificarCalificacionInterno(int idNota, bool actualizada)
    {
        var nota = await db.Notas.AsNoTracking()
            .Include(n => n.Matricula!).ThenInclude(m => m.Estudiante!).ThenInclude(e => e.Usuario)
            .Include(n => n.Actividad!).ThenInclude(a => a.Curso)
            .FirstOrDefaultAsync(n => n.IdNota == idNota);
        if (nota?.Matricula?.Estudiante?.Usuario == null || nota.Actividad == null) return;
        var estudiante = nota.Matricula.Estudiante;
        string curso = nota.Actividad.Curso?.Nombre ?? "el curso";
        string accion = actualizada ? "actualizado" : "registrado";
        string codigo = actualizada ? "CALIFICACION_ACTUALIZADA" : "CALIFICACION_REGISTRADA";
        string clave = $"NOTA:{idNota}:{(actualizada ? "ACTUALIZADA" : "REGISTRADA")}:{nota.FechaModificacion?.Ticks}";
        string nombreEstudiante = $"{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}".Trim();
        string mensaje = $"Se ha {accion} una calificación de {nombreEstudiante} en {curso}. Actividad: {nota.Actividad.Nombre}. Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}.";
        await NotificarUsuarios(codigo, clave, actualizada ? "Calificación actualizada" : "Nueva calificación", mensaje,
            "/Calificacion/MisCalificaciones", new[] { estudiante.Usuario.IdUsuario });
        var encargados = await db.EncargadosEstudiantes.Where(x => x.IdEstudiante == estudiante.IdEstudiante)
            .Select(x => x.Encargado!.IdUsuario).ToListAsync();
        await NotificarUsuarios(codigo, clave, actualizada ? "Calificación actualizada" : "Nueva calificación", mensaje,
            "/Calificacion/CalificacionesEstudiantes", encargados);
    }

    public Task NotificarAsistencia(int idHorario, DateOnly fecha) =>
        EjecutarEventoSeguro("asistencia", () => NotificarAsistenciaInterno(idHorario, fecha));

    private async Task NotificarAsistenciaInterno(int idHorario, DateOnly fecha)
    {
        var registros = await db.Asistencias.AsNoTracking()
            .Include(a => a.Matricula!).ThenInclude(m => m.Estudiante!).ThenInclude(e => e.Usuario)
            .Include(a => a.Horario!).ThenInclude(h => h.Curso)
            .Where(a => a.IdHorario == idHorario && a.Fecha == fecha &&
                (a.Estado == "Ausente" || a.Estado == "Tardia" || a.Estado == "Justificada"))
            .ToListAsync();
        foreach (var a in registros)
        {
            var estudiante = a.Matricula?.Estudiante;
            if (estudiante?.Usuario == null) continue;
            string curso = a.Horario?.Curso?.Nombre ?? "la clase";
            string nombreEstudiante = $"{estudiante.Usuario!.Nombre} {estudiante.Usuario.Apellido}".Trim();
            string mensaje = a.Estado switch
            {
                "Ausente" => $"Se registró una ausencia de {nombreEstudiante} en {curso} el {a.Fecha:dd/MM/yyyy}.",
                "Tardia" => $"Se registró una llegada tardía de {nombreEstudiante} en {curso} el {a.Fecha:dd/MM/yyyy}.",
                _ => $"La asistencia de {nombreEstudiante} en {curso}, el {a.Fecha:dd/MM/yyyy}, fue marcada como justificada."
            };
            if (a.Estado == "Justificada" && !string.IsNullOrWhiteSpace(a.Observacion))
                mensaje += $" Observación: {a.Observacion}";
            var usuarios = new List<int> { estudiante.Usuario.IdUsuario };
            usuarios.AddRange(await db.EncargadosEstudiantes.Where(x => x.IdEstudiante == estudiante.IdEstudiante)
                .Select(x => x.Encargado!.IdUsuario).ToListAsync());
            await NotificarUsuarios("ASISTENCIA_EXCEPCIONAL", $"ASISTENCIA:{a.IdAsistencia}:{a.Estado}:{(a.FechaModificacion ?? a.FechaRegistro).Ticks}",
                "Actualización de asistencia", mensaje, "/Asistencia/MiAsistencia", usuarios);
        }
    }

    public Task NotificarHorario(int idHorario, string evento, int? idDocenteAnterior = null, HorarioDTO? horarioAnterior = null) =>
        EjecutarEventoSeguro("horario", () => NotificarHorarioInterno(idHorario, evento, idDocenteAnterior, horarioAnterior));

    private async Task NotificarHorarioInterno(int idHorario, string evento, int? idDocenteAnterior, HorarioDTO? horarioAnterior)
    {
        var h = await db.Horarios.AsNoTracking().Include(x => x.Curso).Include(x => x.Seccion)
            .Include(x => x.Grupo).Include(x => x.Docente).FirstOrDefaultAsync(x => x.IdHorario == idHorario);
        if (h == null) return;
        string codigo = evento == "ASIGNADO" ? "HORARIO_ASIGNADO" : evento == "RETIRADO" ? "HORARIO_RETIRADO" : "HORARIO_MODIFICADO";
        string curso = h.Curso?.Nombre ?? "Clase";
        string horario = $"{h.HoraInicio.ToString("HH:mm")}-{h.HoraFin.ToString("HH:mm")}";
        string grupo = h.Grupo?.Nombre ?? string.Empty;
        string seccion = h.Seccion?.Nombre ?? "su sección";
        string mensaje = evento == "RETIRADO"
            ? $"Se retiró el horario de {curso}, sección {seccion} grupo {grupo}: {h.Dia}, {horario}, aula {h.Aula}, docente {h.Docente?.Nombre} {h.Docente?.Apellidos}."
            : $"{(evento == "ASIGNADO" ? "Se asignó" : "Se modificó")} {curso}, sección {seccion} grupo {grupo}: {h.Dia}, {horario}, aula {h.Aula}, docente {h.Docente?.Nombre} {h.Docente?.Apellidos}.";
        var matriculas = await db.Matriculas.AsNoTracking().Include(m => m.Estudiante!).ThenInclude(e => e.Usuario)
            .Where(m => m.Estado == "Activa" && m.IdPeriodo == h.IdPeriodo && m.IdSeccion == h.IdSeccion && m.IdGrupo == h.IdGrupo
                && m.MatriculasCursos.Any(mc => mc.IdCurso == h.IdCurso))
            .Select(m => new { m.IdMatricula, IdUsuario = m.Estudiante!.Usuario!.IdUsuario })
            .ToListAsync();
        var usuarios = matriculas.Select(x => x.IdUsuario).ToList();
        if (horarioAnterior != null && (horarioAnterior.IdCurso != h.IdCurso || horarioAnterior.IdSeccion != h.IdSeccion ||
            horarioAnterior.IdGrupo != h.IdGrupo || horarioAnterior.IdPeriodo != h.IdPeriodo))
        {
            var usuariosAnteriores = await db.Matriculas.AsNoTracking()
                .Where(m => m.Estado == "Activa" && m.IdPeriodo == horarioAnterior.IdPeriodo &&
                    m.IdSeccion == horarioAnterior.IdSeccion && m.IdGrupo == horarioAnterior.IdGrupo &&
                    m.MatriculasCursos.Any(mc => mc.IdCurso == horarioAnterior.IdCurso))
                .Select(m => m.Estudiante!.Usuario!.IdUsuario).ToListAsync();
            usuarios.AddRange(usuariosAnteriores);
        }
        if (h.Docente?.IdUsuario is int idDocente) usuarios.Add(idDocente);
        if (idDocenteAnterior.HasValue)
        {
            var usuarioAnterior = await db.Docentes.Where(d => d.IdDocente == idDocenteAnterior.Value)
                .Select(d => d.IdUsuario).FirstOrDefaultAsync();
            if (usuarioAnterior.HasValue) usuarios.Add(usuarioAnterior.Value);
        }
        string huellaAnterior = horarioAnterior == null
            ? "NUEVO"
            : string.Join("|", horarioAnterior.IdPeriodo, horarioAnterior.IdSeccion,
                horarioAnterior.IdGrupo, horarioAnterior.IdCurso, horarioAnterior.IdDocente,
                horarioAnterior.Dia, horarioAnterior.HoraInicio, horarioAnterior.HoraFin,
                horarioAnterior.Aula, horarioAnterior.Estado);
        string huellaActual = string.Join("|", h.IdPeriodo, h.IdSeccion, h.IdGrupo,
            h.IdCurso, h.IdDocente, h.Dia, h.HoraInicio.ToString("HH:mm:ss"),
            h.HoraFin.ToString("HH:mm:ss"), h.Aula, h.Estado);
        string clave = $"HORARIO:{idHorario}:{evento}:{huellaAnterior}=>{huellaActual}";
        await NotificarUsuarios(codigo, clave, evento == "RETIRADO" ? "Horario retirado" : evento == "ASIGNADO" ? "Nuevo horario" : "Horario actualizado",
            mensaje, null, usuarios);
    }

    public Task NotificarInformativo(int idInformativo, string evento, string? claveCambio = null) =>
        EjecutarEventoSeguro("informativo", () => NotificarInformativoInterno(idInformativo, evento, claveCambio));

    private async Task NotificarInformativoInterno(int idInformativo, string evento, string? claveCambio)
    {
        var item = await db.InformativoRegistros.AsNoTracking().FirstOrDefaultAsync(x => x.IdInformativo == idInformativo);
        if (item == null || item.Estado != "Publicado") return;
        if (evento != "CANCELADO" && (item.Eliminado || item.FechaPublicacion > DateTime.Now ||
            item.Tipo == "Anuncio" && item.FechaVigencia.HasValue && item.FechaVigencia.Value.Date < DateTime.Today ||
            item.Tipo != "Anuncio" && (item.FechaFin ?? item.FechaInicio).Date < DateTime.Today)) return;
        string sufijo = item.Tipo == "Anuncio" ? "ANUNCIO" : item.Tipo == "Actividad" ? "ACTIVIDAD" : "EVENTO";
        string accionCodigo = item.Tipo == "Actividad"
            ? (evento switch
            {
                "PUBLICADO" => "PUBLICADA",
                "ACTUALIZADO" => "ACTUALIZADA",
                "CANCELADO" => "CANCELADA",
                _ => evento
            })
            : evento;
        string codigo = $"{sufijo}_{accionCodigo}";
        string? tituloTipo = item.Tipo == "Anuncio" ? "Anuncio institucional" : item.Tipo == "Actividad" ? "Actividad de calendario" : "Evento institucional";
        string mensaje = $"{item.Titulo}: {item.Descripcion}";
        if (item.Tipo == "Anuncio") mensaje += $" Publicación: {item.FechaPublicacion:dd/MM/yyyy}.";
        else mensaje += $" Fecha: {item.FechaInicio:dd/MM/yyyy}" + (item.HoraInicio.HasValue ? $", hora: {item.HoraInicio.Value.ToString(@"hh\:mm")}" : "") +
            (string.IsNullOrWhiteSpace(item.Ubicacion) ? "." : $", ubicación: {item.Ubicacion}.");
        var usuarios = await db.Usuarios.AsNoTracking().Where(u => u.Estado && u.Rol != null &&
            (u.Rol.Nombre == "Estudiante" || u.Rol.Nombre == "Encargado" || u.Rol.Nombre == "Docente"))
            .Select(u => u.IdUsuario).ToListAsync();
        string huellaActual = string.Join("|", item.Tipo, item.Titulo, item.Descripcion,
            item.FechaPublicacion.ToString("O"), item.FechaVigencia?.ToString("O"),
            item.FechaInicio.ToString("O"), item.FechaFin?.ToString("O"),
            item.HoraInicio?.ToString("c"), item.HoraFin?.ToString("c"),
            item.Ubicacion, item.Categoria, item.Estado, item.Eliminado);
        string clave = $"INFORMATIVO:{idInformativo}:{evento}:{(string.IsNullOrWhiteSpace(claveCambio) ? huellaActual : claveCambio)}";
        await NotificarUsuarios(codigo, clave, evento == "CANCELADO" ? $"{tituloTipo} cancelado" : evento == "ACTUALIZADO" ? $"{tituloTipo} actualizado" : tituloTipo,
            mensaje, $"/Informativo/Index", usuarios);
    }

    public Task NotificarBeca(int idBeca, string evento) =>
        EjecutarEventoSeguro("beca", () => NotificarBecaInterno(idBeca, evento));

    private async Task NotificarBecaInterno(int idBeca, string evento)
    {
        var beca = await db.Becas.AsNoTracking().Include(b => b.Estudiante!).ThenInclude(e => e.Usuario)
            .FirstOrDefaultAsync(b => b.IdBeca == idBeca);
        if (beca?.Estudiante?.Usuario == null) return;
        if (evento == "ACTUALIZADA" && beca.Estado != "Activa") return;
        if (evento == "APROBADA" && beca.Estado != "Activa") return;
        string codigo = evento == "APROBADA" ? "BECA_APROBADA" : evento == "CANCELADA" ? "BECA_CANCELADA" : "BECA_ACTUALIZADA";
        string estado = beca.Estado;
        string nombreEstudiante = $"{beca.Estudiante.Usuario.Nombre} {beca.Estudiante.Usuario.Apellido}".Trim();
        string mensaje = evento == "APROBADA"
            ? $"La beca socioeconómica de {nombreEstudiante} fue aprobada. Monto: {beca.Monto:C}. Vigencia: {beca.FechaInicio:dd/MM/yyyy} al {beca.FechaFin:dd/MM/yyyy}."
            : evento == "CANCELADA"
                ? $"La beca socioeconómica de {nombreEstudiante} fue desactivada. Estado actual: {estado}. Motivo: {beca.MotivoDesactivacion ?? "No especificado"}."
                : $"La beca socioeconómica de {nombreEstudiante} fue actualizada. Monto: {beca.Monto:C}. Vigencia: {beca.FechaInicio:dd/MM/yyyy} al {beca.FechaFin:dd/MM/yyyy}.";
        var usuarios = new List<int> { beca.Estudiante.Usuario.IdUsuario };
        usuarios.AddRange(await db.EncargadosEstudiantes.Where(x => x.IdEstudiante == beca.IdEstudiante)
            .Select(x => x.Encargado!.IdUsuario).ToListAsync());
        string clave = $"BECA:{idBeca}:{evento}:{beca.Monto}:{beca.FechaInicio}:{beca.FechaFin}:{beca.Estado}:{beca.FechaModificacion?.Ticks}";
        await NotificarUsuarios(codigo, clave, evento == "APROBADA" ? "Beca aprobada" : evento == "CANCELADA" ? "Beca cancelada" : "Beca actualizada",
            mensaje, "/Beca/MiBeca", usuarios);
    }

    private async Task EjecutarEventoSeguro(string modulo, Func<Task> accion)
    {
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            // Una falla de notificaciones nunca debe invalidar una operación académica ya guardada.
            logger.LogError(ex, "No fue posible generar notificaciones del módulo {Modulo}.", modulo);
        }
    }

}
