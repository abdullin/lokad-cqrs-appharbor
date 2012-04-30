#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Security.Cryptography;
using SaaS.Envelope;
using SaaS.Partition;

namespace SaaS
{
    public enum IdGeneration
    {
        Default,
        HashContent
    }

    public sealed class SimpleMessageSender
    {
        readonly IQueueWriter[] _queues;
        readonly Func<string> _idGenerator;
        readonly IEnvelopeStreamer _streamer;

        public SimpleMessageSender(IEnvelopeStreamer streamer, IQueueWriter[] queues, Func<string> idGenerator = null)
        {
            _queues = queues;
            _idGenerator = idGenerator ?? (() => Guid.NewGuid().ToString());
            _streamer = streamer;

            if (queues.Length == 0)
                throw new InvalidOperationException("There should be at least one queue");
        }

        public SimpleMessageSender(IEnvelopeStreamer streamer, params IQueueWriter[] queues)
            : this(streamer, queues, null) {}

        public void SendOne(object content)
        {
            InnerSendBatch(cb => { }, new[] {content});
        }

        public void SendOne(object content, Action<EnvelopeBuilder> configure)
        {
            InnerSendBatch(configure, new[] {content});
        }


        public void SendBatch(object[] content, IdGeneration id = IdGeneration.Default)
        {
            if (content.Length == 0)
                return;

            InnerSendBatch(cb => { }, content, id);
        }

        public void SendBatch(object[] content, Action<EnvelopeBuilder> builder)
        {
            InnerSendBatch(builder, content);
        }

        public void SendControl(Action<EnvelopeBuilder> builder)
        {
            InnerSendBatch(builder, new object[0]);
        }


        readonly Random _random = new Random();

        string HashContents(Action<EnvelopeBuilder> configure, object[] messageItems)
        {
            var builder = new EnvelopeBuilder("hash");
            builder.OverrideCreatedOnUtc(DateTime.MinValue);

            foreach (var item in messageItems)
            {
                builder.AddItem(item);
            }
            configure(builder);
            var envelope = builder.Build();
            var data = _streamer.SaveEnvelopeData(envelope);
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        void InnerSendBatch(Action<EnvelopeBuilder> configure, object[] messageItems,
            IdGeneration id = IdGeneration.Default)
        {
            string envelopeId;

            switch (id)
            {
                case IdGeneration.Default:
                    envelopeId = _idGenerator();
                    break;
                case IdGeneration.HashContent:
                    envelopeId = HashContents(configure, messageItems);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("id");
            }

            var builder = new EnvelopeBuilder(envelopeId);
            foreach (var item in messageItems)
            {
                builder.AddItem(item);
            }
            configure(builder);
            var envelope = builder.Build();


            var data = _streamer.SaveEnvelopeData(envelope);

            var queue = GetOutboundQueue();
            queue.PutMessage(data);

            SystemObserver.Notify(new EnvelopeSent(queue.Name, envelope.EnvelopeId,
                envelope.Items.Select(x => x.MappedType.Name).ToArray(), envelope.GetAllAttributes()));
        }

        IQueueWriter GetOutboundQueue()
        {
            if (_queues.Length == 1)
                return _queues[0];
            var random = _random.Next(_queues.Length);
            return _queues[random];
        }
    }
}