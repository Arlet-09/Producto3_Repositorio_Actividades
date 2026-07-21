import spark.Spark;
import java.net.HttpURLConnection;
import java.net.URL;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;

public class SoapClient {
    
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
        
        // Buscar cualquier tag que contenga el resultado
        // El formato es: <m:NumberToWordsResult>ten</m:NumberToWordsResult>
        int start = responseStr.indexOf(">") + 1;
        while (start > 0 && start < responseStr.length()) {
            int end = responseStr.indexOf("<", start);
            if (end == -1) break;
            
            String content = responseStr.substring(start, end).trim();
            if (!content.isEmpty() && !content.contains("xmlns") && !content.contains("soap") 
                && !content.contains("NumberToWords") && !content.contains("Envelope") 
                && !content.contains("Body") && content.length() < 100) {
                return content;
            }
            start = responseStr.indexOf(">", end) + 1;
        }
        
        return "Error: no se pudo extraer el resultado de: " + responseStr;
    }
    
    public static void main(String[] args) throws Exception {
        Spark.get("/", (req, res) -> {
            String numero = req.queryParams("n");
            if (numero == null || numero.isEmpty()) numero = "10";
            
            try {
                String resultado = callSOAP(numero);
                return "Numero " + numero + " en palabras: " + resultado;
            } catch (Exception e) {
                return "Error: " + e.getMessage();
            }
        });
        
        System.out.println("Servidor iniciado en http://localhost:4567");
        System.out.println("Ejemplo: http://localhost:4567/?n=10");
        
        Thread.currentThread().join();
    }
}