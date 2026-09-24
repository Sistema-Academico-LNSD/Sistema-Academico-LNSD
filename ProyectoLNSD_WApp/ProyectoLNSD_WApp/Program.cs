using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.BLL;
using ProyectoLNSD_WApp.BLL.Email;
using ProyectoLNSD_WApp.BLL.Security;
using ProyectoLNSD_WApp.BLL.Service.Auditoria;
using ProyectoLNSD_WApp.BLL.Service.Permiso;
using ProyectoLNSD_WApp.BLL.Service.Rol;
using ProyectoLNSD_WApp.BLL.Service.Usuario;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Repositories.LogAcceso;
using ProyectoLNSD_WApp.DAL.Repositories.Modulo;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;
using ProyectoLNSD_WApp.DAL.Repositories.RolPermiso;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;
using ProyectoLNSD_WApp.DAL.Repositories.UsuarioTokenReset;


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

            //Servicios
            builder.Services.AddScoped<IRolService, RolService>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddScoped<IPermisoService, PermisoService>();
            builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

            // Seguridad
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

            // Correo (MUSF-01-06)
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

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}