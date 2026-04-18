using HotelSync.Worker;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;

var builder = Host.CreateApplicationBuilder(args);

// Registro do Service Bus Client
builder.Services.AddSingleton(s => 
    new ServiceBusClient("Endpoint=sb://localhost/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_Key_Name;UseDevelopmentStorage=true"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
