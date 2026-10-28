using System.Security.Claims;
using ProyectoLNSD_WApp.Authorization;

namespace ProyectoLNSD_WApp.Navegacion
{
    /// <summary>
    /// Un enlace del menú lateral.
    /// "Visible" usa la MISMA regla que protege la acción en su controlador,
    /// así nadie ve un enlace que lo lleve a "Acceso denegado".
    /// "CoincidirAccion" se usa cuando varias opciones comparten controlador (ej. Boletería).
    /// </summary>
    public sealed record MenuItem(
        string Texto,
        string Icono,
        string Controlador,
        string Accion,
        Func<ClaimsPrincipal, bool> Visible,
        bool CoincidirAccion = false);

    public sealed record MenuGrupo(string Titulo, IReadOnlyList<MenuItem> Items);

    /// <summary>
    /// Única fuente del menú de la aplicación. Para agregar un módulo nuevo
    /// basta con agregar una línea aquí: el layout no se toca.
    /// </summary>
    public static class MenuNavegacion
    {
        private static readonly IReadOnlyList<MenuGrupo> Grupos =
        [
            new("", [
                new("Inicio", "bi-house-door", "Home", "Index", _ => true)
            ]),

            new("Mi espacio", [
                new("Mi información", "bi-person-vcard", "MiPerfil", "MiExpediente",
                    u => u.IsInRole(RolesSistema.Estudiante), CoincidirAccion: true),
                new("Mis estudiantes", "bi-people", "MiPerfil", "MisEstudiantes",
                    u => u.IsInRole(RolesSistema.Encargado), CoincidirAccion: true),
                new("Mi perfil docente", "bi-person-video3", "MiPerfil", "Index",
                    u => u.IsInRole(RolesSistema.Docente), CoincidirAccion: true),
                new("Mi tiquete", "bi-ticket-perforated", "Boleteria", "Index",
                    _ => true, CoincidirAccion: true),
                new("Material de estudio", "bi-journal-bookmark", "Recurso", "Material",
                    u => u.IsInRole(RolesSistema.Estudiante) && u.TienePermiso("Recursos", "Ver"), CoincidirAccion: true),
                new("Mis recursos", "bi-folder2-open", "Recurso", "Index",
                    u => u.TienePermiso("Recursos", "Crear"), CoincidirAccion: true)
            ]),

            new("Gestión académica", [
                new("Estudiantes", "bi-backpack2", "Estudiante", "Index",
                    u => u.TienePermiso("Estudiantes", "Ver")),
                new("Docentes", "bi-person-workspace", "Docente", "Index",
                    u => u.TienePermiso("Docentes", "Ver")),
                new("Matrículas", "bi-clipboard-check", "Matriculas", "Index",
                    u => u.TienePermiso("Matricula", "Ver")),
                new("Grados y secciones", "bi-diagram-3", "Grado", "Index",
                    u => u.TienePermiso("Grados", "Ver")),
                new("Grupos A/B", "bi-people-fill", "Grupos", "Index",
                    u => u.TienePermiso("Grados", "Ver")),
                new("Cursos", "bi-book", "Curso", "Index",
                    u => u.TienePermiso("Cursos", "Ver")),
                new("Supervisión de recursos", "bi-card-checklist", "Recurso", "Supervision",
                    u => (u.IsInRole(RolesSistema.Administrador) || u.IsInRole(RolesSistema.Director))
                         && u.TienePermiso("Recursos", "Ver"), CoincidirAccion: true)
            ]),

            new("Comedor", [
                new("Validar tiquetes", "bi-qr-code-scan", "Boleteria", "Validar",
                    u => u.TienePermiso("Boleteria", "Editar"), CoincidirAccion: true),
                new("Control de tiquetes", "bi-clipboard-data", "Boleteria", "Consulta",
                    u => u.TienePermiso("Boleteria", "Ver"), CoincidirAccion: true)
            ]),

            new("Administración", [
                new("Usuarios", "bi-person-gear", "Usuario", "Index",
                    u => u.TienePermiso("Usuarios", "Ver")),
                new("Roles", "bi-person-badge", "Rol", "Index",
                    u => u.TienePermiso("Roles", "Ver")),
                // Permisos y Auditoría se protegen por rol en su controlador ([Authorize(Roles = "Administrador")]).
                new("Permisos", "bi-shield-lock", "Permiso", "Index",
                    u => u.IsInRole(RolesSistema.Administrador)),
                new("Auditoría", "bi-journal-text", "Auditoria", "Index",
                    u => u.IsInRole(RolesSistema.Administrador)),
                new("Configuración", "bi-gear", "Configuracion", "Index",
                    u => u.TienePermiso("Configuracion", "Ver"))
            ])
        ];

        /// <summary>Devuelve solo los grupos y opciones que el usuario puede ver. Los grupos vacíos no se muestran.</summary>
        public static IEnumerable<MenuGrupo> Para(ClaimsPrincipal usuario) =>
            Grupos
                .Select(g => g with { Items = g.Items.Where(i => i.Visible(usuario)).ToList() })
                .Where(g => g.Items.Count > 0);
    }
}
