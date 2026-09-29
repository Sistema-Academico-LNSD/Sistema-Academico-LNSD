using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using System.Net.Mail;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.DAL.Repositories.Institucion;

namespace ProyectoLNSD_WApp.BLL.Service.Institucion
{
    public class InstitucionService : IInstitucionService
    {
        private readonly IInstitucionRepository _institucionRepository;
        private readonly IMapper _mapper;

        public InstitucionService(IInstitucionRepository institucionRepository, IMapper mapper)
        {
            _institucionRepository = institucionRepository;
            _mapper = mapper;
        }

        public async Task<RespuestaDTO<InstitucionDTO?>> GetInstitucion()
        {
            var entidad = await _institucionRepository.GetInstitucion();

            // Aún no se ha registrado nada: no es un error, la vista muestra el formulario vacío.
            if (entidad == null)
                return RespuestaDTO<InstitucionDTO?>.Exito(null, "Aún no hay información institucional registrada");

            return RespuestaDTO<InstitucionDTO?>.Exito(_mapper.Map<InstitucionDTO>(entidad));
        }

        public async Task<RespuestaDTO<InstitucionDTO>> GuardarInstitucion(InstitucionDTO institucion, string? nuevaRutaLogo)
        {
            // Campos obligatorios (HU escenario 3)
            if (string.IsNullOrWhiteSpace(institucion.Nombre) ||
                string.IsNullOrWhiteSpace(institucion.Correo) ||
                string.IsNullOrWhiteSpace(institucion.Telefono))
                return RespuestaDTO<InstitucionDTO>.Error("El nombre, el correo y el teléfono principal son requeridos", 3001);

            // Formato de correo (HU escenario 4)
            if (!MailAddress.TryCreate(institucion.Correo.Trim(), out _))
                return RespuestaDTO<InstitucionDTO>.Error("El correo institucional no tiene un formato válido", 3002);

            var existente = await _institucionRepository.GetInstitucion();
            var entidad = _mapper.Map<DAL.Entities.Institucion>(institucion);

            entidad.Nombre = institucion.Nombre.Trim();
            entidad.Correo = institucion.Correo.Trim();
            entidad.Telefono = institucion.Telefono.Trim();
            entidad.Direccion = institucion.Direccion?.Trim();
            entidad.TelefonoSecundario = institucion.TelefonoSecundario?.Trim();

            // La ruta del logo nunca se toma de lo que manda el cliente:
            // solo se cambia si el controller guardó un archivo nuevo; si no, se conserva el anterior.
            entidad.RutaLogo = nuevaRutaLogo ?? existente?.RutaLogo;

            bool guardado;
            string mensaje;

            if (existente == null)
            {
                guardado = await _institucionRepository.CreateInstitucion(entidad);
                mensaje = "Información institucional registrada correctamente";
            }
            else
            {
                entidad.IdInstitucion = existente.IdInstitucion;
                guardado = await _institucionRepository.UpdateInstitucion(entidad);
                mensaje = "Información institucional actualizada correctamente";
            }

            if (!guardado)
                return RespuestaDTO<InstitucionDTO>.Error("No se pudo guardar la información institucional", 3003);

            return RespuestaDTO<InstitucionDTO>.Exito(_mapper.Map<InstitucionDTO>(entidad), mensaje);
        }
    }
}
