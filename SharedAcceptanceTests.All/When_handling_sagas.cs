using System.IO;

namespace NServiceBus.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;
    using System.Threading;

    public class When_handling_sagas : NServiceBusAcceptanceTest
    {
        private const int NumberOfSagas = 100;

        [SetUp]
        public async Task Setup()
        {
            var testRunId = TestContext.CurrentContext.Test.ID;
            var tempDir = Environment.OSVersion.Platform == PlatformID.Win32NT ? @"c:\temp" : Path.GetTempPath();
            var storageDir = Path.Combine(tempDir, testRunId);
            if (Directory.Exists(storageDir))
            {
                Directory.Delete(storageDir, true);
            }

            await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithASaga>((b) =>
                {
                    b.CustomConfig(c =>
                    {
                        c.AssemblyScanner().ExcludeTypes(typeof(EndpointWithASaga.JustASaga));
                        c.SendOnly();
                    });

                    b.When(async (session, ctx) =>
                    {
                        var destination = AcceptanceTesting.Customization.Conventions.EndpointNamingConvention(typeof(EndpointWithASaga));
                        for (var i = 0; i < NumberOfSagas; i++)
                        {
                            await session.Send(destination, new StartSaga1
                            {
                                DataId = Guid.NewGuid()
                            });
                        }

                        ctx.Seeded = true;
                    });
                })
                .Done(c => c.Seeded)
                .Run();
        }

        [Test]
        public async Task Should_work()
        {
            await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithASaga>()
                .Done(c => c.IsDone)
                .Run();
        }

        public class Context : ScenarioContext
        {
            private long counter;
            public bool Seeded { get; set; }
            public long NumberOfSagasCompleted => Interlocked.Read(ref counter);
            public bool IsDone => Interlocked.Read(ref counter) >= NumberOfSagas;

            public void Done()
            {
                Interlocked.Increment(ref counter);
            }
        }

        public class EndpointWithASaga : EndpointConfigurationBuilder
        {
            public EndpointWithASaga()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.LimitMessageProcessingConcurrencyTo(10);
                });
            }

            public class JustASaga : Saga<JustASagaData>, IAmStartedByMessages<StartSaga1>, IHandleMessages<SagaUpdate>
            {
                public Task Handle(StartSaga1 message, IMessageHandlerContext context)
                {
                    Data.DataId = message.DataId;
                    return context.SendLocal(new SagaUpdate
                    {
                        DataId = message.DataId
                    });
                }

                public Task Handle(SagaUpdate message, IMessageHandlerContext context)
                {
                    MarkAsComplete();
                    return context.SendLocal(new SagaDone
                    {
                        DataId = message.DataId
                    });
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<JustASagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSaga1>(m => m.DataId).ToSaga(s => s.DataId);
                    mapper.ConfigureMapping<SagaUpdate>(m => m.DataId).ToSaga(s => s.DataId);
                }
            }

            public class SagaDoneHandler : IHandleMessages<SagaDone>
            {
                public SagaDoneHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(SagaDone message, IMessageHandlerContext context)
                {
                    testContext.Done();
                    return Task.CompletedTask;
                }

                private Context testContext;
            }

            public class JustASagaData : ContainSagaData
            {
                public virtual Guid DataId { get; set; }
            }
        }

        public class StartSaga1 : ICommand
        {
            public Guid DataId { get; set; }
        }

        public class SagaUpdate : ICommand
        {
            public Guid DataId { get; set; }
        }
        public class SagaDone : ICommand
        {
            public Guid DataId { get; set; }
        }
    }
}