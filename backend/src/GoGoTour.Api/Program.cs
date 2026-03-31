using GoGoTour.Application.Abstractions;
using GoGoTour.Application.Services;
using GoGoTour.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<AdminAuthOptions>(builder.Configuration.GetSection(AdminAuthOptions.SectionName));

builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<ITourService, MockTourService>();
builder.Services.AddSingleton<IBookingService, MockBookingService>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
