using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Estudiante
{
    public class EstudianteRepository : IEstudianteRepository
    {
        private const string PrefijoCarnet = "LNSD";
        private const int IntentosCarnet = 3;

        private readonly AppDbContext _context;

        public EstudianteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Estudiante>> Buscar(string? texto, int? idGrado, bool? estado)
        {
            var query = _context.Estudiantes
                .AsNoTracking()
                .Include(e => e.Usuario)
                .Include(e => e.Grado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                // Cada palabra debe coincidir con alguno de los campos (nombre, apellido,
                // identificación, correo, carné o datos de un encargado), de modo que
                // "Ana Mora" encuentra a Ana Mora aunque nombre y apellido estén separados.
                var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (string palabra in palabras)
                {
                    string p = palabra;

                    query = query.Where(e =>
                        e.Usuario!.Nombre.Contains(p) ||
                        e.Usuario.Apellido.Contains(p) ||
                        e.Usuario.Correo.Contains(p) ||
                        e.Identificacion.Contains(p) ||
                        e.Carnet.Contains(p) ||
                        e.EncargadosEstudiantes.Any(v =>
                            v.Encargado!.Usuario!.Nombre.Contains(p) ||
                            v.Encargado.Usuario.Apellido.Contains(p) ||
                            v.Encargado.Usuario.Correo.Contains(p)));
                }
            }

            if (idGrado.HasValue)
                query = query.Where(e => e.IdGrado == idGrado.Value);

            if (estado.HasValue)
                query = query.Where(e => e.Estado == estado.Value);

            return await query
                .OrderBy(e => e.Usuario!.Apellido)
                .ThenBy(e => e.Usuario!.Nombre)
                .ToListAsync();
        }

        public async Task<Entities.Estudiante?> GetById(int idEstudiante)
        {
            return await _context.Estudiantes
                .AsNoTracking()
                .Include(e => e.Usuario)
                .Include(e => e.Grado)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante);
        }

        public async Task<Entities.Estudiante?> GetByUsuario(int idUsuario)
        {
            return await _context.Estudiantes
                .AsNoTracking()
                .Include(e => e.Usuario)
                .Include(e => e.Grado)
                .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);
        }

        public async Task<bool> ExisteIdentificacion(string identificacion, int? excluirIdEstudiante = null)
        {
            return await _context.Estudiantes.AnyAsync(e =>
                e.Identificacion == identificacion &&
                (!excluirIdEstudiante.HasValue || e.IdEstudiante != excluirIdEstudiante.Value));
        }

        public async Task<string?> CrearConUsuario(Entities.Estudiante estudiante, Entities.Usuario usuario)
        {
            int anio = estudiante.FechaIngreso.Year;

            // Dos registros simultáneos podrían calcular el mismo consecutivo: el índice único
            // de carnet lo impide y aquí se reintenta con el siguiente número.
            for (int intento = 1; intento <= IntentosCarnet; intento++)
            {
                await using var transaccion = await _context.Database.BeginTransactionAsync();

                try
                {
                    estudiante.Carnet = await SiguienteCarnet(anio);
                    estudiante.Usuario = usuario;

                    await _context.Estudiantes.AddAsync(estudiante);
                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    return estudiante.Carnet;
                }
                catch (DbUpdateException)
                {
                    await transaccion.RollbackAsync();
                    _context.ChangeTracker.Clear();

                    if (intento == IntentosCarnet)
                        return null;
                }
            }

            return null;
        }

        public async Task<bool> Actualizar(Entities.Estudiante datos, string nombre, string apellido, string correo)
        {
            var existente = await _context.Estudiantes
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.IdEstudiante == datos.IdEstudiante);

            if (existente == null || existente.Usuario == null)
                return false;

            existente.Usuario.Nombre = nombre;
            existente.Usuario.Apellido = apellido;
            existente.Usuario.Correo = correo;

            existente.Identificacion = datos.Identificacion;
            existente.FechaNacimiento = datos.FechaNacimiento;
            existente.FechaIngreso = datos.FechaIngreso;
            existente.Telefono = datos.Telefono;
            existente.Direccion = datos.Direccion;
            existente.CorreoEmergencia = datos.CorreoEmergencia;
            existente.IdGrado = datos.IdGrado;
            existente.EstadoAcademico = datos.EstadoAcademico;
            existente.Alergias = datos.Alergias;
            existente.ObservacionesMedicas = datos.ObservacionesMedicas;
            existente.AdecuacionesEducativas = datos.AdecuacionesEducativas;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CambiarEstado(int idEstudiante, bool estado)
        {
            var existente = await _context.Estudiantes.FindAsync(idEstudiante);

            if (existente == null)
                return false;

            existente.Estado = estado;

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<string> SiguienteCarnet(int anio)
        {
            string prefijo = $"{PrefijoCarnet}-{anio}-";

            string? ultimo = await _context.Estudiantes
                .Where(e => e.Carnet.StartsWith(prefijo))
                .OrderByDescending(e => e.Carnet)
                .Select(e => e.Carnet)
                .FirstOrDefaultAsync();

            int siguiente = 1;

            if (ultimo != null &&
                int.TryParse(ultimo.AsSpan(prefijo.Length), out int numero))
            {
                siguiente = numero + 1;
            }

            return $"{prefijo}{siguiente:D4}";
        }
    }
}
