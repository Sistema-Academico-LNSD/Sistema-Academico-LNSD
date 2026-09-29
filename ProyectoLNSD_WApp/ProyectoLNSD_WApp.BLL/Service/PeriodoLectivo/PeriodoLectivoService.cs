using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;

namespace ProyectoLNSD_WApp.BLL.Service.PeriodoLectivo
{
    public class PeriodoLectivoService : IPeriodoLectivoService
    {
        // Códigos de error del módulo Configuración (3xxx)
        public const int CodigoDatosRequeridos = 3101;
        public const int CodigoNombreDuplicado = 3102;
        public const int CodigoFechasInvalidas = 3103;
        public const int CodigoConfirmarCambioActivo = 3104;   // la vista pide confirmación y reenvía
        public const int CodigoNoEncontrado = 3105;

        private readonly IPeriodoLectivoRepository _periodoRepository;
        private readonly IMapper _mapper;

        public PeriodoLectivoService(IPeriodoLectivoRepository periodoRepository, IMapper mapper)
        {
            _periodoRepository = periodoRepository;
            _mapper = mapper;
        }

        // HU escenario 5: consulta de períodos
        public async Task<RespuestaDTO<List<PeriodoLectivoDTO>>> GetPeriodos()
        {
            var periodos = await _periodoRepository.GetPeriodos();
            return RespuestaDTO<List<PeriodoLectivoDTO>>.Exito(_mapper.Map<List<PeriodoLectivoDTO>>(periodos));
        }

        public async Task<RespuestaDTO<PeriodoLectivoDTO?>> GetPeriodo(int idPeriodo)
        {
            var periodo = await _periodoRepository.GetPeriodo(idPeriodo);

            if (periodo == null)
                return RespuestaDTO<PeriodoLectivoDTO?>.Error("El período lectivo no existe", CodigoNoEncontrado);

            return RespuestaDTO<PeriodoLectivoDTO?>.Exito(_mapper.Map<PeriodoLectivoDTO>(periodo));
        }

        public async Task<RespuestaDTO<PeriodoLectivoDTO>> GuardarPeriodo(PeriodoLectivoDTO periodo, bool confirmarCambioActivo)
        {
            // Datos obligatorios
            if (string.IsNullOrWhiteSpace(periodo.Nombre) ||
                periodo.FechaInicio == null || periodo.FechaFin == null)
                return RespuestaDTO<PeriodoLectivoDTO>.Error("El nombre, la fecha de inicio y la fecha de finalización son requeridos", CodigoDatosRequeridos);

            // HU escenario 3: fechas inválidas
            if (periodo.FechaFin < periodo.FechaInicio)
                return RespuestaDTO<PeriodoLectivoDTO>.Error("La fecha de finalización no puede ser anterior a la fecha de inicio", CodigoFechasInvalidas);

            string nombre = periodo.Nombre.Trim();
            bool esNuevo = periodo.IdPeriodo == 0;

            if (await _periodoRepository.ExisteNombre(nombre, esNuevo ? null : periodo.IdPeriodo))
                return RespuestaDTO<PeriodoLectivoDTO>.Error("Ya existe un período lectivo con ese nombre", CodigoNombreDuplicado);

            if (!esNuevo && await _periodoRepository.GetPeriodo(periodo.IdPeriodo) == null)
                return RespuestaDTO<PeriodoLectivoDTO>.Error("El período lectivo no existe", CodigoNoEncontrado);

            // HU escenario 4: ya hay otro período activo -> se pide confirmar el cambio
            bool desactivarAnteriores = false;
            if (periodo.Activo)
            {
                var activoActual = await _periodoRepository.GetPeriodoActivo();

                if (activoActual != null && activoActual.IdPeriodo != periodo.IdPeriodo)
                {
                    if (!confirmarCambioActivo)
                        return RespuestaDTO<PeriodoLectivoDTO>.Error(
                            $"Ya existe un período activo ({activoActual.Nombre}). ¿Desea desactivarlo y activar este período?",
                            CodigoConfirmarCambioActivo);

                    desactivarAnteriores = true;
                }
            }

            var entidad = _mapper.Map<DAL.Entities.PeriodoLectivo>(periodo);
            entidad.Nombre = nombre;

            bool guardado = esNuevo
                ? await _periodoRepository.CreatePeriodo(entidad, desactivarAnteriores)
                : await _periodoRepository.UpdatePeriodo(entidad, desactivarAnteriores);

            if (!guardado)
                return RespuestaDTO<PeriodoLectivoDTO>.Error("No se pudo guardar el período lectivo", 500);

            return RespuestaDTO<PeriodoLectivoDTO>.Exito(
                _mapper.Map<PeriodoLectivoDTO>(entidad),
                esNuevo ? "Período lectivo registrado correctamente" : "Período lectivo actualizado correctamente");
        }
    }
}
