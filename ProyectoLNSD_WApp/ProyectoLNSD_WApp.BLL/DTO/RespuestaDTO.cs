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
    }
}