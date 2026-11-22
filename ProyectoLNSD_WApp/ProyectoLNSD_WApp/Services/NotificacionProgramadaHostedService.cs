using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProyectoLNSD_WApp.BLL.Service.Notificaciones;
using ProyectoLNSD_WApp.DAL.Data;

namespace ProyectoLNSD_WApp.Services;

/// <summary>
/// Publica automáticamente los registros informativos programados y genera
/// sus notificaciones sin depender de que alguien abra el módulo.
/// </summary>
public sealed class NotificacionProgramadaHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificacionProgramadaHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcesarAsync(stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await ProcesarAsync(stoppingToken);
    }

    private async Task ProcesarAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificaciones = scope.ServiceProvider.GetRequiredService<INotificacionService>();
            var vencidos = await db.InformativoRegistros
                .Where(x => !x.Eliminado && x.Estado == "Programado" && x.FechaPublicacion <= DateTime.Now)
                .ToListAsync(cancellationToken);

            if (vencidos.Count == 0) return;
            foreach (var registro in vencidos)
                registro.Estado = "Publicado";
            await db.SaveChangesAsync(cancellationToken);

            foreach (var registro in vencidos)
                await notificaciones.NotificarInformativo(registro.IdInformativo, "PUBLICADO");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cierre normal de la aplicación.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No fue posible procesar las publicaciones informativas programadas.");
        }
    }
}
