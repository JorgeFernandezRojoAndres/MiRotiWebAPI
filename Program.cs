using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Interfaces;
using MiRoti.Repositories;
using MiRoti.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            // 🔐 Configuración de autenticación combinada: Cookies (MVC) + JWT (API)
            var jwtSettings = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/Denied";
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured")))
                };
            });


            // 🔓 Agregar autorización (por roles)
            builder.Services.AddAuthorization();

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

            // 🧩 Autenticación y autorización
            app.UseAuthentication();
            app.UseAuthorization();

            // ✅ Rutas MVC (nuevo formato)
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

            // 🧩 === SOLO PARA GENERAR HASHES TEMPORALES ===
            if (args.Contains("--hash"))
            {
                Console.WriteLine("=== Generador de Hashes BCrypt ===\n");
                string[] contrasenias = { "admin123", "chef123", "cliente123", "cadete123" };

                foreach (var pass in contrasenias)
                {
                    string hash = BCrypt.Net.BCrypt.HashPassword(pass);
                    Console.WriteLine($"{pass} -> {hash}");
                }

                Console.WriteLine("\n💡 Copiá los hashes y pegá en tu base con UPDATE Usuario ...");
                return; // 🔚 evita ejecutar el servidor web
            }

            app.Run();
        }
    }
}
