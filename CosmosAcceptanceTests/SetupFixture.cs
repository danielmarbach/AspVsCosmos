namespace NServiceBus.AcceptanceTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using NUnit.Framework;
    using Persistence.CosmosDB;

    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var connectionStringEnvironmentVariableName = "CosmosDBPersistence_ConnectionString";
            var connectionString = GetEnvironmentVariable(connectionStringEnvironmentVariableName,
                fallbackEmulatorConnectionString: "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            ContainerName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileNameWithoutExtension(Path.GetTempFileName())}";

            var builder = new CosmosClientBuilder(connectionString);

            CosmosDbClient = builder.Build();

            await CosmosDbClient.CreateDatabaseIfNotExistsAsync(DatabaseName);
            var database = CosmosDbClient.GetDatabase(DatabaseName);
            await database.CreateContainerIfNotExistsAsync(ContainerName, "/id");

            Container = database.GetContainer(ContainerName);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Container.DeleteContainerStreamAsync();
            CosmosDbClient.Dispose();
        }

        static string GetEnvironmentVariable(string variable, string fallbackEmulatorConnectionString)
        {
            var candidate = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
            var environmentVariableConnectionString = string.IsNullOrWhiteSpace(candidate) ? Environment.GetEnvironmentVariable(variable) : candidate;

            return string.IsNullOrEmpty(environmentVariableConnectionString) ? fallbackEmulatorConnectionString : environmentVariableConnectionString;
        }

        public const string DatabaseName = "CosmosDBPersistence";
        public const string PartitionPathKey = "/id";
        public static string ContainerName;
        public static CosmosClient CosmosDbClient;
        public static Container Container;
    }
}