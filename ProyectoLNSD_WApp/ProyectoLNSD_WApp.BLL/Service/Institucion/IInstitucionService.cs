using System;
using System.Collections.Generic;
using System.Text;
using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Institucion
{
    public interface IInstitucionService
    {
        Task<RespuestaDTO<InstitucionDTO?>> GetInstitucion();

        // nuevaRutaLogo: solo si se subió un logo nuevo (lo valida y guarda el controller).
        Task<RespuestaDTO<InstitucionDTO>> GuardarInstitucion(InstitucionDTO institucion, string? nuevaRutaLogo);
    }
}
