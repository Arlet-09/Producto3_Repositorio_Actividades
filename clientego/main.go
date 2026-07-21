package main

import (
	"context"
	"fmt"
	"log"
	"time"

	pb "clientego/greetpb" // Importa la carpeta generada
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

func main() {
	// 1. Conectar al servidor de forma insegura (HTTP/2 sin TLS)
	conn, err := grpc.Dial("localhost:5077", grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Fatalf("No se pudo conectar: %v", err)
	}
	defer conn.Close()

	// 2. Crear el cliente
	c := pb.NewGreeterClient(conn)

	// 3. Contactar al servidor
	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()
	
	r, err := c.SayHello(ctx, &pb.HelloRequest{Name: "Juan desde Go"})
	if err != nil {
		log.Fatalf("Fallo en gRPC: %v", err)
	}
	
	fmt.Printf("Respuesta del servidor: %s\n", r.GetMessage())
}