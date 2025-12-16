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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MiRoti
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<MiRotiContext>(options =>
                    options.UseMySql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        new MySqlServerVersion(new Version(10, 4, 32))
                    )
                );
            }
            else
            {
                builder.Services.AddDbContext<MiRotiContext>(options =>
                    options.UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection")
                    )
                );
            }

            // ----------------------------
            // 🔹 MVC y Razor
            // ----------------------------
            builder.Services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new MiRoti.ModelBinders.InvariantDecimalModelBinderProvider());
            });
            builder.Services.AddRazorPages();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            // ----------------------------
            // 🔹 Swagger
            // ----------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ----------------------------
            // 🌐 CORS para acceso desde red
            // ----------------------------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalNetwork", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("https://your-frontend.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

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

            // Esquema mixto: cookies para el panel y JWT para la API según el header Authorization
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "AppScheme";
                options.DefaultAuthenticateScheme = "AppScheme";
                options.DefaultChallengeScheme = "AppScheme";
            })
            .AddPolicyScheme("AppScheme", "Cookie or JWT", options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey("Authorization")
                        ? JwtBearerDefaults.AuthenticationScheme
                        : CookieAuthenticationDefaults.AuthenticationScheme;
            })
            // 🔹 Autenticación por cookies para el panel web
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            })
            // 🔹 Autenticación por JWT para la app móvil
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false; // ⚙️ solo desarrollo
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
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

            app.UseStaticFiles();
            app.UseCors("AllowFrontend");
            app.UseRouting();
            app.UseSession();

            // Autenticacion y autorizacion

            // 🔹 Orden correcto: primero autenticación, luego autorización
            app.UseAuthentication();
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

            app.MapGet("/health", () => Results.Ok("healthy"));

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
            // ▶️ Ejecutar
            // ----------------------------
            app.Run();
        }
    }
}
