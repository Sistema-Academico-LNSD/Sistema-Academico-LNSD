using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.BLL;
using ProyectoLNSD_WApp.BLL.Service.Rol;
using ProyectoLNSD_WApp.BLL.Service.Usuario;
using ProyectoLNSD_WApp.DAL.Data;
using ProyectoLNSD_WApp.DAL.Repositories.Rol;
using ProyectoLNSD_WApp.DAL.Repositories.Usuario;


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

            //Inyección de dependencias para repositorios, servicios, etc. Extraer a clase configuracion de servicios para mantener el Program.cs limpio y organizado.
            //Se pueden crear clases estáticas para cada capa (Repositorios, Servicios, etc.) y llamar a sus métodos de configuración desde aquí para una mejor organización.


            // Repositorios
            builder.Services.AddScoped<IRolRepository, RolRepository>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            //Servicios
            builder.Services.AddScoped<IRolService, RolService>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();

            // Servicios Terceros
            builder.Services.AddAutoMapper(cfg => { }, typeof(MapeoClases));


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
