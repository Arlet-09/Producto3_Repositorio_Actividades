import requests

# URL de tu API REST
base_url = "http://localhost:5000"

# 1. Hacer Login para obtener el JWT
print("Iniciando login...")
credenciales = {
    "Correo": "juan@edu.mx",
    "Contrasena": "juan123"
}

respuesta_login = requests.post(f"{base_url}/login", json=credenciales)

if respuesta_login.status_code == 200:
    # Extraer el token de la respuesta JSON
    datos_login = respuesta_login.json()
    token = datos_login.get("token")
    print("¡Token obtenido exitosamente!\n")
    
    print(f"Tu JWT es:\n{token}\n")
    
    # 2. Consumir el endpoint protegido
    print("Consultando el endpoint /usuario...")
    cabeceras = {
        "Authorization": f"Bearer {token}"
    }
    
    respuesta_usuarios = requests.get(f"{base_url}/usuario", headers=cabeceras)
    
    if respuesta_usuarios.status_code == 200:
        print("Lista de Usuarios obtenida:")
        print(respuesta_usuarios.json())
    else:
        print(f"Error al obtener usuarios: {respuesta_usuarios.status_code}")
else:
    print(f"Error de autenticación. Código: {respuesta_login.status_code}")