using System;
using System.Linq;
using System.Threading.Tasks;
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

            // ----------------------------
            // 🔹 Conexión a MySQL
            // ----------------------------
            builder.Services.AddDbContext<MiRotiContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(10, 4, 32))
                )
            );

            // ----------------------------
            // 🔹 MVC y Razor
            // ----------------------------
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSession();

            // ----------------------------
            // 🔹 Swagger
            // ----------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ----------------------------
            // ✅ Inyección de dependencias
            // ----------------------------
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPlatoRepository, PlatoRepository>();
            builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ReporteService>();
            builder.Services.AddScoped<EmailService>();

            // ----------------------------
            // 🔐 Autenticación JWT + Cookies
            // ----------------------------
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings.json");
            var jwtIssuer = jwtSection["Issuer"] ?? "MiRotiAPI";
            var jwtAudience = jwtSection["Audience"] ?? "MiRotiMobile";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // ⚙️ solo desarrollo
                options.SaveToken = true; // ✅ guarda el token en contexto Http
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // ✅ elimina tolerancia horaria
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // ----------------------------
            // 🧩 Inicialización base de datos
            // ----------------------------
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MiRotiContext>();
                DbInitializer.Initialize(context);
            }

            // ----------------------------
            // 🌐 Middleware y pipeline
            // ----------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();  // 🔹 importante: antes que UseAuthorization
            app.UseAuthorization();

            // ----------------------------
            // 🚀 Rutas MVC + API
            // ----------------------------
            app.MapControllers(); // ✅ necesario para [ApiController]
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}"
            );
            app.MapRazorPages();

            // 🔁 Redirigir raíz "/" → /Auth/Login
            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Auth/Login");
                return Task.CompletedTask;
            });

            // ----------------------------
            // 🧰 Generador de hashes (opcional)
            // ----------------------------
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
                return;
            }

            // ----------------------------
            // 🌍 Direcciones LAN
            // ----------------------------
            app.Urls.Add("http://192.168.1.35:5000");
            app.Urls.Add("https://0.0.0.0:5001");

            // ----------------------------
            // ▶️ Ejecutar
            // ----------------------------
            app.Run();
        }
    }
}
