package com.example.demo;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import jakarta.xml.ws.Endpoint;

@SpringBootApplication
public class DemoApplication {

    private static final String URL = "http://localhost:8080/cal";

    public static void main(String[] args) {
        // SpringApplication.run(DemoApplication.class, args);

        System.out.println("Servicio publicado en: " + URL);
        Endpoint.publish(URL, new Calculadora());
    }
}