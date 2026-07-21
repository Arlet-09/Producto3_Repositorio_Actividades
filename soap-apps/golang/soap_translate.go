// soap_translate.go - Cliente SOAP con traducción en Go
package main

import (
	"encoding/json"
	"encoding/xml"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"strings"
)

type SoapEnvelope struct {
	XMLName xml.Name `xml:"Envelope"`
	Body    SoapBody `xml:"Body"`
}

type SoapBody struct {
	NumberToWordsResponse NumberToWordsResponse `xml:"NumberToWordsResponse"`
}

type NumberToWordsResponse struct {
	NumberToWordsResult string `xml:"NumberToWordsResult"`
}

func callSOAP(numero string) (string, error) {
	soapRequest := `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <NumberToWords xmlns="http://www.dataaccess.com/webservicesserver/">
      <ubiNum>` + numero + `</ubiNum>
    </NumberToWords>
  </soap:Body>
</soap:Envelope>`

	req, err := http.NewRequest("POST", "https://www.dataaccess.com/webservicesserver/NumberConversion.wso", strings.NewReader(soapRequest))
	if err != nil {
		return "", err
	}

	req.Header.Set("Content-Type", "text/xml; charset=utf-8")

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	var envelope SoapEnvelope
	err = xml.Unmarshal(body, &envelope)
	if err != nil {
		return "", err
	}

	return envelope.Body.NumberToWordsResponse.NumberToWordsResult, nil
}

func traducir(texto string) (string, error) {
	url := fmt.Sprintf("https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=es&dt=t&q=%s", url.QueryEscape(texto))
	resp, err := http.Get(url)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	body, _ := ioutil.ReadAll(resp.Body)
	var result []interface{}
	err = json.Unmarshal(body, &result)
	if err != nil {
		return "", err
	}

	if arr, ok := result[0].([]interface{}); ok {
		if first, ok := arr[0].([]interface{}); ok {
			return first[0].(string), nil
		}
	}
	return "", fmt.Errorf("error en traducción")
}

func main() {
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		numero := r.URL.Query().Get("n")
		if numero == "" {
			numero = "10"
		}

		resultadoIngles, err := callSOAP(numero)
		if err != nil {
			fmt.Fprintf(w, "Error SOAP: %s", err.Error())
			return
		}

		resultadoEspanol, err := traducir(resultadoIngles)
		if err != nil {
			fmt.Fprintf(w, "Error traducción: %s", err.Error())
			return
		}

		fmt.Fprintf(w, "Numero %s en español: %s", numero, resultadoEspanol)
	})

	fmt.Println("Servidor iniciado en http://localhost:8081")
	fmt.Println("Ejemplo: http://localhost:8081/?n=10")
	log.Fatal(http.ListenAndServe(":8081", nil))
}