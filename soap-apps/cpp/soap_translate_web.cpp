#include <winsock2.h>
#include <ws2tcpip.h>
#include <iostream>
#include <string>
#include <regex>
#include <sstream>
#include <iomanip>
#include <windows.h>
#include <winhttp.h>

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "winhttp.lib")

// Codificar URL para Google Translate
std::string urlEncode(const std::string& value) {
    std::ostringstream escaped;
    escaped.fill('0');
    escaped << std::hex;
    for (char c : value) {
        if (isalnum(c) || c == '-' || c == '_' || c == '.' || c == '~') {
            escaped << c;
        } else {
            escaped << '%' << std::uppercase << std::setw(2) << int((unsigned char)c);
        }
    }
    return escaped.str();
}

// 1. Llamar al servicio SOAP
std::string callSOAP(const std::string& numero) {
    std::string soapRequest = 
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
        "<soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">\r\n"
        "  <soap12:Body>\r\n"
        "    <NumberToWords xmlns=\"http://www.dataaccess.com/webservicesserver/\">\r\n"
        "      <ubiNum>" + numero + "</ubiNum>\r\n"
        "    </NumberToWords>\r\n"
        "  </soap12:Body>\r\n"
        "</soap12:Envelope>";

    HINTERNET hSession = WinHttpOpen(L"SOAP Client", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
    HINTERNET hConnect = WinHttpConnect(hSession, L"www.dataaccess.com", INTERNET_DEFAULT_HTTPS_PORT, 0);
    HINTERNET hRequest = WinHttpOpenRequest(hConnect, L"POST", L"/webservicesserver/NumberConversion.wso", NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);

    std::wstring headers = L"Content-Type: application/soap+xml; charset=utf-8\r\n";
    WinHttpSendRequest(hRequest, headers.c_str(), headers.length(), (LPVOID)soapRequest.c_str(), soapRequest.length(), soapRequest.length(), 0);
    WinHttpReceiveResponse(hRequest, NULL);

    std::string response;
    DWORD dwSize = 0, dwDownloaded = 0;
    char buffer[1024];
    while (WinHttpQueryDataAvailable(hRequest, &dwSize) && dwSize > 0) {
        WinHttpReadData(hRequest, buffer, sizeof(buffer) - 1, &dwDownloaded);
        buffer[dwDownloaded] = '\0';
        response += buffer;
    }

    WinHttpCloseHandle(hRequest);
    WinHttpCloseHandle(hConnect);
    WinHttpCloseHandle(hSession);

    size_t start = response.find("NumberToWordsResult>");
    if (start != std::string::npos) {
        start = response.find(">", start) + 1;
        size_t end = response.find("<", start);
        if (end != std::string::npos) {
            std::string resultado = response.substr(start, end - start);
            size_t first = resultado.find_first_not_of(" \t\n\r");
            size_t last = resultado.find_last_not_of(" \t\n\r");
            if (first != std::string::npos && last != std::string::npos) {
                return resultado.substr(first, last - first + 1);
            }
            return resultado;
        }
    }
    return "Error SOAP";
}

// 2. Traducir usando Google Translate API
std::string traducir(const std::string& texto) {
    std::string encoded = urlEncode(texto);
    std::wstring path = L"/translate_a/single?client=gtx&sl=en&tl=es&dt=t&q=" + std::wstring(encoded.begin(), encoded.end());
    
    HINTERNET hSession = WinHttpOpen(L"Translate", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
    HINTERNET hConnect = WinHttpConnect(hSession, L"translate.googleapis.com", INTERNET_DEFAULT_HTTPS_PORT, 0);
    HINTERNET hRequest = WinHttpOpenRequest(hConnect, L"GET", path.c_str(), NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);
    
    WinHttpSendRequest(hRequest, WINHTTP_NO_ADDITIONAL_HEADERS, 0, WINHTTP_NO_REQUEST_DATA, 0, 0, 0);
    WinHttpReceiveResponse(hRequest, NULL);

    std::string response;
    DWORD dwSize = 0, dwDownloaded = 0;
    char buffer[1024];
    while (WinHttpQueryDataAvailable(hRequest, &dwSize) && dwSize > 0) {
        WinHttpReadData(hRequest, buffer, sizeof(buffer) - 1, &dwDownloaded);
        buffer[dwDownloaded] = '\0';
        response += buffer;
    }

    WinHttpCloseHandle(hRequest);
    WinHttpCloseHandle(hConnect);
    WinHttpCloseHandle(hSession);

    // Parsear JSON simple: [[["traducción","original"...
    size_t first = response.find("\"");
    if (first != std::string::npos) {
        size_t second = response.find("\"", first + 1);
        if (second != std::string::npos) {
            return response.substr(first + 1, second - first - 1);
        }
    }
    return "Error Traducción";
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
    serverAddr.sin_port = htons(8081); // Puerto 8081 para no chocar con la versión 1

    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Error al hacer bind (¿Puerto 8081 en uso?)" << std::endl;
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

    std::cout << "=== Servidor Web C++ (SOAP + Traducción) Iniciado ===" << std::endl;
    std::cout << "Accede en el navegador: http://localhost:8081/?n=10" << std::endl;
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
            
            std::string soap_result = callSOAP(numero);
            std::string translate_result = traducir(soap_result);
            
            response_body = "<h1>Resultado C++ (Versión 2)</h1>";
            response_body += "<p><b>Número:</b> " + numero + "</p>";
            response_body += "<p><b>SOAP (Inglés):</b> " + soap_result + "</p>";
            response_body += "<p><b>Traducido (Español):</b> " + translate_result + "</p>";
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