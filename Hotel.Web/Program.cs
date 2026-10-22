using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("HotelDB")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'HotelDB'.");

builder.Services.AddDbContext<HotelContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<HabitacionRepositorio>();
builder.Services.AddScoped<ReservaRepositorio>();

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
