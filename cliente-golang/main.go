package main

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"io"
	"net/http"
)

// 1. Estructuras actualizadas con los Namespaces exactos de JAX-WS
type Envelope struct {
	XMLName xml.Name `xml:"http://schemas.xmlsoap.org/soap/envelope/ Envelope"`
	Body    Body     `xml:"http://schemas.xmlsoap.org/soap/envelope/ Body"`
}

type Body struct {
	SumarResponse SumarResponse `xml:"http://demo.example.com/ sumarResponse"`
}

type SumarResponse struct {
	Return float64 `xml:"return"`
}

func main() {
	n1 := 10.0
	n2 := 20.0

	// 2. Construir el Envelope SOAP (Petición)
	payload := fmt.Sprintf(`<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:q0="http://demo.example.com/">
  <soapenv:Header/>
  <soapenv:Body>
    <q0:sumar>
      <arg0>%f</arg0>
      <arg1>%f</arg1>
    </q0:sumar>
  </soapenv:Body>
</soapenv:Envelope>`, n1, n2)

	// 3. Crear la petición HTTP POST
	url := "http://localhost:8080/cal"
	req, err := http.NewRequest("POST", url, bytes.NewBufferString(payload))
	if err != nil {
		fmt.Println("Error creando la petición:", err)
		return
	}

	req.Header.Set("Content-Type", "text/xml; charset=utf-8")

	// 4. Enviar la petición al servidor
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Error haciendo la petición SOAP:", err)
		return
	}
	defer resp.Body.Close()

	// 5. Leer el cuerpo de la respuesta
	bodyBytes, err := io.ReadAll(resp.Body)
	if err != nil {
		fmt.Println("Error leyendo la respuesta:", err)
		return
	}

	// ---> NUEVO: Imprimimos la respuesta cruda para entender qué envía Java <---
	fmt.Println("=== RESPUESTA XML DEL SERVIDOR ===")
	fmt.Println(string(bodyBytes))
	fmt.Println("==================================")

	// 6. Decodificar el XML
	var env Envelope
	err = xml.Unmarshal(bodyBytes, &env)
	if err != nil {
		fmt.Println("Error decodificando XML:", err)
		return
	}

	// 7. Mostrar el resultado final
	fmt.Printf("\nPetición exitosa.\n")
	fmt.Printf("El resultado de sumar %v + %v es: %v\n", n1, n2, env.Body.SumarResponse.Return)
}