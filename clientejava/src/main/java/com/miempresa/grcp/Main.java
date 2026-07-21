package com.miempresa.grpc;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

// Importamos las clases que Maven generó por nosotros
import com.miempresa.grpc.greet.GreeterGrpc;
import com.miempresa.grpc.greet.HelloRequest;
import com.miempresa.grpc.greet.HelloReply;

public class Main {
    public static void main(String[] args) {
        System.out.println("Iniciando cliente gRPC en Java...");

        // 1. Crear el canal conectándonos al servidor .NET (localhost:5159)
        // usamos .usePlaintext() porque nuestro servidor local no tiene certificado SSL
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 5077)
                .usePlaintext()
                .build();

        // 2. Crear el cliente (Stub) que hará la llamada
        GreeterGrpc.GreeterBlockingStub cliente = GreeterGrpc.newBlockingStub(channel);

        // 3. Empaquetar nuestra solicitud (enviando nuestro nombre)
        HelloRequest request = HelloRequest.newBuilder()
                .setName("Luz (desde Java)")
                .build();

        // 4. Hacer la petición al servidor y guardar su respuesta
        try {
            HelloReply respuesta = cliente.sayHello(request);
            System.out.println("Respuesta del servidor .NET: " + respuesta.getMessage());
        } catch (Exception e) {
            System.out.println("Error al comunicarse con el servidor: " + e.getMessage());
        }

        // 5. Cerrar la conexión
        channel.shutdown();
    }
}