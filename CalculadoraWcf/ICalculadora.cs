using CoreWCF;

namespace CalculadoraWcf;

[ServiceContract]
public interface ICalculadora
{
    [OperationContract]
    double Sumar(double a, double b);
}