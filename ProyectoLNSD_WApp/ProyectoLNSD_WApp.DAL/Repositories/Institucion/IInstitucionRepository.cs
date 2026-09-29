using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Institucion
{
    public interface IInstitucionRepository
    {
        Task<Entities.Institucion?> GetInstitucion();
        Task<bool> CreateInstitucion(Entities.Institucion institucion);
        Task<bool> UpdateInstitucion(Entities.Institucion institucion);
    }
}
