using System.Threading.Tasks;

namespace NServiceBus.AcceptanceTests
{
    using AcceptanceTesting.Support;

    public partial class TestSuiteConstraints : IConfigureEndpointTestExecution
    {
        public bool SupportsDtc { get; } = false;
        public bool SupportsCrossQueueTransactions { get; } = true;
        public bool SupportsNativePubSub { get; } = false;
        public bool SupportsNativeDeferral { get; } = false;
        public bool SupportsOutbox { get; } = false;

        public IConfigureEndpointTestExecution CreateTransportConfiguration() => this;

        public IConfigureEndpointTestExecution CreatePersistenceConfiguration() => new ConfigureEndpointAzureStoragePersistence();

        public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings,
            PublisherMetadata publisherMetadata)
        {
            return new ConfigureEndpointAcceptanceTestingTransport(SupportsNativePubSub, SupportsNativeDeferral)
                .Configure(endpointName, configuration, settings, publisherMetadata);
        }

        public Task Cleanup()
        {
            return Task.CompletedTask;
        }
    }
}