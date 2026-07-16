import requests

url = "http://localhost:5069/usuario"

try:
    respuesta = requests.get(url)

    if respuesta.status_code == 200:
        usuarios = respuesta.json()

        print("Usuarios encontrados:")
        for u in usuarios:
            print(f"Id: {u['id']}")
            print(f"Correo: {u['correo']}")
            print(f"Edad: {u['edad']}")
            print(f"Rol: {u['rol']}")
            print("--------------------")
    else:
        print("Error:", respuesta.status_code)

except Exception as e:
    print("Ocurrió un error:", e)