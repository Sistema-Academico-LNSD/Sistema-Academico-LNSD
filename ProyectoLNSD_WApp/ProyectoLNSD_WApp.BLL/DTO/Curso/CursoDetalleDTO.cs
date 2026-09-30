using ProyectoLNSD_WApp.BLL.DTO; 

namespace ProyectoLNSD_WApp.BLL.DTO.Curso
{
    public class CursoDetalleDTO
    {
        public CursoDTO Curso { get; set; } = new();

        public AreaAcademicaDTO? Area { get; set; }

        public List<GradoDTO> Grados { get; set; } = new();
    }
}