using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Interfaces;
using MiRoti.Repositories;
using MiRoti.Services;

namespace MiRoti
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 Conexión a MySQL
            builder.Services.AddDbContext<MiRotiContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(10, 4, 32))
                )
            );

            // 🔹 MVC y vistas Razor
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSession();

            // 🔹 Swagger (API)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ✅ Inyección de dependencias
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPlatoRepository, PlatoRepository>();
            builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ReporteService>();
            builder.Services.AddScoped<EmailService>();

            var app = builder.Build();

            // 🔹 Manejo de errores
            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            // 🔹 Middleware
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            // ✅ Rutas MVC (nuevo formato, sin UseEndpoints)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}"
            );

            app.MapRazorPages();

            // 🔹 Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // 🔹 Redirigir raíz "/" → /Auth/Login
            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Auth/Login");
                return Task.CompletedTask;
            });

            // ✅ Inicializar base de datos
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MiRotiContext>();
                DbInitializer.Initialize(context);
            }

            app.Run();
        }
    }
}
