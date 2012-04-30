#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using SaaS.Dispatch.Events;
using SaaS.Partition;

namespace SaaS.Dispatch
{
    /// <summary>
    /// Engine process that coordinates pulling messages from queues and
    /// dispatching them to the specified handlers
    /// </summary>
    public sealed class DispatcherProcess : IEngineProcess
    {
        readonly Action<byte[]> _dispatcher;
        readonly IPartitionInbox _inbox;

        public DispatcherProcess(Action<byte[]> dispatcher, IPartitionInbox inbox)
        {
            _dispatcher = dispatcher;
            _inbox = inbox;
        }

        public void Dispose()
        {
            _disposal.Dispose();
        }

        public void Initialize()
        {
            _inbox.InitIfNeeded();
        }

        readonly CancellationTokenSource _disposal = new CancellationTokenSource();

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ReceiveMessages(token);
                    }
                    catch (ObjectDisposedException)
                    {
                        // suppress
                    }
                }, token);
        }

        void ReceiveMessages(CancellationToken outer)
        {
            using (var source = CancellationTokenSource.CreateLinkedTokenSource(_disposal.Token, outer))
            {
                while (true)
                {
                    MessageTransportContext context;
                    try
                    {
                        if (!_inbox.TakeMessage(source.Token, out context))
                        {
                            // we didn't retrieve message within the token lifetime.
                            // it's time to shutdown the server
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    try
                    {
                        ProcessMessage(context);
                    }
                    catch (ThreadAbortException)
                    {
                        // Nothing. we are being shutdown
                    }
                    catch (Exception ex)
                    {
                        var e = new DispatchRecoveryFailed(ex, context, context.QueueName);
                        SystemObserver.Notify(e);
                    }
                }
            }
        }

        void ProcessMessage(MessageTransportContext context)
        {
            var dispatched = false;
            try
            {
                _dispatcher(context.Unpacked);

                dispatched = true;
            }
            catch (ThreadAbortException)
            {
                // we are shutting down. Stop immediately
                return;
            }
            catch (Exception dispatchEx)
            {
                // if the code below fails, it will just cause everything to be reprocessed later,
                // which is OK (duplication manager will handle this)

                SystemObserver.Notify(new MessageDispatchFailed(context, context.QueueName, dispatchEx));
                // quarantine is atomic with the processing
                _inbox.TryNotifyNack(context);
            }
            if (!dispatched)
                return;
            try
            {
                _inbox.AckMessage(context);
                // 3rd - notify.
                SystemObserver.Notify(new MessageAcked(context));
            }
            catch (ThreadAbortException)
            {
                // nothing. We are going to sleep
            }
            catch (Exception ex)
            {
                // not a big deal. Message will be processed again.
                SystemObserver.Notify(new MessageAckFailed(ex, context));
            }
        }
    }
}