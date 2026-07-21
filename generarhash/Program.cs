// Lógica sin JWT
/*using BCrypt.Net;

Console.Write("Introduce la contraseña para hashear: ");
string password = Console.ReadLine() ?? "";
string passwordHash =  BCrypt.Net.BCrypt.HashPassword(password);
Console.WriteLine("\nHash:");
Console.WriteLine(passwordHash);
if(BCrypt.Net.BCrypt.Verify(password, passwordHash))
{
    Console.WriteLine("Password is correct");
}*/

using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

string cadConexion = builder.Configuration.GetConnectionString("CadConPostgreSql");
builder.Services.AddTransient(sp => new NpgsqlConnection(cadConexion));

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Configuración de JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
app.UseAuthentication(); // Debe ir antes de Authorization
app.UseAuthorization();

// Endpoint para autenticarse y obtener el JWT
app.MapPost("/login", async (SolicitudLogin req, NpgsqlConnection con, IConfiguration config) =>
{
    var sql = "SELECT * FROM usuario WHERE correo = @correo";
    var user = await con.QueryFirstOrDefaultAsync<Usuario>(sql, new { correo = req.Correo });

    // Verificar si existe y si la contraseña (hasheada) coincide
    if (user == null || !BCrypt.Net.BCrypt.Verify(req.Contrasena, user.Contrasenahash))
        return Results.Unauthorized();

    // Generar el token
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Correo),
            new Claim(ClaimTypes.Role, user.Rol)
        }),
        Expires = DateTime.UtcNow.AddHours(2),
        Issuer = config["Jwt:Issuer"],
        Audience = config["Jwt:Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { Token = tokenHandler.WriteToken(token) });
});

// Proteger este endpoint con JWT y requerir rol de Administrador (opcional)
app.MapGet(
    "/usuario",
    async (NpgsqlConnection con) => { 
        const string sql = "SELECT * FROM usuario";
        var usuarios = await con.QueryAsync<Usuario>(sql);
        return Results.Ok(usuarios);
    }
).RequireAuthorization(); // <--- Aquí activamos la protección JWT

app.Run();

public record Usuario(int Id, string Correo, string Nombre, int Edad, string Rol, string Contrasenahash);
public record SolicitudLogin(string Correo, string Contrasena);