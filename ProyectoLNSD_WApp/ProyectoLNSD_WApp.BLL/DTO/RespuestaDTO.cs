namespace ProyectoLNSD_WApp.BLL.DTO

{
    public class RespuestaDTO<T>
    {
        public bool esCorrecto { get; set; }

        public string mensaje { get; set; } = string.Empty;

        public T? Dato { get; set; }

        public int codigo { get; set; }


        public RespuestaDTO()
        {
            esCorrecto = true;
            mensaje = "Operación realizada correctamente";
            codigo = 200;
        }

    }
}
