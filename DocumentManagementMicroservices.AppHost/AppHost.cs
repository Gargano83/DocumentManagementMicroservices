using DocumentManagementMicroservices.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

#region 1. CONFIGURAZIONE INFRASTRUTTURA (CONTAINERS)
var infrastructure = builder.AddInfrastructure();
#endregion

#region 2. CONFIGURAZIONE MICROSERVIZI
var microservices = builder.AddMicroservices(infrastructure);
#endregion

#region 3. CONFIGURAZIONE API GATEWAY
builder.AddApiGateway(microservices);
#endregion

builder.Build().Run();
