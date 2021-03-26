namespace MassTransit.Registration.Futures
{
    using System;
    using Conductor;
    using Internals.Extensions;
    using MassTransit.Futures;
    using Metadata;


    public static class FutureRegistrationCache
    {
        public static void Register(Type futureType, IContainerRegistrar registrar)
        {
            Cached.Instance.GetOrAdd(futureType).Register(registrar);
        }

        public static void AddFuture(IRegistrationConfigurator configurator, Type futureType, Type futureDefinitionType)
        {
            Cached.Instance.GetOrAdd(futureType).AddFuture(configurator, futureDefinitionType);
        }

        public static void AddFuture(this IServiceRegistry registry, Type futureType)
        {
            Cached.Instance.GetOrAdd(futureType).AddFuture(registry);
        }

        static CachedRegistration Factory(Type type)
        {
            if (!type.ClosesType(typeof(Future<,,>), out Type[] types))
                throw new ArgumentException($"Type is not a Future: {TypeMetadataCache.GetShortName(type)}", nameof(type));

            return (CachedRegistration)Activator.CreateInstance(typeof(CachedRegistration<,,,>).MakeGenericType(type, types[0], types[1], types[2]));
        }


        static class Cached
        {
            internal static readonly RegistrationCache<CachedRegistration> Instance = new RegistrationCache<CachedRegistration>(Factory);
        }


        interface CachedRegistration
        {
            void Register(IContainerRegistrar registrar);
            void AddFuture(IRegistrationConfigurator configurator, Type futureDefinitionType);
            void AddFuture(IServiceRegistry registry);
        }


        class CachedRegistration<TFuture, TRequest, TResponse, TFault> :
            CachedRegistration
            where TFuture : Future<TRequest, TResponse, TFault>
            where TRequest : class
            where TResponse : class
            where TFault : class
        {
            public void Register(IContainerRegistrar registrar)
            {
                registrar.RegisterFuture<TFuture>();
            }

            public void AddFuture(IRegistrationConfigurator configurator, Type futureDefinitionType)
            {
                configurator.AddFuture<TFuture>(futureDefinitionType ?? typeof(DefaultFutureDefinition<TFuture>));
            }

            public void AddFuture(IServiceRegistry registry)
            {
                registry.AddStep<TRequest, TResponse>(x => x.Future<TFuture>());
            }
        }
    }
}
