using System.ServiceModel;
using System.ServiceModel.Channels;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async (HttpContext context) => {
    var numero = context.Request.Query["n"].ToString();
    if (string.IsNullOrEmpty(numero)) numero = "10";
    
    try {
        var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
        var endpoint = new EndpointAddress("https://www.dataaccess.com/webservicesserver/NumberConversion.wso");
        var channelFactory = new ChannelFactory<INumberConversion>(binding, endpoint);
        var client = channelFactory.CreateChannel();
        
        var resultado = await client.NumberToWordsAsync(long.Parse(numero));
        return Results.Text($"Numero {numero} en palabras con .NET: {resultado}");
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