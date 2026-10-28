namespace ProyectoLNSD_WApp.Authorization
{
    /// <summary>
    /// Nombres de los roles que el código usa por nombre (deben coincidir con la tabla Rol).
    /// Centralizarlos evita errores de escritura repetidos en vistas y controladores.
    /// </summary>
    public static class RolesSistema
    {
        public const string Administrador = "Administrador";
        public const string Director = "Director";
        public const string Docente = "Docente";
        public const string Estudiante = "Estudiante";
        public const string Encargado = "Encargado";
        public const string PersonalAdministrativo = "Personal administrativo";
    }
}
