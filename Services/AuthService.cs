using System.Threading.Tasks;

namespace MiRoti.Services
{
    public class AuthService
    {
        public async Task<string?> AutenticarAsync(string email, string contrasenia)
        {
            // Por ahora simulamos validación: después se conecta a la base de datos
            if (email == "admin@miroti.com" && contrasenia == "1234")
                return await Task.FromResult("fake-jwt-token");

            return await Task.FromResult<string?>(null);
        }
    }
}
