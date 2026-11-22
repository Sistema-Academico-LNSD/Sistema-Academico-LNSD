using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Repositories.Pauta
{
    public class PautaRepository : IPautaRepository
    {
        private readonly AppDbContext _context;

        public PautaRepository(AppDbContext context)
        {
            _context = context;
        }

        // =====================  EXPEDIENTES  =====================

        public async Task<int> CrearExpediente(PautaExpediente expediente, List<PautaExpedienteEstudiante> estudiantes)
        {
            if (expediente == null)
                return 0;

            try
            {
                // El código es consecutivo por año; el índice único protege contra dos altas simultáneas.
                expediente.Codigo = await GenerarCodigo(expediente.FechaActivacion.Year);
                expediente.FechaCreacion = DateTime.Now;
                expediente.FechaActualizacion = DateTime.Now;

                foreach (var e in estudiantes)
                {
                    e.IdPautaEstudiante = 0;
                    expediente.Estudiantes.Add(e);
                }

                await _context.PautasExpedientes.AddAsync(expediente);
                await _context.SaveChangesAsync();
                return expediente.IdExpediente;
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                return 0;
            }
        }

        private async Task<string> GenerarCodigo(int anio)
        {
            string prefijo = $"PAU-{anio}-";

            var codigos = await _context.PautasExpedientes
                .AsNoTracking()
                .Where(p => p.Codigo.StartsWith(prefijo))
                .Select(p => p.Codigo)
                .ToListAsync();

            int mayor = 0;
            foreach (var c in codigos)
            {
                if (int.TryParse(c.AsSpan(prefijo.Length), out int n) && n > mayor)
                    mayor = n;
            }

            return $"{prefijo}{mayor + 1:0000}";
        }

        public async Task<PautaExpediente?> GetExpediente(int idExpediente)
        {
            return await _context.PautasExpedientes
                .AsNoTracking()
                .Include(p => p.UsuarioResponsable)
                .Include(p => p.Estudiantes)
                .Include(p => p.Formularios)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.IdExpediente == idExpediente);
        }

        public async Task<List<PautaExpediente>> GetExpedientes(PautaExpedienteFiltro filtro)
        {
            var query = _context.PautasExpedientes
                .AsNoTracking()
                .Include(p => p.UsuarioResponsable)
                .Include(p => p.Estudiantes)
                .Include(p => p.Formularios)
                .AsSplitQuery()
                .AsQueryable();

            if (filtro.IdUsuarioResponsable.HasValue)
                query = query.Where(p => p.IdUsuarioResponsable == filtro.IdUsuarioResponsable.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                query = query.Where(p => p.Estado == filtro.Estado);

            if (filtro.NumeroPauta.HasValue)
                query = query.Where(p => p.NumeroPauta == filtro.NumeroPauta.Value);

            if (filtro.Desde.HasValue)
                query = query.Where(p => p.FechaActivacion >= filtro.Desde.Value);

            if (filtro.Hasta.HasValue)
                query = query.Where(p => p.FechaActivacion <= filtro.Hasta.Value);

            if (filtro.IdEstudiante.HasValue)
                query = query.Where(p => p.Estudiantes.Any(e => e.IdEstudiante == filtro.IdEstudiante.Value));

            if (!string.IsNullOrWhiteSpace(filtro.TextoEstudiante))
            {
                string texto = filtro.TextoEstudiante.Trim();
                query = query.Where(p => p.Estudiantes.Any(e => e.NombreCompleto.Contains(texto)));
            }

            return await query
                .OrderByDescending(p => p.FechaActivacion)
                .ThenByDescending(p => p.IdExpediente)
                .ToListAsync();
        }

        public async Task<bool> ReemplazarEstudiantes(int idExpediente, List<PautaExpedienteEstudiante> estudiantes)
        {
            try
            {
                var actuales = await _context.PautasExpedientesEstudiantes
                    .Where(e => e.IdExpediente == idExpediente)
                    .ToListAsync();

                _context.PautasExpedientesEstudiantes.RemoveRange(actuales);

                foreach (var e in estudiantes)
                {
                    e.IdPautaEstudiante = 0;
                    e.IdExpediente = idExpediente;
                    await _context.PautasExpedientesEstudiantes.AddAsync(e);
                }

                var expediente = await _context.PautasExpedientes.FindAsync(idExpediente);
                if (expediente != null)
                    expediente.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                return false;
            }
        }

        public async Task<bool> ActualizarCierreExpediente(PautaExpediente expediente)
        {
            var existente = await _context.PautasExpedientes.FindAsync(expediente.IdExpediente);
            if (existente == null)
                return false;

            existente.Estado = expediente.Estado;
            existente.FechaCierre = expediente.FechaCierre;
            existente.NombreCierre = expediente.NombreCierre;
            existente.VistoBuenoDirector = expediente.VistoBuenoDirector;
            existente.DevolucionRealizada = expediente.DevolucionRealizada;
            existente.ObservacionCierre = expediente.ObservacionCierre;
            existente.FechaActualizacion = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================  FORMULARIOS  =====================

        public async Task<PautaFormulario?> GetFormulario(int idFormulario)
        {
            return await _context.PautasFormularios
                .AsNoTracking()
                .Include(f => f.Expediente)
                .FirstOrDefaultAsync(f => f.IdFormulario == idFormulario);
        }

        public async Task<List<PautaFormulario>> GetFormularios(int idExpediente)
        {
            return await _context.PautasFormularios
                .AsNoTracking()
                .Where(f => f.IdExpediente == idExpediente)
                .OrderBy(f => f.NumeroFormulario)
                .ThenBy(f => f.Secuencia)
                .ToListAsync();
        }

        public async Task<short> SiguienteSecuencia(int idExpediente, byte numeroFormulario)
        {
            var mayor = await _context.PautasFormularios
                .Where(f => f.IdExpediente == idExpediente && f.NumeroFormulario == numeroFormulario)
                .Select(f => (short?)f.Secuencia)
                .MaxAsync();

            return (short)((mayor ?? 0) + 1);
        }

        public async Task<int> CrearFormulario(PautaFormulario formulario)
        {
            if (formulario == null)
                return 0;

            try
            {
                formulario.FechaCreacion = DateTime.Now;
                formulario.FechaActualizacion = DateTime.Now;

                await _context.PautasFormularios.AddAsync(formulario);
                await _context.SaveChangesAsync();
                return formulario.IdFormulario;
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                return 0;
            }
        }

        public async Task<bool> ActualizarFormulario(int idFormulario, string datos, string estado, int idUsuario, DateTime? fechaFinalizacion)
        {
            var existente = await _context.PautasFormularios.FindAsync(idFormulario);
            if (existente == null)
                return false;

            existente.Datos = datos;
            existente.Estado = estado;
            existente.IdUsuarioActualiza = idUsuario;
            existente.FechaActualizacion = DateTime.Now;
            existente.FechaFinalizacion = fechaFinalizacion;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarFormularioBorrador(int idFormulario)
        {
            var existente = await _context.PautasFormularios.FindAsync(idFormulario);

            // Solo se pueden quitar borradores: un formulario en elaboración o finalizado queda en el expediente.
            if (existente == null || existente.Estado != "Borrador")
                return false;

            bool tieneBitacora = await _context.PautasBitacora.AnyAsync(b => b.IdFormulario == idFormulario);
            if (tieneBitacora)
            {
                // La bitácora no se borra: se desvincula el formulario para conservar el historial.
                var registros = await _context.PautasBitacora.Where(b => b.IdFormulario == idFormulario).ToListAsync();
                foreach (var r in registros)
                    r.IdFormulario = null;
            }

            _context.PautasFormularios.Remove(existente);
            return await _context.SaveChangesAsync() > 0;
        }

        // =====================  BITÁCORA  =====================

        public async Task RegistrarBitacora(PautaBitacora registro)
        {
            registro.Fecha = DateTime.Now;
            await _context.PautasBitacora.AddAsync(registro);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PautaBitacora>> GetBitacora(int? idExpediente, int? idUsuario, string? accion, DateTime? desde, DateTime? hasta, int limite = 200)
        {
            var query = _context.PautasBitacora
                .AsNoTracking()
                .Include(b => b.Usuario)
                .Include(b => b.Expediente)
                .AsQueryable();

            if (idExpediente.HasValue)
                query = query.Where(b => b.IdExpediente == idExpediente.Value);

            if (idUsuario.HasValue)
                query = query.Where(b => b.IdUsuario == idUsuario.Value);

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(b => b.Accion == accion);

            if (desde.HasValue)
                query = query.Where(b => b.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(b => b.Fecha <= hasta.Value);

            return await query
                .OrderByDescending(b => b.Fecha)
                .Take(limite)
                .ToListAsync();
        }
    }
}
