namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class RespuestaDTO<T>
    {
        public bool EsCorrecto { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Dato { get; set; }
        public int Codigo { get; set; }

        public RespuestaDTO()
        {
            EsCorrecto = true;
            Mensaje = "Operación realizada correctamente";
            Codigo = 200;
        }

        /// Respuesta exitosa
        public static RespuestaDTO<T> Exito(T? dato = default, string? mensaje = null)
        {
            var respuesta = new RespuestaDTO<T> { Dato = dato };

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                respuesta.Mensaje = mensaje;
            }

            return respuesta;
        }

        /// Respuesta de error
        public static RespuestaDTO<T> Error(string mensaje, int codigo) =>
            new() { EsCorrecto = false, Mensaje = mensaje, Codigo = codigo };
    }
}