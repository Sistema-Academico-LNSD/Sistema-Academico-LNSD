namespace ProyectoLNSD_WApp.BLL.DTO
{
    // Lo que muestra la página pública: solo contenido con estado Publicado + datos de contacto.
    // Las siguientes HU (banners, bloques) agregan aquí sus propiedades.
    public class LandingDTO
    {
        public InstitucionDTO? Institucion { get; set; }
        public ContenidoSitioDTO? Mision { get; set; }
        public ContenidoSitioDTO? Vision { get; set; }
        public ContenidoSitioDTO? Historia { get; set; }

        // Bloques de contenido de la landing (HU 04), solo publicados y ordenados.
        public List<ContenidoSitioDTO> Bloques { get; set; } = new();

        // Banners del carrusel (HU 03), solo publicados y ordenados.
        public List<ContenidoSitioDTO> Banners { get; set; } = new();
    }
}
