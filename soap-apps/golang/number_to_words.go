// number_to_words.go - Conversión local en Go
package main

import (
	"fmt"
	"log"
	"net/http"
	"strconv"
)

func numeroAPalabras(numero int) string {
	unidades := []string{"", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve"}
	decenas := []string{"", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"}
	especiales := map[int]string{
		10: "diez", 11: "once", 12: "doce", 13: "trece", 14: "catorce", 15: "quince",
		16: "dieciseis", 17: "diecisiete", 18: "dieciocho", 19: "diecinueve",
	}

	if numero == 0 {
		return "cero"
	}
	if numero == 100 {
		return "cien"
	}

	if numero < 10 {
		return unidades[numero]
	}
	if numero < 20 {
		return especiales[numero]
	}
	if numero < 100 {
		decena := numero / 10
		unidad := numero % 10
		if unidad == 0 {
			return decenas[decena]
		}
		if decena == 2 {
			return "veinti" + unidades[unidad]
		}
		return decenas[decena] + " y " + unidades[unidad]
	}
	if numero < 1000 {
		centena := numero / 100
		resto := numero % 100
		var resultado string
		if centena == 1 {
			resultado = "ciento"
		} else if centena == 5 {
			resultado = "quinientos"
		} else if centena == 7 {
			resultado = "setecientos"
		} else if centena == 9 {
			resultado = "novecientos"
		} else {
			resultado = unidades[centena] + "cientos"
		}
		if resto > 0 {
			return resultado + " " + numeroAPalabras(resto)
		}
		return resultado
	}

	return "numero muy grande"
}

func main() {
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		numeroStr := r.URL.Query().Get("n")
		if numeroStr == "" {
			numeroStr = "10"
		}
		numero, _ := strconv.Atoi(numeroStr)
		resultado := numeroAPalabras(numero)
		fmt.Fprintf(w, "Numero %d en palabras: %s", numero, resultado)
	})

	fmt.Println("Servidor iniciado en http://localhost:8082")
	fmt.Println("Ejemplo: http://localhost:8082/?n=10")
	log.Fatal(http.ListenAndServe(":8082", nil))
}