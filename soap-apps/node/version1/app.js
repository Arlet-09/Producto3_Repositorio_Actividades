const express = require("express");
const soap = require("soap");

const app = express();
const PORT = 3000;

console.log("Iniciando servicio Node SOAP...");

app.get("/", (req, res) => {
    const numero = req.query.n;

    console.log(`Petición recibida. Número: ${numero}`);

    const url =
        "https://www.dataaccess.com/webservicesserver/NumberConversion.wso?WSDL";

    soap.createClient(url, (err, client) => {
        if (err) {
            console.log("Error al crear cliente SOAP:", err);
            return res.status(500).send(err);
        }

        client.NumberToWords(
            { ubiNum: numero },
            (err, result) => {

                if (err) {
                    console.log("Error al consumir SOAP:", err);
                    return res.status(500).send(err);
                }

                console.log(
                    `Resultado SOAP: ${result.NumberToWordsResult}`
                );

                res.send(result.NumberToWordsResult);
            }
        );
    });
});

app.listen(PORT, () => {
    console.log(
        `Servidor ejecutándose en http://localhost:${PORT}`
    );
});