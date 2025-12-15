using BCrypt.Net;

string[] contrasenias = { "admin123", "chef123", "cliente123", "cadete123" };

foreach (var pass in contrasenias)
{
    var hash = BCrypt.Net.BCrypt.HashPassword(pass);
    Console.WriteLine($"{pass} -> {hash}");
}
