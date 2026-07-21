//Lógica sin JWT
/*using Dapper;
using Npgsql;
using CoreWCF; // requerido por FaultException
using BCrypt.Net; 

namespace UsuariosWcf;

public class UsuarioService : IUsuarioService
{
    private readonly string _connectionString;

    public UsuarioService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<List<Usuario>> ObtenerUsuarios(SolicitudAutenticar solicitud)
    {					
		 if (solicitud == null)
            throw new Exception("Solicitud llegó null");
        //Console.WriteLine($"Correo={solicitud.Correo}");
        //Console.WriteLine($"Contraseña={solicitud.Contrasena}");
        // await using var con = await _connectionString.OpenConnectionAsync();
        using var con = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM usuario WHERE correo = @correo";
        var u = await con.QueryFirstOrDefaultAsync<Usuario>(sql, new { correo = solicitud.Correo });
        if (u == null) throw new FaultException("Usuario no autorizado.");
        bool correcta = BCrypt.Net.BCrypt.Verify(solicitud.Contrasena,u.Contrasenahash);
        if (!correcta) throw new FaultException("Usuario no autorizado.");
        if (u.Rol != "Administrador") throw new FaultException("No tiene permisos.");
        var usuarios = await con.QueryAsync<Usuario>("SELECT * FROM usuario");
        return usuarios.ToList();
    }
}*/

using Dapper;
using Npgsql;
using CoreWCF;
using Microsoft.AspNetCore.Authorization; // Importante para [Authorize]

namespace UsuariosWcf;

// Exigimos que haya un JWT válido para acceder a cualquier método de esta clase
[Authorize(Roles = "Administrador")] 
public class UsuarioService : IUsuarioService
{
    private readonly string _connectionString;

    public UsuarioService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Usuario>> ObtenerUsuarios()
    {
        using var con = new NpgsqlConnection(_connectionString);
        var usuarios = await con.QueryAsync<Usuario>("SELECT * FROM usuario");
        return usuarios.ToList();
    }
}