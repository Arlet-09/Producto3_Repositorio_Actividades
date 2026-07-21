const express = require("express");
const soap = require("soap");
const translate =
require("@vitalets/google-translate-api");

const app = express();
const PORT = 3001;

console.log("Iniciando servicio SOAP + Traducción...");

app.get("/", (req, res) => {

    const numero = req.query.n;

    const url =
        "https://www.dataaccess.com/webservicesserver/NumberConversion.wso?WSDL";

    soap.createClient(url, (err, client) => {

        if (err)
            return res.status(500).send(err);

        client.NumberToWords(
            { ubiNum: numero },
            async (err, result) => {

                if (err)
                    return res.status(500).send(err);

                const ingles =
                    result.NumberToWordsResult;

                console.log(
                    `Resultado SOAP: ${ingles}`
                );

                try {

                    const traduccion =
                        await translate.translate(
                            ingles,
                            { to: "es" }
                        );

                    console.log(
                        `Traducido: ${traduccion.text}`
                    );

                    res.send(traduccion.text);

                } catch (e) {
                    console.log(e);
                    res.status(500).send(e);
                }
            }
        );
    });
});

app.listen(PORT, () => {
    console.log(
        `Servidor ejecutándose en http://localhost:${PORT}`
    );
});