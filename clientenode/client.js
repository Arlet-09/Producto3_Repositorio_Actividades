const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');

// 1. Cargar el archivo .proto de forma dinámica
const packageDefinition = protoLoader.loadSync('greet.proto', {
    keepCase: true,
    longs: String,
    enums: String,
    defaults: true,
    oneofs: true
});
const greetProto = grpc.loadPackageDefinition(packageDefinition).greet;

// 2. Crear el cliente apuntando al servidor sin seguridad (insecure)
const client = new greetProto.Greeter(
    'localhost:5077', 
    grpc.credentials.createInsecure()
);

// 3. Hacer la llamada al servidor
client.SayHello({ name: 'Juan desde Node.js' }, (error, response) => {
    if (error) {
        console.error('Error:', error);
    } else {
        console.log('Respuesta del servidor:', response.message);
    }
});