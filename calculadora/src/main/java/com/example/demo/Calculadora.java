package com.example.demo;

import jakarta.jws.WebMethod;
import jakarta.jws.WebService;

@WebService
public class Calculadora {

  @WebMethod
  public double sumar(double a, double b){ return a + b; }

  @WebMethod
  public double restar(double a, double b){ return a - b; }

  @WebMethod
  public double multiplicar(double a, double b){ return a * b; }

  @WebMethod
  public double dividir(double a, double b){
    if (b == 0) throw new ArithmeticException();
    return a / b;
  }
}
