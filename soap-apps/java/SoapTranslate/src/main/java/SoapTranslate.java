import spark.Spark;
import java.net.HttpURLConnection;
import java.net.URL;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;

public class SoapTranslate {
    
    static String callSOAP(String numero) throws Exception {
        String soapRequest = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">" +
            "<soap12:Body>" +
            "<NumberToWords xmlns=\"http://www.dataaccess.com/webservicesserver/\">" +
            "<ubiNum>" + numero + "</ubiNum>" +
            "</NumberToWords>" +
            "</soap12:Body>" +
            "</soap12:Envelope>";
        
        URL url = new URL("https://www.dataaccess.com/webservicesserver/NumberConversion.wso");
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setRequestMethod("POST");
        conn.setRequestProperty("Content-Type", "application/soap+xml; charset=utf-8");
        conn.setDoOutput(true);
        
        try (OutputStream os = conn.getOutputStream()) {
            os.write(soapRequest.getBytes("UTF-8"));
        }
        
        BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));
        StringBuilder response = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) {
            response.append(line);
        }
        reader.close();
        
        String responseStr = response.toString();
        String[] parts = responseStr.split("<[^>]+>");
        for (String part : parts) {
            String trimmed = part.trim();
            if (!trimmed.isEmpty() && trimmed.matches("[a-zA-Z ]+")) {
                return trimmed;
            }
        }
        
        return "Error: " + responseStr;
    }
    
    static String traducir(String texto) throws Exception {
        String translateUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=es&dt=t&q=" + 
                     java.net.URLEncoder.encode(texto, "UTF-8");
        HttpURLConnection conn = (HttpURLConnection) new URL(translateUrl).openConnection();
        conn.setRequestProperty("User-Agent", "Mozilla/5.0");
        BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));
        StringBuilder response = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) response.append(line);
        reader.close();
        
        String json = response.toString();
        
        // El formato es: [[["traducción","original",...],...],...]
        int start = json.indexOf("[[[\"") + 4;
        int end = json.indexOf("\"", start);
        
        if (start > 3 && end > start) {
            return json.substring(start, end);
        }
        
        return "Error al traducir: " + json;
    }
    
    public static void main(String[] args) throws Exception {
        Spark.port(4568);
        Spark.get("/", (req, res) -> {
            String numero = req.queryParams("n");
            if (numero == null || numero.isEmpty()) numero = "10";
            
            try {
                String resultadoIngles = callSOAP(numero);
                String resultadoEspanol = traducir(resultadoIngles);
                return "Numero " + numero + " en español: " + resultadoEspanol;
            } catch (Exception e) {
                return "Error: " + e.getMessage();
            }
        });
        
        System.out.println("Servidor iniciado en http://localhost:4568");
        System.out.println("Ejemplo: http://localhost:4568/?n=10");
        
        Thread.currentThread().join();
    }
}