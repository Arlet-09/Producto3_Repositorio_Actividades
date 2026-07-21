#include <winsock2.h>
#include <ws2tcpip.h>
#include <iostream>
#include <string>
#include <regex>
#include <map>

#pragma comment(lib, "ws2_32.lib")

// Lógica local de conversión
std::string numeroAPalabras(int numero) {
    std::string unidades[] = {"", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve"};
    std::string decenas[] = {"", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"};
    std::map<int, std::string> especiales = {
        {10, "diez"}, {11, "once"}, {12, "doce"}, {13, "trece"}, {14, "catorce"}, {15, "quince"},
        {16, "dieciseis"}, {17, "diecisiete"}, {18, "dieciocho"}, {19, "diecinueve"}
    };
    
    if (numero == 0) return "cero";
    if (numero == 100) return "cien";
    
    if (numero < 10) return unidades[numero];
    if (numero < 20) return especiales[numero];
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
        std::string resultado;
        if (centena == 1) resultado = "ciento";
        else if (centena == 5) resultado = "quinientos";
        else if (centena == 7) resultado = "setecientos";
        else if (centena == 9) resultado = "novecientos";
        else resultado = unidades[centena] + "cientos";
        return resto > 0 ? resultado + " " + numeroAPalabras(resto) : resultado;
    }
    
    return "numero muy grande";
}

int main() {
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        std::cerr << "Error al iniciar Winsock" << std::endl;
        return 1;
    }

    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket == INVALID_SOCKET) {
        std::cerr << "Error al crear socket" << std::endl;
        WSACleanup();
        return 1;
    }

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = INADDR_ANY;
    serverAddr.sin_port = htons(8082); // Puerto 8082

    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Error al hacer bind (¿Puerto 8082 en uso?)" << std::endl;
        closesocket(serverSocket);
        WSACleanup();
        return 1;
    }

    if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
        std::cerr << "Error al escuchar" << std::endl;
        closesocket(serverSocket);
        WSACleanup();
        return 1;
    }

    std::cout << "=== Servidor Web C++ (Conversión Local) Iniciado ===" << std::endl;
    std::cout << "Accede en el navegador: http://localhost:8082/?n=10" << std::endl;
    std::cout << "Presiona Ctrl+C para detener." << std::endl;

    while (true) {
        SOCKET clientSocket = accept(serverSocket, NULL, NULL);
        if (clientSocket == INVALID_SOCKET) continue;

        char buffer[4096] = {0};
        recv(clientSocket, buffer, sizeof(buffer), 0);

        std::string request(buffer);
        std::regex url_regex("/\\?n=(\\d+)");
        std::smatch match;
        
        std::string response_body = "Numero no proporcionado. Usa ?n=10";
        
        if (std::regex_search(request, match, url_regex)) {
            std::string numero = match[1].str();
            int num_int = std::stoi(numero);
            std::string resultado = numeroAPalabras(num_int);
            
            response_body = "<h1>Resultado C++ (Versión 3 - Local)</h1>";
            response_body += "<p><b>Número:</b> " + numero + "</p>";
            response_body += "<p><b>En palabras (Español):</b> " + resultado + "</p>";
        }

        std::string http_response = 
            "HTTP/1.1 200 OK\r\n"
            "Content-Type: text/html; charset=utf-8\r\n"
            "Connection: close\r\n"
            "\r\n"
            + response_body;

        send(clientSocket, http_response.c_str(), http_response.length(), 0);
        closesocket(clientSocket);
    }

    closesocket(serverSocket);
    WSACleanup();
    return 0;
}