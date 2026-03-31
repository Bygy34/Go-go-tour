using GoGoTour.Application.Abstractions;
using GoGoTour.Application.Services;
using GoGoTour.Infrastructure.Mocking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GoGoTour.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddControllersWithViews();
                        services.AddSingleton<InMemoryStore>();
                        services.AddSingleton(typeof(IGenericRepository<>), typeof(InMemoryGenericRepository<>));
                        services.AddScoped<TourService>();
                        services.AddScoped<BookingService>();
                    });

                    webBuilder.Configure((context, app) =>
                    {
                        if (!context.HostingEnvironment.IsDevelopment())
                        {
                            app.UseExceptionHandler("/Home/Error");
                            app.UseHsts();
                        }

                        app.UseHttpsRedirection();
                        app.UseStaticFiles();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllerRoute(
                                name: "default",
                                pattern: "{controller=Home}/{action=Index}/{id?}");
                        });
                    });
                });
        }
    }
}
