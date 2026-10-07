using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.BLL
{
    public class MapeoClases : Profile
    {
        public MapeoClases()
        {
            // Rol
            CreateMap<DAL.Entities.Rol, DTO.RolDTO>().ReverseMap();

            // Usuario
            CreateMap<DAL.Entities.Usuario, DTO.UsuarioDTO>().ReverseMap();

            // Acciones Usuario
            CreateMap<DTO.UsuarioActions.UsuarioCrearDTO, DAL.Entities.Usuario>();

            CreateMap<DTO.UsuarioActions.UsuarioActualizarDTO, DAL.Entities.Usuario>();

            // Institucion
            CreateMap<DAL.Entities.Institucion, DTO.InstitucionDTO>().ReverseMap();

            // Periodo Lectivo
            CreateMap<DAL.Entities.PeriodoLectivo, DTO.PeriodoLectivoDTO>().ReverseMap();

            // Contenido del sitio (misión, visión, historia, banners...)
            CreateMap<DAL.Entities.ContenidoSitio, DTO.ContenidoSitioDTO>().ReverseMap();

            // Accesos rápidos
            CreateMap<DAL.Entities.AccesoRapido, DTO.AccesoRapidoDTO>().ReverseMap();
            // Tiquete
            CreateMap<DAL.Entities.Tiquete, DTO.TiqueteDTO>();

            // Grado
            CreateMap<DAL.Entities.Grado, DTO.GradoDTO>().ReverseMap();

            // Seccion
            CreateMap<DAL.Entities.Seccion, DTO.SeccionDTO>().ReverseMap();

            //Curso
            CreateMap<DAL.Entities.Curso, DTO.Curso.CursoDTO>().ReverseMap();
            //Area Academica
            CreateMap<DAL.Entities.AreaAcademica, DTO.Curso.AreaAcademicaDTO>().ReverseMap();

            // Docente
            CreateMap<DAL.Entities.Docente, DTO.DocenteDTO>()
                .ForMember(d => d.NombreArea, o => o.MapFrom(s => s.Area != null ? s.Area.Nombre : null))
                .ForMember(d => d.TieneCuenta, o => o.MapFrom(s => s.IdUsuario.HasValue));

            CreateMap<DTO.DocenteDTO, DAL.Entities.Docente>();
        }
    }
}