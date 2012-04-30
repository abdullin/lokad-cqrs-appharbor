#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Partition
{
    public sealed class GetEnvelopeResult
    {
        public static readonly GetEnvelopeResult Empty = new GetEnvelopeResult(null, GetEnvelopeResultState.Empty);
        public static readonly GetEnvelopeResult Retry = new GetEnvelopeResult(null, GetEnvelopeResultState.Retry);
        public readonly GetEnvelopeResultState State;
        readonly MessageTransportContext _message;

        GetEnvelopeResult(MessageTransportContext message, GetEnvelopeResultState state)
        {
            _message = message;
            State = state;
        }


        public MessageTransportContext Message
        {
            get
            {
                if (State != GetEnvelopeResultState.Success)
                    throw new InvalidOperationException("State should be in success");
                return _message;
            }
        }

        public static GetEnvelopeResult Success(MessageTransportContext message)
        {
            return new GetEnvelopeResult(message, GetEnvelopeResultState.Success);
        }

        public static GetEnvelopeResult Error()
        {
            return new GetEnvelopeResult(null, GetEnvelopeResultState.Exception);
        }
    }
}