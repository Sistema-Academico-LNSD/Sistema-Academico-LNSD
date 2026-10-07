using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.BLL.Service.Auth;
using ProyectoLNSD_WApp.BLL;
using ProyectoLNSD_WApp.BLL.Email;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.BLL.Service.Auditoria;
using ProyectoLNSD_WApp.BLL.Service.Permiso;
using ProyectoLNSD_WApp.BLL.Service.Rol;
using ProyectoLNSD_WApp.BLL.Service.Usuario;
using ProyectoLNSD_WApp.BLL.Service.Tiquete;
using ProyectoLNSD_WApp.BLL.Service.Grado;
using ProyectoLNSD_WApp.BLL.Service.Seccion;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Repositories.LogAcceso;
using ProyectoLNSD_WApp.DAL.Repositories.Modulo;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;
using ProyectoLNSD_WApp.DAL.Repositories.RolPermiso;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;
using ProyectoLNSD_WApp.DAL.Repositories.UsuarioTokenReset;
using ProyectoLNSD_WApp.BLL.Service.Institucion;
using ProyectoLNSD_WApp.DAL.Repositories.Institucion;
using ProyectoLNSD_WApp.BLL.Service.PeriodoLectivo;
using ProyectoLNSD_WApp.DAL.Repositories.PeriodoLectivo;
using ProyectoLNSD_WApp.BLL.Service.ContenidoSitio;
using ProyectoLNSD_WApp.BLL.Service.AccesoRapido;
using ProyectoLNSD_WApp.DAL.Repositories.AccesoRapido;
using ProyectoLNSD_WApp.DAL.Repositories.ContenidoSitio;
using ProyectoLNSD_WApp.DAL.Repositories.Tiquete;
using ProyectoLNSD_WApp.DAL.Repositories.Grado;
using ProyectoLNSD_WApp.DAL.Repositories.Seccion;
using ProyectoLNSD_WApp.BLL.Service.Curso;
using ProyectoLNSD_WApp.DAL.Repositories.Curso;
using ProyectoLNSD_WApp.BLL.Service.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.Docente;
using ProyectoLNSD_WApp.DAL.Repositories.DocenteCurso;

namespace ProyectoLNSD_WApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ProyectoLNSDConnection")));

            //Inyección de dependencias para repositorios, servicios, etc.

            // Repositorios
            builder.Services.AddScoped<IRolRepository, RolRepository>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioTokenResetRepository, UsuarioTokenResetRepository>();
            builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
            builder.Services.AddScoped<IRolPermisoRepository, RolPermisoRepository>();
            builder.Services.AddScoped<ILogAccesoRepository, LogAccesoRepository>();
            builder.Services.AddScoped<IInstitucionRepository, InstitucionRepository>();
            builder.Services.AddScoped<IPeriodoLectivoRepository, PeriodoLectivoRepository>();
            builder.Services.AddScoped<IContenidoSitioRepository, ContenidoSitioRepository>();
            builder.Services.AddScoped<IAccesoRapidoRepository, AccesoRapidoRepository>();
            builder.Services.AddScoped<ITiqueteRepository, TiqueteRepository>();
            builder.Services.AddScoped<IGradoRepository, GradoRepository>();
            builder.Services.AddScoped<ISeccionRepository, SeccionRepository>();
            builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
            builder.Services.AddScoped<IDocenteCursoRepository, DocenteCursoRepository>();
            

            //Servicios
            builder.Services.AddScoped<IRolService, RolService>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPermisoService, PermisoService>();
            builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
            builder.Services.AddScoped<IInstitucionService, InstitucionService>();
            builder.Services.AddScoped<IPeriodoLectivoService, PeriodoLectivoService>();
            builder.Services.AddScoped<IContenidoSitioService, ContenidoSitioService>();
            builder.Services.AddScoped<IAccesoRapidoService, AccesoRapidoService>();
            builder.Services.AddScoped<ITiqueteService, TiqueteService>();
            builder.Services.AddScoped<IGradoService, GradoService>();
            builder.Services.AddScoped<ISeccionService, SeccionService>();
            builder.Services.AddScoped<ICursoRepository, CursoRepository>();
            builder.Services.AddScoped<ICursoService, CursoService>();
            builder.Services.AddScoped<IDocenteService, DocenteService>();
            builder.Services.AddScoped<IDocenteCuentaService, DocenteCuentaService>();
            builder.Services.AddScoped<IDocenteCursoService, DocenteCursoService>();
            builder.Services.AddScoped<IExpedienteDocenteService, ExpedienteDocenteService>();

            // Seguridad
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();

            // Servicios Terceros
            builder.Services.AddAutoMapper(cfg => { }, typeof(MapeoClases));

            // Autenticación
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                });

            builder.Services.AddAuthorization();

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();   // sirve los archivos subidos en runtime (logo, banners) desde wwwroot/uploads

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}