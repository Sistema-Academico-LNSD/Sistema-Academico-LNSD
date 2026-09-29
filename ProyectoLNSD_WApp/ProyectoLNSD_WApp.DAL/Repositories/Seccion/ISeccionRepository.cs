using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Seccion
{
    public interface ISeccionRepository
    {
        Task<List<Entities.Seccion>> GetSecciones();
        Task<Entities.Seccion?> GetSeccionById(int id);
        Task<bool> CreateSeccion(Entities.Seccion seccion);
        Task<bool> UpdateSeccion(Entities.Seccion seccion);
        Task<bool> CambiarEstado(int id, bool estado);
        Task<bool> ExisteNombreEnGrado(int idGrado, string nombre, int? excluirId = null);
    }
}
