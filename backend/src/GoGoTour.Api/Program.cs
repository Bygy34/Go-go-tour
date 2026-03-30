using GoGoTour.Application.Abstractions;
using GoGoTour.Application.Services;
using GoGoTour.Infrastructure.Mocking;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton(typeof(IGenericRepository<>), typeof(InMemoryGenericRepository<>));

builder.Services.AddScoped<TourService>();
builder.Services.AddScoped<BookingService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
