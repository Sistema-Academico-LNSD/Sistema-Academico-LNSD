using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.AccesoRapido;
using ProyectoLNSD_WApp.BLL.Service.ContenidoSitio;
using ProyectoLNSD_WApp.Models;
using System.Diagnostics;

namespace ProyectoLNSD_WApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IContenidoSitioService _contenidoService;
        private readonly IAccesoRapidoService _accesoService;

        public HomeController(IContenidoSitioService contenidoService, IAccesoRapidoService accesoService)
        {
            _contenidoService = contenidoService;
            _accesoService = accesoService;
        }

        // Página de inicio (pública): muestra solo el contenido con estado Publicado.
        public async Task<IActionResult> Index()
        {
            var respuesta = await _contenidoService.GetLandingPublica();
            var landing = respuesta.Dato ?? new LandingDTO();

            // HU 06: accesos rápidos activos que corresponden al rol del usuario en sesión
            if (User.Identity?.IsAuthenticated == true)
            {
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var accesos = await _accesoService.GetActivosParaRoles(roles);
                landing.Accesos = accesos.Dato ?? new List<AccesoRapidoDTO>();
            }

            return View(landing);
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
