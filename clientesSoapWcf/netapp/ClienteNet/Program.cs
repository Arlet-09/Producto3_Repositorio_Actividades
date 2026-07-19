using System.ServiceModel;

namespace ClienteDotNet;

// 1. Replicamos el contrato del servidor (interfaz)
// .NET usa http://tempuri.org/ por defecto si no se especifica uno en el servidor
[ServiceContract(Namespace = "http://tempuri.org/")]
public interface ICalculadora
{
    [OperationContract]
    double Sumar(double a, double b);
}

class Program
{
    static void Main(string[] args)
    {
        // 2. Configurar la conexión al servicio
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress("http://localhost:8080/cal");

        // 3. Crear la fábrica de canales y el cliente
        var factory = new ChannelFactory<ICalculadora>(binding, endpoint);
        var cliente = factory.CreateChannel();

        // 4. Consumir el servicio
        Console.WriteLine("Llamando al servicio desde .NET...");
        double n1 = 15.5;
        double n2 = 20.0;
        double resultado = cliente.Sumar(n1, n2);

        Console.WriteLine($"El resultado de sumar {n1} + {n2} es: {resultado}");

        // 5. Cerrar la conexión
        ((IClientChannel)cliente).Close();
    }
}