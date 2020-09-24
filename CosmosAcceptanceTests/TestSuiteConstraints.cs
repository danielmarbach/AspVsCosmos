namespace NServiceBus.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting.Support;

    public partial class TestSuiteConstraints : IConfigureEndpointTestExecution
    {
        public bool SupportsDtc { get; } = false;
        public bool SupportsCrossQueueTransactions { get; } = true;
        public bool SupportsNativePubSub { get; } = true;
        public bool SupportsNativeDeferral { get; } = true;
        public bool SupportsOutbox { get; } = false;

        public IConfigureEndpointTestExecution CreateTransportConfiguration()
        {
            return this;
        }

        public IConfigureEndpointTestExecution CreatePersistenceConfiguration()
        {
            return new ConfigureEndpointCosmosDBPersistence();
        }

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