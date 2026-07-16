using Npgsql;
using Dapper;
var builder = WebApplication.CreateBuilder(args);

string cadConexion = builder.Configuration.GetConnectionString("CadConPostgreSql");
builder.Services.AddTransient(sp => new NpgsqlConnection(cadConexion));

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet(
    "/usuario",
    async (NpgsqlConnection con) => { // Inyectar NpgsqlConnection directamente en el endpoint
        // Dapper extiende la conexi├│n con QueryAsync.
        // Mapea autom├íticamente las columnas a las propiedades del record Cliente.
        const string sql = "SELECT * FROM Usuario";
        var usuarios = await con.QueryAsync<Usuario>(sql);
        return Results.Ok(usuarios); // .NET serializa esto a JSON autom├íticamente
    }
);

app.UseCors();

app.Run();

public record Usuario(
    int Id,
    string Correo,
    string Nombre,
    int Edad,
    string Rol,
    string Contrasenahash
);