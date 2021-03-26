namespace MassTransit.Registration.Consumers
{
    using System;
    using ConsumeConfigurators;
    using Definition;


    public interface IConsumerRegistration
    {
        Type ConsumerType { get; }

        void AddConfigureAction<T>(Action<IConsumerConfigurator<T>> configure)
            where T : class, IConsumer;

        void Configure(IReceiveEndpointConfigurator configurator, IConfigurationServiceProvider scopeProvider);

        IConsumerDefinition GetDefinition(IConfigurationServiceProvider provider);
    }
}
