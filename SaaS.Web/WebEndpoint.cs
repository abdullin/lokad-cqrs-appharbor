#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Partition;
using Sample;

namespace SaaS.Web
{
    public sealed class WebEndpoint
    {
        readonly NuclearStorage _store;
        readonly IEnvelopeStreamer _streamer;
        readonly IQueueWriter _writer;


        public WebEndpoint(NuclearStorage store, IEnvelopeStreamer streamer, IQueueWriter writer)
        {
            _store = store;
            _streamer = streamer;
            _writer = writer;
        }


        public void SendOne(ISampleCommand command, string optionalId = null)
        {
            SendMessage(command, optionalId);
        }

        void SendMessage(object command, string optionalId)
        {
            var auth = FormsAuth.GetSessionIdentityFromRequest();


            var envelopeId = optionalId ?? Guid.NewGuid().ToString().ToLowerInvariant();
            var eb = new EnvelopeBuilder(envelopeId);

            if (auth.HasValue)
            {
                eb.AddString("web-user", auth.Value.User.Id.ToString(CultureInfo.InvariantCulture));
                eb.AddString("web-token", auth.Value.Token);
            }
            eb.AddItem(command);

            _writer.PutMessage(_streamer.SaveEnvelopeData(eb.Build()));
        }

        public void PublishOne(ISampleEvent e, string optionalId = null)
        {
            SendMessage(e, optionalId);
        }

        public TSingleton GetSingleton<TSingleton>() where TSingleton : new()
        {
            return _store.GetSingletonOrNew<TSingleton>();
        }

        public Maybe<TEntity> GetView<TEntity>(object key)
        {
            var optional = _store.GetEntity<TEntity>(key);
            return optional.HasValue ? optional.Value : Maybe<TEntity>.Empty;
        }


        public TEntity GetViewOrThrow<TEntity>(object key)
        {
            return GetView<TEntity>(key).ExposeException("Failed to locate view {0} by key {1}", typeof(TEntity), key);
        }
    }
}