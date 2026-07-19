package com.example.cliente; // Cambia esto a com.example.cliente si usaste esa opción

import jakarta.jws.WebMethod;
import jakarta.jws.WebParam;
import jakarta.jws.WebResult;
import jakarta.jws.WebService;

@WebService(targetNamespace = "http://tempuri.org/", name = "ICalculadora")
public interface ICalculadora {

    // 1. Agregamos el action exacto que espera .NET
    @WebMethod(operationName = "Sumar", action = "http://tempuri.org/ICalculadora/Sumar")
    
    // 2. Le decimos a Java que el resultado viene en la etiqueta <SumarResult>
    @WebResult(name = "SumarResult", targetNamespace = "http://tempuri.org/")
    
    double Sumar(
        @WebParam(name = "a", targetNamespace = "http://tempuri.org/") double a, 
        @WebParam(name = "b", targetNamespace = "http://tempuri.org/") double b
    );
}