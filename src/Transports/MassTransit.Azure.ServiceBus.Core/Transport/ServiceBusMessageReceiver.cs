﻿namespace MassTransit.Azure.ServiceBus.Core.Transport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Context;
    using Contexts;
    using global::Azure.Messaging.ServiceBus;
    using GreenPipes;
    using MassTransit.Pipeline;
    using Transports;
    using Transports.Metrics;


    public class ServiceBusMessageReceiver :
        IServiceBusMessageReceiver
    {
        readonly ReceiveEndpointContext _context;
        readonly IReceivePipeDispatcher _dispatcher;

        public ServiceBusMessageReceiver(ReceiveEndpointContext context)
        {
            _context = context;

            _dispatcher = context.CreateReceivePipeDispatcher();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("receiver");
            scope.Add("type", "brokeredMessage");

            _context.ReceivePipe.Probe(scope);
        }

        ConnectHandle IReceiveObserverConnector.ConnectReceiveObserver(IReceiveObserver observer)
        {
            return _context.ConnectReceiveObserver(observer);
        }

        ConnectHandle IPublishObserverConnector.ConnectPublishObserver(IPublishObserver observer)
        {
            return _context.ConnectPublishObserver(observer);
        }

        ConnectHandle ISendObserverConnector.ConnectSendObserver(ISendObserver observer)
        {
            return _context.ConnectSendObserver(observer);
        }

        async Task IServiceBusMessageReceiver.Handle(ServiceBusReceivedMessage message, CancellationToken cancellationToken, Action<ReceiveContext> contextCallback)
        {
            var context = new ServiceBusReceiveContext(message, _context);
            contextCallback?.Invoke(context);

            CancellationTokenRegistration registration = default;
            if (cancellationToken.CanBeCanceled)
                registration = cancellationToken.Register(context.Cancel);

            var receiveLock = context.TryGetPayload<MessageLockContext>(out var lockContext)
                ? new ServiceBusReceiveLockContext(lockContext, context)
                : default;

            try
            {
                await _dispatcher.Dispatch(context, receiveLock).ConfigureAwait(false);
            }
            catch(ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.SessionLockLost)
            {
                LogContext.Warning?.Log(ex, "Session Lock Lost: {MessageId}", message.MessageId);

                if (_context.ReceiveObservers.Count > 0)
                    await _context.ReceiveObservers.ReceiveFault(context, ex).ConfigureAwait(false);

                throw;
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessageLockLost)
            {
                LogContext.Warning?.Log(ex, "Message Lock Lost: {MessageId}", message.MessageId);

                if (_context.ReceiveObservers.Count > 0)
                    await _context.ReceiveObservers.ReceiveFault(context, ex).ConfigureAwait(false);

                throw;
            }
            finally
            {
                registration.Dispose();
                context.Dispose();
            }
        }

        ConnectHandle IConsumeMessageObserverConnector.ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer)
        {
            return _context.ReceivePipe.ConnectConsumeMessageObserver(observer);
        }

        ConnectHandle IConsumeObserverConnector.ConnectConsumeObserver(IConsumeObserver observer)
        {
            return _context.ReceivePipe.ConnectConsumeObserver(observer);
        }

        public int ActiveDispatchCount => _dispatcher.ActiveDispatchCount;

        public long DispatchCount => _dispatcher.DispatchCount;

        public int MaxConcurrentDispatchCount => _dispatcher.MaxConcurrentDispatchCount;

        public event ZeroActiveDispatchHandler ZeroActivity
        {
            add => _dispatcher.ZeroActivity += value;
            remove => _dispatcher.ZeroActivity -= value;
        }

        public DeliveryMetrics GetMetrics()
        {
            return _dispatcher.GetMetrics();
        }
    }
}
