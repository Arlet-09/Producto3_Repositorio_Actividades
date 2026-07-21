import spark.Spark;

public class NumberToWords {
    
    static String numeroAPalabras(int numero) {
        String[] unidades = {"", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve"};
        String[] decenas = {"", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"};
        String[] especiales = {"diez", "once", "doce", "trece", "catorce", "quince", "dieciseis", "diecisiete", "dieciocho", "diecinueve"};
        
        if (numero == 0) return "cero";
        if (numero == 100) return "cien";
        
        if (numero < 10) return unidades[numero];
        if (numero < 20) return especiales[numero - 10];
        if (numero < 100) {
            int decena = numero / 10;
            int unidad = numero % 10;
            if (unidad == 0) return decenas[decena];
            if (decena == 2) return "veinti" + unidades[unidad];
            return decenas[decena] + " y " + unidades[unidad];
        }
        if (numero < 1000) {
            int centena = numero / 100;
            int resto = numero % 100;
            String resultado;
            if (centena == 1) resultado = "ciento";
            else if (centena == 5) resultado = "quinientos";
            else if (centena == 7) resultado = "setecientos";
            else if (centena == 9) resultado = "novecientos";
            else resultado = unidades[centena] + "cientos";
            return resto > 0 ? resultado + " " + numeroAPalabras(resto) : resultado;
        }
        
        return "numero muy grande";
    }
    
    public static void main(String[] args) throws Exception {
        Spark.port(4569);
        Spark.get("/", (req, res) -> {
            String numeroStr = req.queryParams("n");
            int numero = numeroStr != null && !numeroStr.isEmpty() ? Integer.parseInt(numeroStr) : 10;
            String resultado = numeroAPalabras(numero);
            return "Numero " + numero + " en palabras: " + resultado;
        });
        
        System.out.println("Servidor iniciado en http://localhost:4569");
        System.out.println("Ejemplo: http://localhost:4569/?n=10");
        
        Thread.currentThread().join();
    }
}