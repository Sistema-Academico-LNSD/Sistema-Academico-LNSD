using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Grado
{
    public interface IGradoRepository
    {
        Task<List<Entities.Grado>> GetGrados();
        Task<List<Entities.Grado>> GetGradosActivos();
        Task<Entities.Grado?> GetGradoById(int id);
        Task<Entities.Grado?> GetGradoConSecciones(int id);
        Task<bool> CreateGrado(Entities.Grado grado);
        Task<bool> UpdateGrado(Entities.Grado grado);
        Task<bool> CambiarEstado(int id, bool estado);
        Task<bool> ExisteNombre(string nombre, int? excluirId = null);
        Task<bool> ExisteCodigo(string codigo, int? excluirId = null);
    }
}
