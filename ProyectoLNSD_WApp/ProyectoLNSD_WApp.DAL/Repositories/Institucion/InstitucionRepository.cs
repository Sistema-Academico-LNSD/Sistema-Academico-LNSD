using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.DAL.Repositories.Institucion
{
    public class InstitucionRepository : IInstitucionRepository
    {
        private readonly AppDbContext _context;

        public InstitucionRepository(AppDbContext context)
        {
            _context = context;
        }

        // La institución es una sola fila: se toma la primera.
        public async Task<Entities.Institucion?> GetInstitucion()
        {
            return await _context.Instituciones
                .AsNoTracking()
                .OrderBy(i => i.IdInstitucion)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateInstitucion(Entities.Institucion institucion)
        {
            if (institucion == null)
                return false;

            institucion.FechaActualizacion = DateTime.UtcNow;
            await _context.Instituciones.AddAsync(institucion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateInstitucion(Entities.Institucion institucion)
        {
            if (institucion == null)
                return false;

            var existing = await _context.Instituciones.FindAsync(institucion.IdInstitucion);
            if (existing == null)
                return false;

            existing.Nombre = institucion.Nombre;
            existing.Direccion = institucion.Direccion;
            existing.Telefono = institucion.Telefono;
            existing.TelefonoSecundario = institucion.TelefonoSecundario;
            existing.Correo = institucion.Correo;
            existing.RutaLogo = institucion.RutaLogo;
            existing.FechaActualizacion = DateTime.UtcNow;   // siempre cambia, así SaveChanges devuelve > 0

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
