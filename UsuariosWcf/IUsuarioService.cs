//Logica sin JWT
/*using CoreWCF;
using System.Runtime.Serialization; // requerido por [DataContract] y [DataMember]

namespace UsuariosWcf;

[ServiceContract]
public interface IUsuarioService
{
    [OperationContract]
    Task<List<Usuario>> ObtenerUsuarios(SolicitudAutenticar solicitud);
}

[DataContract]
public record SolicitudAutenticar(
    [property: DataMember] string Correo, 
    [property: DataMember] string Contrasena
);

[DataContract]
public record Usuario(
    [property: DataMember] int Id, 
    [property: DataMember] string Correo, 
    [property: DataMember] string Nombre, 
    [property: DataMember] int Edad, 
    [property: DataMember] string Rol, 
    [property: DataMember] string Contrasenahash
);*/

using CoreWCF;
using System.Runtime.Serialization;

namespace UsuariosWcf;

[ServiceContract]
public interface IUsuarioService
{
    // Ya no requerimos SolicitudAutenticar como parámetro
    [OperationContract]
    Task<List<Usuario>> ObtenerUsuarios(); 
}

[DataContract]
public record Usuario(
    [property: DataMember] int Id,
    [property: DataMember] string Correo,
    [property: DataMember] string Nombre,
    [property: DataMember] int Edad,
    [property: DataMember] string Rol,
    [property: DataMember] string Contrasenahash
);