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

        }
    }
}