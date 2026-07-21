import requests
from zeep import Client
from zeep.transports import Transport

# URL de tu servidor CoreWCF
base_url = "http://localhost:8080"

# 1. Hacer Login por REST para obtener el JWT
print("Iniciando login en el servidor CoreWCF...")
credenciales = {
    "Correo": "juan@edu.mx",
    "Contrasena": "juan123"
}

respuesta_login = requests.post(f"{base_url}/login", json=credenciales)

if respuesta_login.status_code == 200:
    datos_login = respuesta_login.json()
    token = datos_login.get("token")

    print(f"\nTu token JWT es:\n{token}\n")
    
    print("¡Token JWT obtenido exitosamente!\n")

    # 2. Configurar la sesión HTTP para inyectar el token en el cliente SOAP
    session = requests.Session()
    session.headers.update({"Authorization": f"Bearer {token}"})
    
    # Enlazar la sesión al transporte de Zeep
    transport = Transport(session=session)
    
    # 3. Conectar al servicio SOAP
    wsdl = f"{base_url}/usu?singleWsdl"
    print("Conectando al servicio SOAP...")
    
    try:
        cliente_soap = Client(wsdl=wsdl, transport=transport)
        
        # Llamar al método ObtenerUsuarios definido en tu IUsuarioService
        usuarios = cliente_soap.service.ObtenerUsuarios()
        
        print("\nLista de Usuarios (SOAP):")
        # Zeep devuelve objetos complejos, los imprimimos directamente
        for u in usuarios:
            print(u)
            
    except Exception as e:
        print(f"Error al consumir el servicio SOAP: {e}")
else:
    print(f"Error de autenticación. Código: {respuesta_login.status_code}")