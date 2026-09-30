namespace ProyectoLNSD_WApp.BLL.DTO.Curso
{
    public class CursoListaDTO
    {
        public int IdCurso { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NombreArea { get; set; } = string.Empty;
        public string Grados { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}