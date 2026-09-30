using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.Curso;
using ProyectoLNSD_WApp.DAL.Repositories.Curso;

namespace ProyectoLNSD_WApp.BLL.Service.Curso
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _cursoRepository;
        private readonly IMapper _mapper;

        public CursoService(
            ICursoRepository cursoRepository,
            IMapper mapper)
        {
            _cursoRepository = cursoRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<List<CursoListaDTO>>> GetCursos(
            string? texto,
            int? idGrado,
            int? idArea,
            bool? estado)
        {
            List<DAL.Entities.Curso> cursos =
                await _cursoRepository.GetCursos(
                    texto,
                    idGrado,
                    idArea,
                    estado);

            List<CursoListaDTO> resultado = cursos
                .Select(c => new CursoListaDTO
                {
                    IdCurso = c.IdCurso,
                    Codigo = c.Codigo,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    NombreArea = c.Area?.Nombre ?? "Sin área",
                    Grados = string.Join(
                        ", ",
                        c.CursosGrados
                            .Where(cg => cg.Grado != null)
                            .OrderBy(cg => cg.Grado!.Nombre)
                            .Select(cg => cg.Grado!.Nombre)),
                    Estado = c.Estado
                })
                .ToList();

            return RespuestaDTO<List<CursoListaDTO>>.Exito(resultado);
        }

        public async Task<RespuestaDTO<List<AreaAcademicaDTO>>> GetAreasActivas()
        {
            List<DAL.Entities.AreaAcademica> areas =
                await _cursoRepository.GetAreasActivas();

            return RespuestaDTO<List<AreaAcademicaDTO>>.Exito(
                _mapper.Map<List<AreaAcademicaDTO>>(areas));
        }

        public async Task<RespuestaDTO<List<GradoDTO>>> GetGradosActivos()
        {
            List<DAL.Entities.Grado> grados =
                await _cursoRepository.GetGradosActivos();

            return RespuestaDTO<List<GradoDTO>>.Exito(
                _mapper.Map<List<GradoDTO>>(grados));
        }

        public async Task<RespuestaDTO<CursoDTO?>> GetCursoById(int id)
        {
            DAL.Entities.Curso? curso =
                await _cursoRepository.GetCursoById(id);

            if (curso == null)
                return RespuestaDTO<CursoDTO?>.Error(
                    "Curso no encontrado",
                    404);

            CursoDTO resultado = _mapper.Map<CursoDTO>(curso);

            resultado.IdGrados = curso.CursosGrados
                .Select(cg => cg.IdGrado)
                .ToList();

            return RespuestaDTO<CursoDTO?>.Exito(resultado);
        }

        public async Task<RespuestaDTO<CursoDetalleDTO?>> GetDetalleCurso(int id)
        {
            DAL.Entities.Curso? curso =
                await _cursoRepository.GetCursoDetalle(id);

            if (curso == null)
                return RespuestaDTO<CursoDetalleDTO?>.Error(
                    "Curso no encontrado",
                    404);

            CursoDetalleDTO detalle = new CursoDetalleDTO
            {
                Curso = _mapper.Map<CursoDTO>(curso),
                Area = curso.Area == null
                    ? null
                    : _mapper.Map<AreaAcademicaDTO>(curso.Area),

                Grados = curso.CursosGrados
                    .Where(cg => cg.Grado != null)
                    .Select(cg => _mapper.Map<GradoDTO>(cg.Grado))
                    .OrderBy(g => g.Nombre)
                    .ToList()
            };

            detalle.Curso.IdGrados = detalle.Grados
                .Select(g => g.IdGrado)
                .ToList();

            return RespuestaDTO<CursoDetalleDTO?>.Exito(detalle);
        }

        public async Task<RespuestaDTO<CursoDTO>> CreateCurso(
            CursoDTO curso)
        {
            RespuestaDTO<CursoDTO>? validacion =
                ValidarCurso(curso);

            if (validacion != null)
                return validacion;

            if (await _cursoRepository.ExisteCodigo(curso.Codigo))
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Ya existe un curso con ese código",
                    1006);
            }

            if (await _cursoRepository.ExisteNombre(
                curso.Nombre,
                curso.IdArea))
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Ya existe un curso con ese nombre en esta área académica",
                    1006);
            }

            if (curso.IdGrados == null ||
                curso.IdGrados.Count == 0)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Debe seleccionar al menos un grado",
                    1001);
            }

            curso.Codigo = curso.Codigo.Trim();
            curso.Nombre = curso.Nombre.Trim();
            curso.Descripcion = curso.Descripcion.Trim();
            curso.Estado = true;

            DAL.Entities.Curso entidad =
                _mapper.Map<DAL.Entities.Curso>(curso);

            try
            {
                bool creado = await _cursoRepository.CreateCurso(
                    entidad,
                    curso.IdGrados);

                if (!creado)
                {
                    return RespuestaDTO<CursoDTO>.Error(
                        "No se pudo registrar el curso",
                        1002);
                }
            }
            catch (DbUpdateException)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "No se pudo registrar el curso. Verifique los datos seleccionados",
                    1002);
            }

            curso.IdCurso = entidad.IdCurso;

            return RespuestaDTO<CursoDTO>.Exito(
                curso,
                "Curso registrado correctamente");
        }

        public async Task<RespuestaDTO<CursoDTO>> UpdateCurso(
            CursoDTO curso)
        {
            if (curso.IdCurso <= 0)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Curso inválido",
                    1003);
            }

            DAL.Entities.Curso? existente =
                await _cursoRepository.GetCursoById(curso.IdCurso);

            if (existente == null)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Curso no encontrado",
                    404);
            }

            RespuestaDTO<CursoDTO>? validacion =
                ValidarCurso(curso);

            if (validacion != null)
                return validacion;

            if (await _cursoRepository.ExisteCodigo(
                curso.Codigo,
                curso.IdCurso))
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Ya existe un curso con ese código",
                    1006);
            }

            if (await _cursoRepository.ExisteNombre(
                curso.Nombre,
                curso.IdArea,
                curso.IdCurso))
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Ya existe un curso con ese nombre en esta área académica",
                    1006);
            }

            if (curso.IdGrados == null ||
                curso.IdGrados.Count == 0)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Debe seleccionar al menos un grado",
                    1001);
            }

            curso.Codigo = curso.Codigo.Trim();
            curso.Nombre = curso.Nombre.Trim();
            curso.Descripcion = curso.Descripcion.Trim();

            curso.Estado = existente.Estado;

            DAL.Entities.Curso entidad =
                _mapper.Map<DAL.Entities.Curso>(curso);

            try
            {
                bool actualizado =
                    await _cursoRepository.UpdateCurso(
                        entidad,
                        curso.IdGrados);

                if (!actualizado)
                {
                    return RespuestaDTO<CursoDTO>.Error(
                        "No se pudo actualizar el curso",
                        1004);
                }
            }
            catch (DbUpdateException)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "No se pudo actualizar el curso. Verifique los datos seleccionados",
                    1004);
            }

            return RespuestaDTO<CursoDTO>.Exito(
                curso,
                "Curso actualizado correctamente");
        }

        public async Task<RespuestaDTO<CursoDTO>> CambiarEstadoCurso(
            int id,
            bool estado)
        {
            DAL.Entities.Curso? existente =
                await _cursoRepository.GetCursoById(id);

            if (existente == null)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Curso no encontrado",
                    404);
            }

            if (!await _cursoRepository.CambiarEstado(id, estado))
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "No se pudo cambiar el estado del curso",
                    1005);
            }

            return RespuestaDTO<CursoDTO>.Exito(
                null,
                estado
                    ? "Curso activado correctamente"
                    : "Curso desactivado correctamente");
        }

        public async Task<RespuestaDTO<bool>> EliminarCurso(int id)
        {
            DAL.Entities.Curso? existente =
                await _cursoRepository.GetCursoById(id);

            if (existente == null)
            {
                return RespuestaDTO<bool>.Error(
                    "Curso no encontrado",
                    404);
            }

            try
            {
                bool eliminado =
                    await _cursoRepository.EliminarCurso(id);

                if (!eliminado)
                {
                    return RespuestaDTO<bool>.Error(
                        "No se pudo eliminar el curso",
                        1007);
                }

                return RespuestaDTO<bool>.Exito(
                    true,
                    "Curso eliminado correctamente");
            }
            catch (DbUpdateException)
            {
                return RespuestaDTO<bool>.Error(
                    "No se puede eliminar el curso porque tiene dependencias académicas asociadas. Puede inactivarlo en su lugar.",
                    1008);
            }
        }

        private static RespuestaDTO<CursoDTO>? ValidarCurso(
            CursoDTO curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Codigo) ||
                string.IsNullOrWhiteSpace(curso.Nombre) ||
                string.IsNullOrWhiteSpace(curso.Descripcion))
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Complete los campos obligatorios del curso",
                    1001);
            }

            if (curso.IdArea <= 0)
            {
                return RespuestaDTO<CursoDTO>.Error(
                    "Seleccione un área académica",
                    1001);
            }

            return null;
        }
    }
}