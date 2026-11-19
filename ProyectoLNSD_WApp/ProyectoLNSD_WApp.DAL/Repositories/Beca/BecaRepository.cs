using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Beca
{
    public class BecaRepository : IBecaRepository
    {
        private const string MatriculaAnulada = "Anulada";

        private readonly AppDbContext _context;

        public BecaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BecaRequisito>> GetRequisitos(bool soloActivos)
        {
            return await _context.BecasRequisitos
                .AsNoTracking()
                .Where(r => !soloActivos || r.Activo)
                .OrderByDescending(r => r.Activo)
                .ThenBy(r => r.IdRequisito)
                .ToListAsync();
        }

        public async Task<BecaRequisito?> GetRequisito(int idRequisito)
        {
            return await _context.BecasRequisitos
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRequisito == idRequisito);
        }

        public async Task<bool> ExisteNombreRequisito(string nombre, int? excluirIdRequisito = null)
        {
            return await _context.BecasRequisitos
                .AnyAsync(r => r.Nombre == nombre &&
                               (!excluirIdRequisito.HasValue || r.IdRequisito != excluirIdRequisito.Value));
        }

        public async Task CrearRequisito(BecaRequisito requisito)
        {
            _context.BecasRequisitos.Add(requisito);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarRequisito(BecaRequisito requisito)
        {
            _context.BecasRequisitos.Update(requisito);
            await _context.SaveChangesAsync();
        }

        public async Task<Entities.Beca?> GetById(int idBeca)
        {
            return await _context.Becas
                .AsNoTracking()
                .Include(b => b.Estudiante).ThenInclude(e => e!.Usuario)
                .Include(b => b.Periodo)
                .Include(b => b.RequisitosCumplidos)
                .Include(b => b.UsuarioRegistra)
                .Include(b => b.UsuarioModifica)
                .FirstOrDefaultAsync(b => b.IdBeca == idBeca);
        }

        public async Task<Entities.Beca?> GetDeEstudiante(int idEstudiante, int idPeriodo)
        {
            return await _context.Becas
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.IdEstudiante == idEstudiante && b.IdPeriodo == idPeriodo);
        }

        public async Task<List<BecaListado>> Buscar(BecaFiltro filtro, int limite)
        {
            var query = _context.Becas.AsNoTracking().AsQueryable();

            if (filtro.IdPeriodo.HasValue)
                query = query.Where(b => b.IdPeriodo == filtro.IdPeriodo.Value);

            if (filtro.IdEstudiante.HasValue)
                query = query.Where(b => b.IdEstudiante == filtro.IdEstudiante.Value);

            if (filtro.IdUsuarioEstudiante.HasValue)
                query = query.Where(b => b.Estudiante!.IdUsuario == filtro.IdUsuarioEstudiante.Value);

            if (filtro.IdUsuarioEncargado.HasValue)
                query = query.Where(b => b.Estudiante!.EncargadosEstudiantes
                    .Any(ee => ee.Encargado!.IdUsuario == filtro.IdUsuarioEncargado.Value));

            if (!string.IsNullOrWhiteSpace(filtro.Texto))
            {
                foreach (string palabra in filtro.Texto.Split(' ',
                             StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    string p = palabra;

                    query = query.Where(b =>
                        b.Estudiante!.Usuario!.Nombre.Contains(p) ||
                        b.Estudiante.Usuario.Apellido.Contains(p) ||
                        b.Estudiante.Identificacion.Contains(p) ||
                        b.Estudiante.Carnet.Contains(p));
                }
            }

            if (filtro.IdGrado.HasValue)
            {
                int idGrado = filtro.IdGrado.Value;

                query = query.Where(b =>
                    _context.Matriculas.Any(m =>
                        m.IdEstudiante == b.IdEstudiante &&
                        m.IdPeriodo == b.IdPeriodo &&
                        m.Estado != MatriculaAnulada &&
                        m.Seccion!.IdGrado == idGrado) ||
                    (!_context.Matriculas.Any(m =>
                        m.IdEstudiante == b.IdEstudiante &&
                        m.IdPeriodo == b.IdPeriodo &&
                        m.Estado != MatriculaAnulada) &&
                     b.Estudiante!.IdGrado == idGrado));
            }

            DateOnly hoy = filtro.Hoy;

            query = filtro.Estado switch
            {
                EstadoVisibleBeca.Activa => query.Where(b => b.Estado == EstadoBeca.Activa && b.FechaFin >= hoy),
                EstadoVisibleBeca.Vencida => query.Where(b => b.Estado == EstadoBeca.Activa && b.FechaFin < hoy),
                EstadoVisibleBeca.Pendiente => query.Where(b => b.Estado == EstadoBeca.Pendiente),
                EstadoVisibleBeca.Inactiva => query.Where(b => b.Estado == EstadoBeca.Inactiva),
                _ => query
            };

            return await query
                .OrderByDescending(b => b.Periodo!.FechaInicio)
                .ThenBy(b => b.Estudiante!.Usuario!.Apellido)
                .ThenBy(b => b.Estudiante!.Usuario!.Nombre)
                .Take(limite)
                .Select(b => new
                {
                    Beca = b,
                    Matricula = _context.Matriculas
                        .Where(m => m.IdEstudiante == b.IdEstudiante &&
                                    m.IdPeriodo == b.IdPeriodo &&
                                    m.Estado != MatriculaAnulada)
                        .Select(m => new
                        {
                            m.Estado,
                            Grado = m.Seccion!.Grado!.Nombre,
                            Seccion = m.Seccion.Nombre
                        })
                        .FirstOrDefault()
                })
                .Select(x => new BecaListado
                {
                    IdBeca = x.Beca.IdBeca,
                    IdEstudiante = x.Beca.IdEstudiante,
                    Nombre = x.Beca.Estudiante!.Usuario!.Nombre,
                    Apellido = x.Beca.Estudiante.Usuario.Apellido,
                    Identificacion = x.Beca.Estudiante.Identificacion,
                    Carnet = x.Beca.Estudiante.Carnet,
                    EstudianteActivo = x.Beca.Estudiante.Estado,
                    IdPeriodo = x.Beca.IdPeriodo,
                    NombrePeriodo = x.Beca.Periodo!.Nombre,
                    NombreGrado = x.Matricula != null ? x.Matricula.Grado : x.Beca.Estudiante.Grado!.Nombre,
                    NombreSeccion = x.Matricula != null ? x.Matricula.Seccion : null,
                    EstadoMatricula = x.Matricula != null ? x.Matricula.Estado : null,
                    Monto = x.Beca.Monto,
                    FechaInicio = x.Beca.FechaInicio,
                    FechaFin = x.Beca.FechaFin,
                    Estado = x.Beca.Estado,
                    MotivoDesactivacion = x.Beca.MotivoDesactivacion,
                    IdsRequisitosCumplidos = x.Beca.RequisitosCumplidos.Select(c => c.IdRequisito).ToList()
                })
                .ToListAsync();
        }

        public async Task<string?> GetEstadoMatricula(int idEstudiante, int idPeriodo)
        {
            return await _context.Matriculas
                .AsNoTracking()
                .Where(m => m.IdEstudiante == idEstudiante &&
                            m.IdPeriodo == idPeriodo &&
                            m.Estado != MatriculaAnulada)
                .Select(m => m.Estado)
                .FirstOrDefaultAsync();
        }

        public async Task<(string? Grado, string? Seccion)> GetUbicacion(int idEstudiante, int idPeriodo)
        {
            var matricula = await _context.Matriculas
                .AsNoTracking()
                .Where(m => m.IdEstudiante == idEstudiante &&
                            m.IdPeriodo == idPeriodo &&
                            m.Estado != MatriculaAnulada)
                .Select(m => new { Grado = m.Seccion!.Grado!.Nombre, Seccion = m.Seccion.Nombre })
                .FirstOrDefaultAsync();

            if (matricula != null)
                return (matricula.Grado, matricula.Seccion);

            string? grado = await _context.Estudiantes
                .AsNoTracking()
                .Where(e => e.IdEstudiante == idEstudiante)
                .Select(e => e.Grado != null ? e.Grado.Nombre : null)
                .FirstOrDefaultAsync();

            return (grado, null);
        }

        public async Task Crear(Entities.Beca beca, IEnumerable<int> idsRequisitosCumplidos)
        {
            foreach (int idRequisito in idsRequisitosCumplidos.Distinct())
                beca.RequisitosCumplidos.Add(new BecaRequisitoCumplido { IdRequisito = idRequisito });

            _context.Becas.Add(beca);
            await _context.SaveChangesAsync();
        }

        public async Task Actualizar(Entities.Beca beca, IEnumerable<int>? idsRequisitosCumplidos)
        {
            var actual = await _context.Becas
                .Include(b => b.RequisitosCumplidos)
                .FirstAsync(b => b.IdBeca == beca.IdBeca);

            _context.Entry(actual).CurrentValues.SetValues(beca);

            if (idsRequisitosCumplidos != null)
            {
                var nuevos = idsRequisitosCumplidos.Distinct().ToHashSet();

                foreach (var cumplido in actual.RequisitosCumplidos.Where(c => !nuevos.Contains(c.IdRequisito)).ToList())
                    _context.BecasRequisitosCumplidos.Remove(cumplido);

                foreach (int idRequisito in nuevos.Where(id => actual.RequisitosCumplidos.All(c => c.IdRequisito != id)))
                    actual.RequisitosCumplidos.Add(new BecaRequisitoCumplido { IdBeca = actual.IdBeca, IdRequisito = idRequisito });
            }

            await _context.SaveChangesAsync();
        }
    }
}
