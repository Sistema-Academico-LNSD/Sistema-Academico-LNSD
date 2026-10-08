namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class CursoDisponibleDTO
    {
        public int IdCurso { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? NombreArea { get; set; }
    }
}