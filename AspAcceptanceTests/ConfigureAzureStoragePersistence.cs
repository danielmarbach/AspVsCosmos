using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using Conventions = NServiceBus.AcceptanceTesting.Customization.Conventions;

public class ConfigureEndpointAzureStoragePersistence : IConfigureEndpointTestExecution
{
    static string ConnectionString => Testing.Utillities.GetEnvConfiguredConnectionStringForPersistence();

    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        configuration.UsePersistence<AzureStoragePersistence, StorageType.Sagas>().ConnectionString(ConnectionString);

        return Task.FromResult(0);
    }

    Task IConfigureEndpointTestExecution.Cleanup()
    {
        return Task.FromResult(0);
    }
}