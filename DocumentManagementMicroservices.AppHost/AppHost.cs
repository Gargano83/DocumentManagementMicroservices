using DocumentManagementMicroservices.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

#region CONFIGURAZIONE INFRASTRUTTURA (CONTAINERS)
var infrastructure = builder.AddInfrastructure();
#endregion

#region CONFIGURAZIONE MICROSERVIZI
var microservices = builder.AddMicroservices(infrastructure);
#endregion

#region CONFIGURAZIONE API GATEWAY
builder.AddApiGateway(microservices);
#endregion

builder.Build().Run();
