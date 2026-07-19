  using CoreWCF;
    using CoreWCF.Configuration;
    using CoreWCF.Description;
    using CalculadoraWcf;

    var builder = WebApplication.CreateBuilder(args);

    // 1. Configurar Kestrel para escuchar expresamente en el puerto 8080
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(8080);
    });

    // 2. Registrar los servicios de CoreWCF en el contenedor de DI
    builder.Services.AddServiceModelServices();
    builder.Services.AddServiceModelMetadata();

    var app = builder.Build();

    // 3. Configurar el pipeline de CoreWCF y mapear el servicio
    app.UseServiceModel(serviceBuilder =>
    {
        serviceBuilder.AddService<Calculadora>();
       
        // Mapea el contrato ICalculadora a la ruta "/cal" usando BasicHttpBinding (SOAP 1.1)
        serviceBuilder.AddServiceEndpoint<Calculadora, ICalculadora>(
            new CoreWCF.BasicHttpBinding(),
            "/cal"
        );
    });

    var smb = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    smb.HttpGetEnabled = true;

    Console.WriteLine("Servicio publicado en: http://localhost:8080/cal");

    app.Run();