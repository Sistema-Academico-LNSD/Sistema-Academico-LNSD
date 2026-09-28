using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo
{
    public interface IPeriodoLectivoRepository
    {
        Task<List<Entities.PeriodoLectivo>> GetPeriodos();
        Task<Entities.PeriodoLectivo?> GetPeriodo(int idPeriodo);
        Task<Entities.PeriodoLectivo?> GetPeriodoActivo();
        Task<bool> ExisteNombre(string nombre, int? excluirIdPeriodo = null);

        // desactivarAnteriores = true: desactiva cualquier otro período activo en la misma transacción.
        Task<bool> CreatePeriodo(Entities.PeriodoLectivo periodo, bool desactivarAnteriores);
        Task<bool> UpdatePeriodo(Entities.PeriodoLectivo periodo, bool desactivarAnteriores);
    }
}
