from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from zeep import Client

app = FastAPI()

client = Client("http://localhost:8080/usu?wsdl")


class Login(BaseModel):
    correo: str
    contrasena: str


@app.post("/usuarios")
def obtener_usuarios(login: Login):

    solicitud = {
        "Correo": login.correo,
        "Contrasena": login.contrasena
    }

    try:
        respuesta = client.service.ObtenerUsuarios(solicitud)

        return [
            {
                "Id": u.Id,
                "Correo": u.Correo,
                "Nombre": u.Nombre,
                "Edad": u.Edad,
                "Rol": u.Rol
            }
            for u in respuesta
        ]

    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))