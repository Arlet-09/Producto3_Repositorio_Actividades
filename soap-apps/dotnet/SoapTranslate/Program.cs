// SoapTranslate - Cliente SOAP con traducción en .NET 10
// Ejecutar: dotnet run --urls="http://localhost:5001"
// Acceder en navegador: http://localhost:5001/?n=10

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async (HttpContext context) => {
    var numero = context.Request.Query["n"].ToString();
    if (string.IsNullOrEmpty(numero)) numero = "10";
    
    try {
        var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
        var endpoint = new EndpointAddress("https://www.dataaccess.com/webservicesserver/NumberConversion.wso");
        var factory = new ChannelFactory<INumberConversion>(binding, endpoint);
        var client = factory.CreateChannel();
        
        var resultadoIngles = await client.NumberToWordsAsync(long.Parse(numero));
        
        // Traducir usando API de Google Translate
        using var httpClient = new HttpClient();
        var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=es&dt=t&q={Uri.EscapeDataString(resultadoIngles)}";
        var response = await httpClient.GetStringAsync(url);
        
        var jsonDoc = JsonDocument.Parse(response);
        var resultadoEspanol = jsonDoc.RootElement[0][0][0].GetString();
        
        return Results.Text($"Numero {numero} en español con .NET: {resultadoEspanol}");
    } catch (Exception ex) {
        return Results.Text($"Error: {ex.Message}");
    }
});

app.Run();

[ServiceContract(Namespace = "http://www.dataaccess.com/webservicesserver/")]
public interface INumberConversion
{
    [OperationContract(Action = "http://www.dataaccess.com/webservicesserver/NumberToWords")]
    Task<string> NumberToWordsAsync(long ubiNum);
}