using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;

namespace ProyectoLNSD_WApp.Controllers;

[Authorize]
public class MatriculasController : Controller
{
    [HttpGet]
    [RequierePermiso("Matricula", "Ver")]
    public IActionResult Index() => View();
}
