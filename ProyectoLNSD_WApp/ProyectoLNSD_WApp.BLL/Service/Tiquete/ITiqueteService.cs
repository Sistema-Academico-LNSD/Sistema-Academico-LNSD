using System;
using System.Collections.Generic;
using System.Text;

using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Tiquete
{
    public interface ITiqueteService
    {
        // MBLF-01-01
        Task<RespuestaDTO<TiqueteDTO>> GenerarTiquete(int idUsuario);

        // MBLF-01-02
        Task<RespuestaDTO<List<TiqueteDTO>>> GetMisTiquetes(int idUsuario);

        // MBLF-01-03
        Task<RespuestaDTO<TiqueteDTO?>> GetTiqueteActual(int idUsuario);
        // MBLF-01-04
        Task<RespuestaDTO<TiqueteDTO>> ValidarTiquete(string codigo, string correo);

        // MBLF-01-05
        Task<RespuestaDTO<TiqueteDTO>> MarcarComoUtilizado(string codigo, string correo, int idUsuarioValidador);

        // MBLF-01-06 y 01-07
        Task<RespuestaDTO<List<TiqueteDTO>>> GetTiquetes(string? estado);

        // MBLF-01-08
        Task<RespuestaDTO<TiqueteResumenDTO>> GetResumen();
    }
}