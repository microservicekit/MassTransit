namespace MassTransit.Azure.Table.Configurators
{
    using System;
    using System.Collections.Generic;
    using Contexts;
    using GreenPipes;
    using MassTransit.Saga;
    using Microsoft.Azure.Cosmos.Table;
    using Registration;
    using Saga;


    public class AzureTableSagaRepositoryConfigurator<TSaga> :
        IAzureTableSagaRepositoryConfigurator<TSaga>,
        ISpecification
        where TSaga : class, ISaga
    {
        Func<IConfigurationServiceProvider, CloudTable> _connectionFactory;

        Func<IConfigurationServiceProvider, ISagaKeyFormatter<TSaga>> _formatterFactory = provider =>
            new ConstPartitionSagaKeyFormatter<TSaga>(typeof(TSaga).Name);

        /// <summary>
        /// Supply factory for retrieving the Cloud Table.
        /// </summary>
        /// <param name="connectionFactory"></param>
        public void ConnectionFactory(Func<CloudTable> connectionFactory)
        {
            _connectionFactory = provider => connectionFactory();
        }

        /// <summary>
        /// Supply factory for retrieving the Cloud Table.
        /// </summary>
        /// <param name="connectionFactory"></param>
        public void ConnectionFactory(Func<IConfigurationServiceProvider, CloudTable> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Supply factory for retrieving the key formatter.
        /// </summary>
        /// <param name="formatterFactory"></param>
        public void KeyFormatter(Func<ISagaKeyFormatter<TSaga>> formatterFactory)
        {
            _formatterFactory = provider => formatterFactory();
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_connectionFactory == null)
                yield return this.Failure("ConnectionFactory", "must be specified");
        }

        public void Register<T>(ISagaRepositoryRegistrationConfigurator<T> configurator)
            where T : class, ISaga
        {
            configurator.RegisterSingleInstance<ICloudTableProvider<TSaga>>(provider => new ConstCloudTableProvider<TSaga>(_connectionFactory(provider)));
            configurator.RegisterSingleInstance(_formatterFactory);
            configurator.RegisterSagaRepository<T, DatabaseContext<T>, SagaConsumeContextFactory<DatabaseContext<T>, T>,
                AzureTableSagaRepositoryContextFactory<T>>();
        }
    }
}
