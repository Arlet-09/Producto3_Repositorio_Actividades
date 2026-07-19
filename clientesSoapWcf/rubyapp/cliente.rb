require 'savon'

puts "Llamando al servicio desde Ruby..."

# 1. Instanciar el cliente usando el WSDL del servicio .NET
client = Savon.client(
  wsdl: "http://localhost:8080/cal?singleWsdl",
  log: false # Cambia a true si quieres ver el XML crudo en consola
)

# Valores de prueba
n1 = 40.0
n2 = 10.0

# 2. Hacer la llamada (Savon convierte :sumar en el XML correcto para .NET)
response = client.call(:sumar, message: { 'a' => n1, 'b' => n2 })

# 3. Extraer el resultado. 
# .NET siempre devuelve la estructura {operacion}Response -> {operacion}Result
resultado = response.body[:sumar_response][:sumar_result]

puts "El resultado de sumar #{n1} + #{n2} es: #{resultado}"