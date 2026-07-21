const express = require("express");

const app = express();
const PORT = 3002;

const numeros = {
    0: "cero",
    1: "uno",
    2: "dos",
    3: "tres",
    4: "cuatro",
    5: "cinco",
    6: "seis",
    7: "siete",
    8: "ocho",
    9: "nueve",
    10: "diez",
    11: "once",
    12: "doce",
    13: "trece",
    14: "catorce",
    15: "quince",
    16: "dieciséis",
    17: "diecisiete",
    18: "dieciocho",
    19: "diecinueve",
    20: "veinte"
};

console.log("Iniciando servicio número a letras...");

app.get("/", (req, res) => {
    const numero = parseInt(req.query.n);

    console.log(`Número recibido: ${numero}`);

    const texto = numeros[numero] || "Número no implementado";

    console.log(`Resultado: ${texto}`);

    res.send(texto);
});

app.listen(PORT, () => {
    console.log(
        `Servidor ejecutándose en http://localhost:${PORT}`
    );
});