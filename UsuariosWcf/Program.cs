//Lógica sin JWT
/*using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using UsuariosWcf; 

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(8080);
});


string cadConexion = builder.Configuration.GetConnectionString("CadConPostgreSql");
builder.Services.AddTransient(sp => new NpgsqlConnection(cadConexion));

string cadConexion = builder.Configuration.GetConnectionString("CadConPostgreSql") ?? "";
builder.Services.AddTransient<UsuarioService>(provider => new UsuarioService(cadConexion));

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
//builder.Services.AddSingleton<UsuarioService>();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<UsuarioService>();

    serviceBuilder.AddServiceEndpoint<UsuarioService, IUsuarioService>(
        new BasicHttpBinding(),
        "/usu");
});

var smb = app.Services.GetRequiredService<ServiceMetadataBehavior>();
smb.HttpGetEnabled = true;

Console.WriteLine("Servicio publicado en: http://localhost:8080/usu");

app.Run();

public record Usuario(int Id, string Correo, string Nombre, int Edad, string Rol, string Contrasenahash);
public record SolicitudAutenticar(string Correo, string Contrasena);*/

using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using UsuariosWcf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;
using Dapper;
using CoreWCF.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => { options.ListenLocalhost(8080); });

string cadConexion = builder.Configuration.GetConnectionString("CadConPostgreSql") ?? "";
builder.Services.AddTransient<UsuarioService>(provider => new UsuarioService(cadConexion));
builder.Services.AddTransient(sp => new NpgsqlConnection(cadConexion));

// 1. Configurar JWT
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
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// 2. Endpoint REST para Login (Los clientes de PHP llamarán a este primero)
app.MapPost("/login", async (SolicitudLogin req, NpgsqlConnection con, IConfiguration config) =>
{
    var sql = "SELECT * FROM usuario WHERE correo = @correo";
    var user = await con.QueryFirstOrDefaultAsync<UsuarioDb>(sql, new { correo = req.Correo });

    if (user == null || !BCrypt.Net.BCrypt.Verify(req.Contrasena, user.Contrasenahash))
        return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDesc = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Rol)
        }),
        Expires = DateTime.UtcNow.AddHours(2),
        Issuer = config["Jwt:Issuer"],
        Audience = config["Jwt:Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    return Results.Ok(new { Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDesc)) });
});

// 3. Configurar WCF para heredar la seguridad HTTP (JWT)
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<UsuarioService>();

    var binding = new BasicHttpBinding();
    // TransportCredentialOnly le dice a WCF que confíe en la seguridad HTTP subyacente de ASP.NET Core
binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.InheritedFromHost;
    serviceBuilder.AddServiceEndpoint<UsuarioService, IUsuarioService>(binding, "/usu");
});

var smb = app.Services.GetRequiredService<ServiceMetadataBehavior>();
smb.HttpGetEnabled = true;

app.Run();

public record UsuarioDb(int Id, string Correo, string Nombre, int Edad, string Rol, string Contrasenahash);
public record SolicitudLogin(string Correo, string Contrasena);