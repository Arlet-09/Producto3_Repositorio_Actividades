#include <iostream>
#include <string>
#include <windows.h>
#include <winhttp.h>

#pragma comment(lib, "winhttp.lib")

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

    HINTERNET hSession = WinHttpOpen(L"SOAP Client",
        WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
        WINHTTP_NO_PROXY_NAME,
        WINHTTP_NO_PROXY_BYPASS, 0);

    if (!hSession) return "Error: WinHttpOpen failed";

    HINTERNET hConnect = WinHttpConnect(hSession,
        L"www.dataaccess.com",
        INTERNET_DEFAULT_HTTPS_PORT, 0);

    if (!hConnect) {
        WinHttpCloseHandle(hSession);
        return "Error: WinHttpConnect failed";
    }

    HINTERNET hRequest = WinHttpOpenRequest(hConnect,
        L"POST",
        L"/webservicesserver/NumberConversion.wso",
        NULL,
        WINHTTP_NO_REFERER,
        WINHTTP_DEFAULT_ACCEPT_TYPES,
        WINHTTP_FLAG_SECURE);

    if (!hRequest) {
        WinHttpCloseHandle(hConnect);
        WinHttpCloseHandle(hSession);
        return "Error: WinHttpOpenRequest failed";
    }

    std::wstring headers = L"Content-Type: application/soap+xml; charset=utf-8\r\n";
    BOOL result = WinHttpSendRequest(hRequest,
        headers.c_str(),
        headers.length(),
        (LPVOID)soapRequest.c_str(),
        soapRequest.length(),
        soapRequest.length(),
        0);

    if (!result) {
        WinHttpCloseHandle(hRequest);
        WinHttpCloseHandle(hConnect);
        WinHttpCloseHandle(hSession);
        return "Error: WinHttpSendRequest failed";
    }

    WinHttpReceiveResponse(hRequest, NULL);

    std::string response;
    DWORD dwSize = 0;
    DWORD dwDownloaded = 0;
    char buffer[1024];

    while (WinHttpQueryDataAvailable(hRequest, &dwSize) && dwSize > 0) {
        WinHttpReadData(hRequest, buffer, sizeof(buffer) - 1, &dwDownloaded);
        buffer[dwDownloaded] = '\0';
        response += buffer;
    }

    WinHttpCloseHandle(hRequest);
    WinHttpCloseHandle(hConnect);
    WinHttpCloseHandle(hSession);

    // Extraer resultado - buscar con o sin namespace
    size_t start = response.find("NumberToWordsResult>");
    if (start != std::string::npos) {
        start = response.find(">", start) + 1;
        size_t end = response.find("<", start);
        if (end != std::string::npos) {
            std::string resultado = response.substr(start, end - start);
            // Eliminar espacios en blanco
            size_t first = resultado.find_first_not_of(" \t\n\r");
            size_t last = resultado.find_last_not_of(" \t\n\r");
            if (first != std::string::npos && last != std::string::npos) {
                return resultado.substr(first, last - first + 1);
            }
            return resultado;
        }
    }

    return "Error: no se encontro el resultado";
}

int main() {
    std::cout << "=== Cliente SOAP C++ (WinHTTP) ===" << std::endl;
    std::cout << "Probando llamada SOAP..." << std::endl;
    
    std::string resultado = callSOAP("10");
    std::cout << "Numero 10 en palabras: " << resultado << std::endl;
    
    std::cout << "\nProbando con numero 25..." << std::endl;
    resultado = callSOAP("25");
    std::cout << "Numero 25 en palabras: " << resultado << std::endl;
    
    std::cout << "\nProbando con numero 100..." << std::endl;
    resultado = callSOAP("100");
    std::cout << "Numero 100 en palabras: " << resultado << std::endl;
    
    return 0;
}