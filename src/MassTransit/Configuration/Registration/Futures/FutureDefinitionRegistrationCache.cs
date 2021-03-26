namespace MassTransit.Registration.Futures
{
    using System;
    using System.Linq;
    using Automatonymous;
    using Consumers;
    using Internals.Extensions;
    using MassTransit.Futures;
    using Metadata;


    public static class FutureDefinitionRegistrationCache
    {
        public static void Register(Type futureDefinitionType, IContainerRegistrar registrar)
        {
            Cached.Instance.GetOrAdd(futureDefinitionType).Register(registrar);
        }

        static CachedRegistration Factory(Type type)
        {
            if (!type.HasInterface(typeof(IFutureDefinition<>)))
                throw new ArgumentException($"The type is not a future definition: {TypeMetadataCache.GetShortName(type)}", nameof(type));

            var futureType = type.GetClosingArguments(typeof(IFutureDefinition<>)).Single();

            return (CachedRegistration)Activator.CreateInstance(typeof(CachedRegistration<,>).MakeGenericType(type, futureType));
        }


        static class Cached
        {
            internal static readonly ContainerRegistrationCache Instance = new ContainerRegistrationCache(Factory);
        }


        class CachedRegistration<TDefinition, TFuture> :
            CachedRegistration
            where TDefinition : class, IFutureDefinition<TFuture>
            where TFuture : MassTransitStateMachine<FutureState>
        {
            public void Register(IContainerRegistrar registrar)
            {
                registrar.RegisterFutureDefinition<TDefinition, TFuture>();
            }
        }
    }
}
