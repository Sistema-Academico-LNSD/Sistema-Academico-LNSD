using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.ContenidoSitio;
using ProyectoLNSD_WApp.Models;
using System.Diagnostics;

namespace ProyectoLNSD_WApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IContenidoSitioService _contenidoService;

        public HomeController(IContenidoSitioService contenidoService)
        {
            _contenidoService = contenidoService;
        }

        // Página de inicio (pública): muestra solo el contenido con estado Publicado.
        public async Task<IActionResult> Index()
        {
            var respuesta = await _contenidoService.GetLandingPublica();
            return View(respuesta.Dato ?? new LandingDTO());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
