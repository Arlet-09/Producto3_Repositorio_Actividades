// NumberToWords - Conversión local en .NET 10
// Ejecutar: dotnet run --urls="http://localhost:5002"
// Acceder en navegador: http://localhost:5002/?n=10

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string NumeroAPalabras(int numero)
{
    string[] unidades = {"", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve"};
    string[] decenas = {"", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"};
    Dictionary<int, string> especiales = new() {
        {10, "diez"}, {11, "once"}, {12, "doce"}, {13, "trece"}, {14, "catorce"}, {15, "quince"},
        {16, "dieciseis"}, {17, "diecisiete"}, {18, "dieciocho"}, {19, "diecinueve"}
    };
    
    if (numero == 0) return "cero";
    if (numero == 100) return "cien";
    
    if (numero < 10) return unidades[numero];
    if (numero < 20) return especiales[numero];
    if (numero < 100)
    {
        int decena = numero / 10;
        int unidad = numero % 10;
        if (unidad == 0) return decenas[decena];
        if (decena == 2) return "veinti" + unidades[unidad];
        return decenas[decena] + " y " + unidades[unidad];
    }
    if (numero < 1000)
    {
        int centena = numero / 100;
        int resto = numero % 100;
        string resultado;
        if (centena == 1) resultado = "ciento";
        else if (centena == 5) resultado = "quinientos";
        else if (centena == 7) resultado = "setecientos";
        else if (centena == 9) resultado = "novecientos";
        else resultado = unidades[centena] + "cientos";
        return resto > 0 ? resultado + " " + NumeroAPalabras(resto) : resultado;
    }
    
    return "numero muy grande";
}

app.MapGet("/", (HttpContext context) => {
    var numeroStr = context.Request.Query["n"].ToString();
    int numero = int.TryParse(numeroStr, out int n) ? n : 10;
    var resultado = NumeroAPalabras(numero);
    return Results.Text($"Numero {numero} en palabras con .NET v3: {resultado}");
});

app.Run();