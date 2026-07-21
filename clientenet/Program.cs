using Grpc.Net.Client;
using Greet; // El namespace generado a partir del paquete 'greet' en el proto

// Como el servidor local no usa HTTPS por defecto en esta plantilla, permitimos HTTP inseguro
using var channel = GrpcChannel.ForAddress("http://localhost:5077");
var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(new HelloRequest { Name = "Juan desde .NET" });
Console.WriteLine("Respuesta del servidor: " + reply.Message);