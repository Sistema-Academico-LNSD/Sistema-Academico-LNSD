using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.Tiquete
{
    public interface ITiqueteRepository
    {
        Task<Entities.Tiquete?> GetTiqueteDisponibleByUsuario(int idUsuario);

        Task<List<Entities.Tiquete>> GetTiquetesByUsuario(int idUsuario);

        Task<bool> ExisteCodigo(string codigo);

        Task<bool> CreateTiquete(Entities.Tiquete tiquete);

        Task<Entities.Tiquete?> GetTiqueteByCodigo(string codigo);

        Task<bool> MarcarUtilizado(int idTiquete, int idUsuarioValidador, DateTime fechaUtilizacion);

        Task<List<Entities.Tiquete>> GetTiquetes(string? estado);

        Task<int> ContarTiquetes(string? estado = null);
    }
}
