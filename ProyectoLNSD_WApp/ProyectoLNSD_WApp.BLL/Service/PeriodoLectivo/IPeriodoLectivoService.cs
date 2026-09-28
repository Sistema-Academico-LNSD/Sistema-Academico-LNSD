using System;
using System.Collections.Generic;
using System.Text;
using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.PeriodoLectivo
{
    public interface IPeriodoLectivoService
    {
        Task<RespuestaDTO<List<PeriodoLectivoDTO>>> GetPeriodos();
        Task<RespuestaDTO<PeriodoLectivoDTO?>> GetPeriodo(int idPeriodo);

        // IdPeriodo == 0 registra; IdPeriodo > 0 actualiza.
        Task<RespuestaDTO<PeriodoLectivoDTO>> GuardarPeriodo(PeriodoLectivoDTO periodo, bool confirmarCambioActivo);
    }
}
