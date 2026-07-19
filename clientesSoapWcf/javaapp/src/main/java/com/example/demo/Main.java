package com.example.cliente;

import java.net.URL;
import javax.xml.namespace.QName;
import jakarta.xml.ws.Service;

public class Main {
    public static void main(String[] args) throws Exception {
        System.out.println("Llamando al servicio desde Java...");

        // 1. Apuntar a la URL del WSDL
        URL wsdlUrl = new URL("http://localhost:8080/cal?singleWsdl");

        // 2. Definir el QName (Namespace de .NET y nombre de la clase que implementa el servicio)
        // Por defecto en CoreWCF el servicio se llama igual que la clase (Calculadora)
        QName qname = new QName("http://tempuri.org/", "Calculadora");

        // 3. Crear el servicio y obtener el puerto (interfaz)
        Service service = Service.create(wsdlUrl, qname);
        ICalculadora cliente = service.getPort(ICalculadora.class);

        // 4. Consumir el método
        double n1 = 7.5;
        double n2 = 3.2;
        double resultado = cliente.Sumar(n1, n2);

        System.out.println("El resultado de sumar " + n1 + " + " + n2 + " es: " + resultado);
    }
}