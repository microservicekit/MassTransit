namespace MassTransit.Conductor.Observers
{
    using System;
    using Automatonymous;
    using ConsumeConfigurators;
    using Courier;
    using Inventory;
    using Saga;
    using SagaConfigurators;


    public class ServiceRegistryEndpointConfigurationObserver :
        IConsumerConfigurationObserver,
        IHandlerConfigurationObserver,
        ISagaConfigurationObserver,
        IActivityConfigurationObserver
    {
        readonly IBusFactoryConfigurator _configurator;
        readonly IReceiveEndpointConfigurator _receiveEndpointConfigurator;
        readonly ServiceRegistry _registry;

        public ServiceRegistryEndpointConfigurationObserver(IBusFactoryConfigurator configurator, ServiceRegistry registry,
            IReceiveEndpointConfigurator receiveEndpointConfigurator)
        {
            _configurator = configurator;
            _registry = registry;
            _receiveEndpointConfigurator = receiveEndpointConfigurator;
        }

        public void ActivityConfigured<TActivity, TArguments>(IExecuteActivityConfigurator<TActivity, TArguments> configurator, Uri compensateAddress)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            _registry.OnConfigureInput<TArguments>(_receiveEndpointConfigurator);
        }

        public void ExecuteActivityConfigured<TActivity, TArguments>(IExecuteActivityConfigurator<TActivity, TArguments> configurator)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            _registry.OnConfigureInput<TArguments>(_receiveEndpointConfigurator);
        }

        public void CompensateActivityConfigured<TActivity, TLog>(ICompensateActivityConfigurator<TActivity, TLog> configurator)
            where TActivity : class, ICompensateActivity<TLog>
            where TLog : class
        {
        }

        public void ConsumerConfigured<TConsumer>(IConsumerConfigurator<TConsumer> configurator)
            where TConsumer : class
        {
        }

        public void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator)
            where TConsumer : class
            where TMessage : class
        {
            _registry.OnConfigureInput<TMessage>(_receiveEndpointConfigurator);
        }

        public void HandlerConfigured<TMessage>(IHandlerConfigurator<TMessage> configurator)
            where TMessage : class
        {
        }

        public void SagaConfigured<TSaga>(ISagaConfigurator<TSaga> configurator)
            where TSaga : class, ISaga
        {
        }

        public void StateMachineSagaConfigured<TInstance>(ISagaConfigurator<TInstance> configurator, SagaStateMachine<TInstance> stateMachine)
            where TInstance : class, ISaga, SagaStateMachineInstance
        {
        }

        public void SagaMessageConfigured<TSaga, TMessage>(ISagaMessageConfigurator<TSaga, TMessage> configurator)
            where TSaga : class, ISaga
            where TMessage : class
        {
            _registry.OnConfigureInput<TMessage>(_receiveEndpointConfigurator);
        }
    }
}
